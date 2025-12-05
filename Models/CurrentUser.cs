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

            // role from DB
            public int RoleId           { get; set; }
            public string RoleName      { get; set; } = string.Empty;

            public bool IsAuthenticated { get; set; }

            // populated at runtime
            public ICollection<string> Permissions { get; set; } = new List<string>();

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
                        RoleId          = GetClaimValue<int>(principal, "role_id"),
                        RoleName        = GetClaimValue<string>(principal, ClaimTypes.Role, "role") ?? string.Empty
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
                => IsAuthenticated && roleIds.Contains(RoleId);

            public bool HasPermission(string permission)
                => IsAuthenticated && Permissions.Contains(permission);

            public bool CanManageUsers()
                => IsInRole(1, 3); // default SystemAdmin role id 1 and Manager id 3 - keep legacy semantics if seeded that way

            public bool CanViewSensitiveData()
                => IsInRole(1, 3);
        }
    }
