using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Repositories.Permissions
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all permissions configured in the system.
        /// </summary>
        /// <returns>A collection of Permission entities.</returns>
        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _context.Permissions.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Retrieves permissions assigned to a specific role, automatically inheriting permissions from descendant child roles via BFS traversal.
        /// </summary>
        /// <param name="roleId">Database id of the role.</param>
        /// <returns>A collection of Permission entities assigned to or inherited by the role.</returns>
        public async Task<IEnumerable<Permission>> GetForRoleAsync(int roleId)
        {
            var allRoles = await _context.Roles
                .Where(r => r.IsActive)
                .ToListAsync();

            var descendantIds = new HashSet<int> { roleId };
            var queue = new Queue<int>();
            queue.Enqueue(roleId);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var children = allRoles.Where(r => r.ParentRoleId == currentId).Select(r => r.Id);
                foreach (var childId in children)
                {
                    if (descendantIds.Add(childId))
                    {
                        queue.Enqueue(childId);
                    }
                }
            }

            return await _context.RolePermissions
                .Where(rp => descendantIds.Contains(rp.RoleId))
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// Adds the named permission to the specified role. If the permission does not exist in the Permissions table it will be created.
        /// </summary>
        /// <param name="roleId">Database id of the role.</param>
        /// <param name="permissionName">The name of the permission (must match a PermissionType enum value).</param>
        /// <returns>True if the permission was added; false if the mapping already existed.</returns>
        /// <exception cref="ArgumentException">Thrown when permissionName is not a valid PermissionType.</exception>
        public async Task<bool> AddPermissionToRoleAsync(int roleId, string permissionName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!Enum.TryParse<PermissionType>(permissionName, true, out var permType))
                    throw new ArgumentException("Invalid permission type", nameof(permissionName));

                var perm = await _context.Permissions.SingleOrDefaultAsync(p => p.PermissionType == permType);
                if (perm == null)
                {
                    var val = (int)permType;
                    var cat = val switch
                    {
                        >= 100 and < 200 => PermissionCatagory.ORDER,
                        >= 200 and < 300 => PermissionCatagory.ITEM,
                        >= 300 and < 400 => PermissionCatagory.USER,
                        >= 400 and < 500 => PermissionCatagory.SUPPLIER,
                        >= 500 and < 600 => PermissionCatagory.CONTACT,
                        >= 600 and < 650 => PermissionCatagory.PERMISSION,
                        >= 650 and < 700 => PermissionCatagory.ROLE,
                        >= 800 and < 850 => PermissionCatagory.REPORT,
                        _ => PermissionCatagory.DEFAULT
                    };

                    perm = new Permission { Id = val, PermissionType = permType, PermissionCatagory = cat };
                    _context.Permissions.Add(perm);
                    await _context.SaveChangesAsync();
                }

                if (await _context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == perm.Id))
                {
                    await transaction.CommitAsync();
                    return false;
                }

                _context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = perm.Id, Uuid = Guid.NewGuid().ToString() });
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Removes the named permission mapping from the specified role.
        /// </summary>
        /// <param name="roleId">Database id of the role.</param>
        /// <param name="permissionName">The name of the permission (must match a PermissionType enum value).</param>
        /// <returns>True when the mapping was removed; false if the permission or mapping was not found or the permission name is invalid.</returns>
        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, string permissionName)
        {
            if (!Enum.TryParse<PermissionType>(permissionName, true, out var permType))
                return false;

            var perm = await _context.Permissions.SingleOrDefaultAsync(p => p.PermissionType == permType);
            if (perm == null)
                return false;

            var mapping = await _context.RolePermissions.SingleOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == perm.Id);
            if (mapping == null)
                return false;

            _context.RolePermissions.Remove(mapping);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
