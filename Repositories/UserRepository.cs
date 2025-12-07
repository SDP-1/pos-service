using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Contacts)
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <summary>
        /// Retrieves a user by their username (email), including their related contacts.
        /// </summary>
        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _context.Users
                .Include(u => u.Contacts)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<User?> GetByIdWithContactsAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Contacts)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUuidAsync(string uuid)
        {
            return await _context.Users
                .Include(u => u.Contacts)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Uuid == uuid);
        }

        public async Task<User> AddAsync(User user)
        {
            try { 
                user.Uuid = Guid.NewGuid().ToString();
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception e) 
            { 
            
            }

            return null;
        }

        public async Task<User> UpdateAsync(User user)
        {
            try
            {
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception e) 
            { 
            
            }
            return null;
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(e => e.Id == id);
        }
    }
}
