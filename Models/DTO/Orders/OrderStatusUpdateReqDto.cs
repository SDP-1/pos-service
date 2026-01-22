using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Orders
{
    public class OrderStatusUpdateReqDto
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}
