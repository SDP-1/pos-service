using pos_service.Models;

namespace pos_service.Security
{
    public interface IJwtGenerator
    {
        string GenerateToken(User user);
    }
}
