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

            /// <summary>
            /// Create a CurrentUser instance from a ClaimsPrincipal.
            /// Inspects claims to populate user identifying information (Uuid and Role.Id) and sets IsAuthenticated.
            /// </summary>
            /// <param name="principal">ClaimsPrincipal to read claims from. If null or not authenticated an unauthenticated CurrentUser is returned.</param>
            /// <returns>A <see cref="CurrentUser"/> populated from claims, or an unauthenticated CurrentUser on failure.</returns>
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

            /// <summary>
            /// Attempts to read a claim value from the provided ClaimsPrincipal and convert it to the requested type.
            /// </summary>
            /// <typeparam name="T">The target type to convert the claim value to (string, int, etc.).</typeparam>
            /// <param name="principal">The ClaimsPrincipal that contains the claims.</param>
            /// <param name="claimTypes">One or more claim type names to try in order. The first non-empty claim value will be returned if convertible.</param>
            /// <returns>Converted claim value of type <typeparamref name="T"/> if found and convertible; otherwise default(<typeparamref name="T"/>).</returns>
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

            /// <summary>
            /// Determines whether the current user is in any of the specified role ids.
            /// </summary>
            /// <param name="roleIds">Role id values to check against the current user's role.</param>
            /// <returns>True when the user is authenticated and their role id matches one of the provided roleIds; otherwise false.</returns>
            public bool IsInRole(params int[] roleIds)
                => IsAuthenticated && Role != null && roleIds.Contains(Role.Id);

            /// <summary>
            /// Checks whether the current user has a specific permission.
            /// </summary>
            /// <param name="permission">The permission to check for.</param>
            /// <returns>True when the user is authenticated and their Permissions collection contains the requested permission; otherwise false.</returns>
            public bool HasPermission(PermissionType permission)
                => IsAuthenticated && Permissions.Any(p => p.PermissionType == permission);
        }
    }
