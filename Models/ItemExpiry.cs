using pos_service.Models.Audit;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    public class ItemExpiry : IAuditable
    {
        public int Id               { get; set; }
        public int ItemsId          { get; set; }
        public int ItemsSubId       { get; set; }
        public string ItemUuid      { get; set; }

        [Column(TypeName = "date")]
        public DateTime ExpDate     { get; set; }

        public int NotifyBeforeDays { get; set; } = 0;

        public virtual Item Item    { get; set; }

        // --- Implementation of IAuditable ---
        public string Uuid          { get; set; }
        public DateTime CreatedAt   { get; set; }
        public DateTime? UpdatedAt  { get; set; }
        public string CreatedBy     { get; set; }
        public string? UpdatedBy    { get; set; }
        public bool IsActive        { get; set; } = true;
    }
}
