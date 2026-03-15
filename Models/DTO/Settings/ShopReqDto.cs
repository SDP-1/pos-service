using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace pos_service.Models.DTO.Settings
{
    // Use this DTO for multipart/form-data requests where a logo file may be included.
    public class ShopReqDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        // Optional logo file uploaded as multipart/form-data
        public IFormFile? Logo { get; set; }
    }
}
