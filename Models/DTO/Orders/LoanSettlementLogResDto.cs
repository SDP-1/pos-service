using pos_service.Models.Enums;

namespace pos_service.Models.DTO.Orders
{
    public class LoanSettlementLogResDto
    {
        public int Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Description { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public LoanSettlementStatus Status { get; set; }

        // Audit fields
        public string Uuid { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
