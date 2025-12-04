using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public class Permission
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public string Uuid { get; set; } = string.Empty;
    }
}