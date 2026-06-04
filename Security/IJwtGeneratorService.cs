using pos_service.Models;

namespace pos_service.Security
{
    public interface IJwtGeneratorService
    {
        /// <summary>
        /// Generates a signed JSON Web Token (JWT) for the given user.
        /// The token contains application-specific claims required for authentication/authorization.
        /// </summary>
        /// <param name="user">The user for whom the token will be generated.</param>
        /// <returns>A signed JWT as a string.</returns>
        string GenerateToken(User user);
    }
}
