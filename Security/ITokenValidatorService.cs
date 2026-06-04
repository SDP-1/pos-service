using System.Security.Claims;

namespace pos_service.Security
{
    public interface ITokenValidatorService
    {
        /// <summary>
        /// Validates the authenticated token principal by ensuring the user exists and is active.
        /// Throws UnauthorizedAccessException when validation fails.
        /// </summary>
        /// <param name="principal">ClaimsPrincipal extracted from the validated token.</param>
        Task ValidateTokenPrincipalAsync(ClaimsPrincipal principal);
    }
}
