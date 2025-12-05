using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Services.Permissions
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId)
        {
            var perms = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .ToListAsync();

            return perms;
        }

        public async Task<bool> AddPermissionToRoleAsync(int roleId, string permissionName)
        {
            var perm = await _context.Permissions.SingleOrDefaultAsync(p => p.Name == permissionName);
            if (perm == null)
            {
                perm = new Permission { Name = permissionName, Uuid = Guid.NewGuid().ToString() };
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
            var perm = await _context.Permissions.SingleOrDefaultAsync(p => p.Name == permissionName);
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
