using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User> SaveNewUserWithContactsAsync(User user, IEnumerable<Contact>? contacts)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (contacts != null && contacts.Any())
                {
                    foreach (var contact in contacts)
                    {
                        contact.UserId = user.Id;
                    }
                    _context.Contacts.AddRange(contacts);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return user;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Retrieves all users including their contacts and role.
        /// </summary>
        /// <returns>Collection of User entities.</returns>
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Contacts)
                .Include(u => u.Role)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a user by database id.
        /// </summary>
        /// <param name="id">Database id of the user.</param>
        /// <returns>User when found; otherwise null.</returns>
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <summary>
        /// Retrieves a user by their username (email), including their related contacts.
        /// </summary>
        /// <param name="userName">Username to search for.</param>
        /// <returns>User when found; otherwise null.</returns>
        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _context.Users
                .Include(u => u.Contacts)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }

        /// <summary>
        /// Retrieves a user including related contact information.
        /// </summary>
        /// <param name="id">Database id of the user.</param>
        /// <returns>User when found; otherwise null.</returns>
        public async Task<User?> GetByIdWithContactsAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Contacts)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Retrieves a user by UUID including related contacts and role.
        /// </summary>
        /// <param name="uuid">UUID of the user.</param>
        /// <returns>User when found; otherwise null.</returns>
        public async Task<User?> GetByUuidAsync(string uuid)
        {
            return await _context.Users
                .Include(u => u.Contacts)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Uuid == uuid);
        }

        /// <summary>
        /// Adds a new user to the database and assigns a UUID.
        /// </summary>
        /// <param name="user">User entity to add.</param>
        /// <returns>The added User entity, or null on error.</returns>
        public async Task<User> AddAsync(User user)
        {
            try { 
                user.Uuid = Guid.NewGuid().ToString();
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Explicitly load the Role reference
                await _context.Entry(user).Reference(u => u.Role).LoadAsync();

                return user;
            }
            catch (Exception e) 
            { 

            }

            return null;
        }

        /// <summary>
        /// Updates an existing user in the database.
        /// </summary>
        /// <param name="user">User entity with updated values.</param>
        /// <returns>The updated User entity, or null on error.</returns>
        public async Task<User> UpdateAsync(User user)
        {
            try
            {
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Force reload the Role reference in case RoleId has changed
                _context.Entry(user).Reference(u => u.Role).IsLoaded = false;
                await _context.Entry(user).Reference(u => u.Role).LoadAsync();

                return user;
            }
            catch (Exception e) 
            { 

            }
            return null;
        }

        /// <summary>
        /// Deletes the specified user from the data store.
        /// </summary>
        /// <param name="user">User entity to delete.</param>
        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if a user with the specified identifier exists.
        /// </summary>
        /// <param name="id">Database id to check.</param>
        /// <returns>True if a user with the id exists; otherwise false.</returns>
        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(e => e.Id == id);
        }
    }
}
