using pos_service.Models.DTO.Items;

namespace pos_service.Models.DTO.Inventory
{
    public class RequiredItemDto
    {
        public string ItemUuid          { get; set; }
        public string ItemName          { get; set; }
        public string SupplierName      { get; set; }
        public string CurrentStock      { get; set; }
        public string RequiredStock     { get; set; }
        public string? LastPurchaseInfo { get; set; }
    }
}
