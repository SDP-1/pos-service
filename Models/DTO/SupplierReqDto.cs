using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO
{
    public class SupplierReqDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        public string? Address { get; set; }
    }
}
