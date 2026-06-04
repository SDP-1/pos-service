using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using pos_service.Models;

namespace pos_service.Security
{
    public class JwtGeneratorService : IJwtGeneratorService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtGeneratorService(IConfiguration configuration)
        {
            _secretKey = configuration["JwtSettings:SecretKey"];
        }

        /// <summary>
        /// Generates a signed JSON Web Token (JWT) for the given user.
        /// </summary>
        /// <param name="user">The user for whom the token will be generated. Must not be null.</param>
        /// <returns>A signed JWT as a string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when JWT secret key is not configured.</exception>
        public string GenerateToken(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(_secretKey)) throw new InvalidOperationException("JWT secret key is not configured.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            // safe role retrieval: prefer Role.Name if available, fallback to RoleId
            //var roleName = user.Role?.Name ?? ($"role:{user.RoleId}");

            // 1. Define the claims (user data stored in the token)
            var claims = new List<Claim>
            {
                //new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("role_id", user.RoleId.ToString()),
                //new Claim(ClaimTypes.Role, roleName),
                //new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                //new Claim("user_id", user.Id.ToString()),
                new Claim("uuid", user.Uuid ?? string.Empty),
                //new Claim("username", user.UserName ?? string.Empty)
            };

            // 2. Create the token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject            = new ClaimsIdentity(claims),
                Expires            = DateTime.Now.AddDays(1), // Token valid for 1 days
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            // 3. Create and write the token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
