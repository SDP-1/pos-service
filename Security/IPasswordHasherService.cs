namespace pos_service.Security
{
    public interface IPasswordHasherService
    {
        /// <summary>
        /// Hashes the provided plain-text password.
        /// </summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>The hashed password string suitable for storage.</returns>
        string HashPassword(string password);
        /// <summary>
        /// Verifies a plain-text password against a stored password hash.
        /// </summary>
        /// <param name="password">The plain-text password to verify.</param>
        /// <param name="passwordHash">The hashed password to verify against.</param>
        /// <returns>True if the password matches the hash; otherwise false.</returns>
        bool VerifyPassword(string password, string passwordHash);
    }
}
