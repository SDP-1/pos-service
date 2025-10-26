using Microsoft.AspNetCore.Identity;

namespace pos_service.Security
{
    // Implementation uses the built-in ASP.NET Core Identity PasswordHasher
    public class PasswordHasherService : IPasswordHasher
    {
        private readonly PasswordHasher<object> _passwordHasher = new PasswordHasher<object>();

        /// <summary>
        /// Hashes the plain text password for secure storage.
        /// </summary>
        public string HashPassword(string password)
        {
            // The first argument is 'null' because the generic PasswordHasher requires a TUser, 
            // but we are using it simply for hashing logic.
            return _passwordHasher.HashPassword(null, password);
        }

        /// <summary>
        /// Verifies a plain text password against the stored hash.
        /// </summary>
        public bool VerifyPassword(string password, string passwordHash)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, passwordHash, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
