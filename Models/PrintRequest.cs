using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public record PrintRequest
    {
        [Required]
        public string OrderNumber { get; init; } = string.Empty;

        public string? PrinterName { get; init; }
    }
}
