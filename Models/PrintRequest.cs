using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public record PrintRequest
    {
        [Required]
        public string OrderNumber { get; init; }
        // Whether to use network printing (TCP 9100). Defaults to false when not provided.
        public bool UseNetwork { get; init; } = false;
        public string? PrinterName { get; init; }
        public string? PrinterIp { get; init; }
        public int? Port { get; init; }
    }
}
