using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Order
{
    public class OrderStatusUpdateReqDto
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}
