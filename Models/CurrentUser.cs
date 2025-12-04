using pos_service.Models.Enums;
using System;
using System.Security.Claims;

namespace pos_service.Models
{
    public class CurrentUser
    {
            public int Id               { get; set; }
            public string Uuid          { get; set; } = string.Empty;
            public string Name          { get; set; } = string.Empty;
            public string UserName      { get; set; } = string.Empty;
            public UserRole Role        { get; set; }
            public bool IsAuthenticated { get; set; }

            public static CurrentUser FromClaimsPrincipal(ClaimsPrincipal principal)
            {
                if (principal == null || principal.Identity?.IsAuthenticated != true)
                {
                    return new CurrentUser { IsAuthenticated = false };
                }

                try
                {
                    var currentUser = new CurrentUser
                    {
                        IsAuthenticated = true,
                        Id              = GetClaimValue<int>(principal, ClaimTypes.NameIdentifier, "user_id"),
                        Uuid            = GetClaimValue<string>(principal, "uuid") ?? string.Empty,
                        Name            = GetClaimValue<string>(principal, ClaimTypes.Name) ?? string.Empty,
                        UserName        = GetClaimValue<string>(principal, "username") ?? string.Empty,
                        Role            = GetRoleFromClaims(principal)
                    };

                    return currentUser;
                }
                catch (Exception)
                {
                    // In production, you might want to log this
                    return new CurrentUser { IsAuthenticated = false };
                }
            }

            private static T? GetClaimValue<T>(ClaimsPrincipal principal, params string[] claimTypes)
            {
                foreach (var claimType in claimTypes)
                {
                    var claim = principal.FindFirst(claimType);
                    if (claim != null && !string.IsNullOrEmpty(claim.Value))
                    {
                        try
                        {
                            if (typeof(T) == typeof(string))
                                return (T)(object)claim.Value;

                            return (T)Convert.ChangeType(claim.Value, typeof(T));
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                return default;
            }

            private static UserRole GetRoleFromClaims(ClaimsPrincipal principal)
            {
                var roleClaim = principal.FindFirst(ClaimTypes.Role) ?? principal.FindFirst("role");
                if (roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, out var role))
                {
                    return role;
                }
                return UserRole.Cashier;
            }

            public bool IsInRole(params UserRole[] roles)
                => IsAuthenticated && roles.Contains(Role);

            public bool HasPermission(string permission)
                => IsAuthenticated; // Add your permission logic here

            public bool CanManageUsers()
                => IsInRole(UserRole.SystemAdmin, UserRole.Manager);

            public bool CanViewSensitiveData()
                => IsInRole(UserRole.SystemAdmin, UserRole.Manager);
        }
    }
