using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Items
{
    public class ItemExpiryReqDto
    {
        [Required]
        public DateTime ExpDate     { get; set; }

        [Range(0, int.MaxValue)]
        public int NotifyBeforeDays { get; set; } = 0;
    }
}
