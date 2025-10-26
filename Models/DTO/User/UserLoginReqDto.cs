using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.User
{
    public class UserLoginReqDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
