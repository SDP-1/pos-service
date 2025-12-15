using AutoMapper;
using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Exceptions;
using pos_service.Models;
using pos_service.Models.DTO.Users;
using pos_service.Models.Enums;
using pos_service.Repositories;
using pos_service.Services.Common.Cache;
using pos_service.Services.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace pos_service.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor        _httpContextAccessor;
        private readonly ILogger<CurrentUserService> _logger;
        private readonly IMapper                     _mapper;
        private readonly IPermissionService          _permissionService;
        private readonly AppDbContext                _dbContext;
        private readonly ICacheService               _cacheService;
        private readonly IUserRepository             _userRepository;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CurrentUserService> logger,
            IMapper mapper,
            IPermissionService permissionService,
            ICacheService cacheService,
            IUserRepository userRepository,
            AppDbContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger              = logger;
            _mapper              = mapper;
            _permissionService   = permissionService;
            _dbContext           = dbContext;
            _cacheService        = cacheService;
            _userRepository      = userRepository;
        }

        /// <summary>
        /// Retrieves the current authenticated user's information.
        /// This method does not cache results across requests to avoid leaking data between requests.
        /// </summary>
        /// <returns>The current user details including identity and role information.</returns>
        public CurrentUser GetCurrentUser()
        {
            if (_httpContextAccessor?.HttpContext == null)
            {
                _logger.LogDebug("HttpContext or HttpContextAccessor is null");
                return new CurrentUser { IsAuthenticated = false };
            }

            var principal = _httpContextAccessor.HttpContext.User;
            if (principal?.Identity?.IsAuthenticated != true)
                return new CurrentUser { IsAuthenticated = false };

            try
            {

                // 1. Build minimal current user from claims
                var currentUser = CurrentUser.FromClaimsPrincipal(principal);

                // 2. Try cache by uuidS
                var cached = TryGetCachedUser(currentUser.Uuid);
                if (cached != null)
                {
                    _logger.LogDebug("Current user loaded from cache: Uuid={Uuid}", currentUser.Uuid);
                    return cached;
                }

                // 3. Ensure role metadata is loaded
                LoadRoleForCurrentUser(currentUser);

                // 4. Enrich from DB and validate user
                currentUser = EnrichFromDbOrThrow(currentUser);

                // 5. Load permissions (this will try cache internally)
                currentUser.Permissions = LoadPermissionsForUser(currentUser);

                // 6. Cache final CurrentUser by uuid
                CacheUserIfPossible(currentUser);

                _logger.LogDebug("Current user retrieved: UserId={UserId}, Uuid={Uuid}, UserName={UserName}", currentUser.Id, currentUser.Uuid, currentUser.UserName);
                return currentUser;
            }
            catch (UnauthorizedAccessException)
            {
                // Let authorization exceptions bubble up to be handled
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return new CurrentUser { IsAuthenticated = false };
            }
        }

        // Private helpers to keep the main flow concise and clear
        private CurrentUser? TryGetCachedUser(string? uuid)
        {
            if (string.IsNullOrEmpty(uuid)) return null;
            try
            {
                return _cacheService.Get<CurrentUser>(ServiceCacheKey.Users, uuid);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to read current user cache for Uuid={Uuid}", uuid);
                return null;
            }
        }

        private void LoadRoleForCurrentUser(CurrentUser currentUser)
        {
            try
            {
                var roleId = currentUser.Role?.Id ?? 0;
                if (roleId > 0)
                {
                    var role = _dbContext.Roles.AsNoTracking().FirstOrDefault(r => r.Id == roleId);
                    if (role != null) currentUser.Role = role;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load role details for RoleId={RoleId}", currentUser.Role?.Id ?? 0);
            }
        }

        private CurrentUser EnrichFromDbOrThrow(CurrentUser currentUser)
        {
            if (string.IsNullOrEmpty(currentUser.Uuid))
            {
                _logger.LogWarning("User Uuid null");
                throw new UnauthorizedAccessException("User Uuid null");
            }

            User? userEntity;

            try
            {
                userEntity = _userRepository.GetByUuidAsync(currentUser.Uuid).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error loading user for Uuid={Uuid}", currentUser.Uuid);
                throw new UnauthorizedAccessException("User could not be validated");
            }

            if (userEntity == null)
            {
                _logger.LogWarning("User not found in DB: Uuid={Uuid}", currentUser.Uuid);
                throw new UnauthorizedAccessException("User not found");
            }

            if (!userEntity.IsActive)
            {
                _logger.LogWarning("Inactive user attempted access: Uuid={Uuid}", currentUser.Uuid);
                throw new UnauthorizedAccessException("Inactive user");
            }

            //if (!string.IsNullOrEmpty(userEntity.CreatedBy) && userEntity.CreatedBy.Contains("System", StringComparison.OrdinalIgnoreCase))
            //{
            //    _logger.LogWarning("System user cannot be used as current user: Uuid={Uuid}, CreatedBy={CreatedBy}", uuid, userEntity.CreatedBy);
            //    throw new UnauthorizedAccessException("System user cannot authenticate");
            //}

            var mapped = _mapper.Map<CurrentUser>(userEntity);
            mapped.IsAuthenticated = true;
            return mapped;
        }


        private List<Permission> LoadPermissionsForUser(CurrentUser currentUser)
        {
            List<Permission>? permList = null;

            // Try to read permissions from cache when uuid is available
            if (!string.IsNullOrEmpty(currentUser.Uuid))
            {
                try { permList = _cacheService.Get<List<Permission>>(ServiceCacheKey.Permissions, currentUser.Uuid); }
                catch (Exception ex) { _logger.LogDebug(ex, "Failed to read permissions cache for Uuid={Uuid}", currentUser.Uuid); }
            }

            if (permList == null)
            {
                try
                {
                    var roleId = currentUser.Role?.Id ?? 0;
                    var perms = roleId > 0
                        ? _permissionService.GetPermissionsForRoleAsync(roleId).GetAwaiter().GetResult()
                        : Enumerable.Empty<Permission>();

                    permList = perms.ToList();

                    if (!string.IsNullOrEmpty(currentUser.Uuid))
                    {
                        try { _cacheService.Set(ServiceCacheKey.Permissions, currentUser.Uuid, permList); }
                        catch (Exception ex) { _logger.LogDebug(ex, "Failed to write permissions cache for Uuid={Uuid}", currentUser.Uuid); }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load permissions for role {RoleId}", currentUser.Role?.Id ?? 0);
                    permList = new List<Permission>();
                }
            }

            return permList ?? new List<Permission>();
        }

        private void CacheUserIfPossible(CurrentUser currentUser)
        {
            if (string.IsNullOrEmpty(currentUser.Uuid)) return;
            try 
            { 
                _cacheService.Set(ServiceCacheKey.Users, currentUser.Uuid, currentUser); 
            }
            catch (Exception ex) 
            { 
                _logger.LogDebug(ex, "Failed to write current user cache for Uuid={Uuid}", currentUser.Uuid); 
            }
        }

        // Quick access methods

        /// <summary>
        /// Checks if the current user is in any of the specified roles.
        /// </summary>
        public bool IsInRole(params int[] roles) => GetCurrentUser().IsInRole(roles);

        /// <summary>
        /// Checks if the current user has the specified permission.
        /// </summary>
        public bool HasPermission(PermissionType permission) => GetCurrentUser().HasPermission(permission);

        // Validation methods

        /// <summary>
        /// Ensures that the current user is authenticated.
        /// Throws an exception if the user is not authenticated.
        /// </summary>
        public void EnsureAuthenticated()
        {
            if (!GetCurrentUser().IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");
        }

        /// <summary>
        /// Ensures that the current user has any of the specified roles.
        /// Throws an exception if the user does not have the required roles.
        /// </summary>
        public void EnsureRole(params int[] roles)
        {
            EnsureAuthenticated();
            if (!IsInRole(roles))
                throw new UnauthorizedAccessException($"User does not have required role. Required: {string.Join(", ", roles)}");
        }

        /// <summary>
        /// Ensures that the current user has the specified permission.
        /// Throws an exception if the user does not have the required permission.
        /// </summary>
        public void EnsurePermission(PermissionType permission)
        {
            EnsureAuthenticated();
            if (!HasPermission(permission))
                throw new PermissionDeniedException($"User does not have required permission: {permission}");
        }
    }
}
