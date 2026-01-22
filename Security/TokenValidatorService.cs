using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using pos_service.Repositories;
using pos_service.Services.Common.Cache;

namespace pos_service.Security
{
    public interface ITokenValidator
    {
        Task ValidateTokenPrincipalAsync(ClaimsPrincipal principal);
    }

    public class TokenValidatorService : ITokenValidator
    {
        private readonly IUserRepository _repo;
        private readonly ICacheService _cache;
        private readonly ILogger<TokenValidatorService> _logger;

        public TokenValidatorService(IUserRepository repo, ICacheService cache, ILogger<TokenValidatorService> logger)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
        }

        public async Task ValidateTokenPrincipalAsync(ClaimsPrincipal principal)
        {
            if (principal == null) throw new UnauthorizedAccessException("Invalid token principal");

            var uuid = principal.FindFirst("uuid")?.Value;
            if (string.IsNullOrEmpty(uuid))
            {
                _logger.LogWarning("Token missing uuid claim");
                throw new UnauthorizedAccessException("Missing user identifier in token");
            }

            var user = await _repo.GetByUuidAsync(uuid);
            if (user == null)
            {
                try { _cache?.Remove(ServiceCacheKey.Users, uuid); } catch { }
                _logger.LogWarning("Token user not found in DB: Uuid={Uuid}", uuid);
                throw new UnauthorizedAccessException("User not found");
            }

            if (!user.IsActive)
            {
                try { _cache?.Remove(ServiceCacheKey.Users, uuid); } catch { }
                _logger.LogWarning("Token user is inactive: Uuid={Uuid}", uuid);
                throw new UnauthorizedAccessException("User is not active");
            }

            //if (!string.IsNullOrEmpty(user.CreatedBy) && user.CreatedBy.Contains("System", StringComparison.OrdinalIgnoreCase))
            //{
            //    try { _cache?.Remove(ServiceCacheKey.Users, uuid); } catch { }
            //    _logger.LogWarning("Token user is a system user and cannot authenticate: Uuid={Uuid}", uuid);
            //    throw new UnauthorizedAccessException("System user cannot authenticate");
            //}
        }
    }
}
