using pos_service.Models.Enums;

namespace pos_service.Models.DTO.Order
{
    public class OrderQueryDto
    {
        // Date range filters
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Status and type filters
        public OrderStatus? Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public SaleType? SaleType { get; set; }

        // Entity filters
        public int? CustomerId { get; set; }
        public int? CashierId { get; set; }

        // Search term
        public string? SearchTerm { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        // Sorting
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }
}
