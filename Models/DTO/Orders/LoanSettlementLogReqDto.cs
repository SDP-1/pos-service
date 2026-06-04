using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Orders
{
    public class LoanSettlementLogReqDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "AmountPaid must be greater than zero")]
        public decimal AmountPaid { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
