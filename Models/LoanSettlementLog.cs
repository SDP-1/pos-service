using pos_service.Models.Audit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using pos_service.Models.Enums;

namespace pos_service.Models
{
    public class LoanSettlementLog : IAuditable
    {
        public int Id                      { get; set; }

        [Required]
        public int OrderId                 { get; set; }
        public virtual Order Order         { get; set; }

        public DateTime PaymentDate        { get; set; }

        [MaxLength(500)]
        public string? Description         { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid          { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingBalance    { get; set; }

        public LoanSettlementStatus Status { get; set; } = LoanSettlementStatus.Created;

        // IAuditable
        public string Uuid                 { get; set; }
        public DateTime CreatedAt          { get; set; }
        public DateTime? UpdatedAt         { get; set; }
        public string CreatedBy            { get; set; }
        public string? UpdatedBy           { get; set; }
        public bool IsActive               { get; set; } = true;
    }
}
