namespace pos_service.Models.DTO.Order
{
    public class OrderListResDto
    {
        public List<OrderSummaryResDto> Orders { get; set; } = new List<OrderSummaryResDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
