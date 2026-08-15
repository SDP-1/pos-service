using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories.Roles
{
    public class RoleRepository : BaseRepository, IRoleRepository
    {
        public RoleRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves all roles in the system.
        /// </summary>
        /// <returns>A collection of Role entities.</returns>
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Retrieves roles that are currently active (IsActive == true).
        /// </summary>
        /// <returns>A collection of active Role entities.</returns>
        public async Task<IEnumerable<Role>> GetActiveAsync()
        {
            return await _context.Roles.AsNoTracking().Where(r => r.IsActive).ToListAsync();
        }

        /// <summary>
        /// Finds a role by its database identifier.
        /// </summary>
        /// <param name="id">Database id of the role.</param>
        /// <returns>The Role when found; otherwise null.</returns>
        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        /// <summary>
        /// Finds a role by its unique name.
        /// </summary>
        /// <param name="name">Name of the role to find.</param>
        /// <returns>The Role when found; otherwise null.</returns>
        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == name);
        }

        /// <summary>
        /// Adds a new role to the database and assigns a new UUID.
        /// </summary>
        /// <param name="role">Role entity to add.</param>
        /// <returns>The added Role with updated identity fields.</returns>
        public async Task<Role> AddAsync(Role role)
        {
            if (string.IsNullOrEmpty(role.Uuid))
            {
                role.Uuid = Guid.NewGuid().ToString();
            }
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        /// <summary>
        /// Updates an existing role.
        /// </summary>
        /// <param name="role">Role entity with updated values.</param>
        /// <returns>The updated Role.</returns>
        public async Task<Role> UpdateAsync(Role role)
        {
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
            return role;
        }

        /// <summary>
        /// Deletes the given role from the database.
        /// </summary>
        /// <param name="role">Role entity to remove.</param>
        public async Task DeleteAsync(Role role)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Breadth-First Search (BFS) Permission Resolution.
        /// Parent/Senior roles automatically inherit all permissions of their Child/Subordinate roles recursively down the tree.
        /// </summary>
        public List<Permission> GetPermissionsByRoleId(int roleId)
        {
            // Load all active roles and their direct permissions into memory
            var allRoles = _context.Roles
                .Include(r => r.Permissions)
                .Where(r => r.IsActive)
                .ToList();

            var rolesDict = allRoles.ToDictionary(r => r.Id, r => r);

            // BFS queue to discover all descendant role IDs
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

            // Accumulate unique permissions from target role and all descendant roles
            var permissions = new List<Permission>();
            var addedPermissionIds = new HashSet<int>();

            foreach (var rId in descendantIds)
            {
                if (rolesDict.TryGetValue(rId, out var role))
                {
                    foreach (var perm in role.Permissions)
                    {
                        if (addedPermissionIds.Add(perm.Id))
                        {
                            permissions.Add(perm);
                        }
                    }
                }
            }

            return permissions;
        }

        public async Task<List<Permission>> GetPermissionsByRoleIdAsync(int roleId)
        {
            var allRoles = await _context.Roles
                .Include(r => r.Permissions)
                .Where(r => r.IsActive)
                .ToListAsync();

            var rolesDict = allRoles.ToDictionary(r => r.Id, r => r);

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

            var permissions = new List<Permission>();
            var addedPermissionIds = new HashSet<int>();

            foreach (var rId in descendantIds)
            {
                if (rolesDict.TryGetValue(rId, out var role))
                {
                    foreach (var perm in role.Permissions)
                    {
                        if (addedPermissionIds.Add(perm.Id))
                        {
                            permissions.Add(perm);
                        }
                    }
                }
            }

            return permissions;
        }
    }
}