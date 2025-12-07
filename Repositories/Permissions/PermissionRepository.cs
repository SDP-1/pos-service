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

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _context.Permissions.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetForRoleAsync(int roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<bool> AddPermissionToRoleAsync(int roleId, string permissionName)
        {
            if (!Enum.TryParse<PermissionType>(permissionName, true, out var permType))
                throw new ArgumentException("Invalid permission type", nameof(permissionName));

            var perm = await _context.Permissions.SingleOrDefaultAsync(p => p.PermissionType == permType);
            if (perm == null)
            {
                var val = (int)permType;
                var cat = val switch
                {
                    >= 100 and < 110 => PermissionCatagory.ORDER,
                    >= 110 and < 120 => PermissionCatagory.ITEM,
                    >= 120 and < 130 => PermissionCatagory.USER,
                    >= 130 and < 140 => PermissionCatagory.SUPPLIER,
                    >= 140 and < 150 => PermissionCatagory.CONTACT,
                    >= 150 and < 160 => PermissionCatagory.PERMISSION,
                    >= 160 and < 170 => PermissionCatagory.ROLE,
                    _ => PermissionCatagory.PERMISSION
                };

                perm = new Permission { Id = val, PermissionType = permType, PermissionCatagory = cat, Uuid = Guid.NewGuid().ToString() };
                _context.Permissions.Add(perm);
                await _context.SaveChangesAsync();
            }

            if (await _context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == perm.Id))
                return false;

            _context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = perm.Id, Uuid = Guid.NewGuid().ToString() });
            await _context.SaveChangesAsync();

            return true;
        }

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
