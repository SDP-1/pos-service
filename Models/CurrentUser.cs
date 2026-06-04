using pos_service.Models.Enums;
using pos_service.Models.DTO.Users;
using System.Security.Claims;

namespace pos_service.Models
{
    public class CurrentUser : UserResDto
    {
            public string Uuid                         { get; set; } = string.Empty;

            public bool IsAuthenticated                { get; set; }

            // populated at runtime
            public ICollection<Permission> Permissions { get; set; } = new List<Permission>();

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
                            //Id              = GetClaimValue<int>(principal, ClaimTypes.NameIdentifier, "user_id"),
                            Uuid            = GetClaimValue<string>(principal, "uuid") ?? string.Empty,
                            //Name            = GetClaimValue<string>(principal, ClaimTypes.Name) ?? string.Empty,
                            //UserName        = GetClaimValue<string>(principal, "username") ?? string.Empty,
                            Role = new Role
                            {
                                Id = GetClaimValue<int>(principal, "role_id"),
                                //Name = GetClaimValue<string>(principal, ClaimTypes.Role, "role") ?? string.Empty
                            }
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

            public bool IsInRole(params int[] roleIds)
                => IsAuthenticated && Role != null && roleIds.Contains(Role.Id);

            public bool HasPermission(PermissionType permission)
                => IsAuthenticated && Permissions.Any(p => p.PermissionType == permission);
        }
    }
