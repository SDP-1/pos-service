using pos_service.Models.DTO.Audits;
using pos_service.Models.DTO.Suppliers;
using pos_service.Models.DTO.Inventory;

namespace pos_service.Models.DTO.Items
{
    public class ItemResDto : IFullResAuditDto
    {
        public int Id                            { get; set; }
        public int SubId                         { get; set; }
        public string Name                       { get; set; }
        public string PrintName                  { get; set; }
        public string? BarCode                   { get; set; }
        public InventoryResDto? Inventory        { get; set; }
        public ItemPriceResDto Price             { get; set; }
        public List<ItemExpiryResDto> ExpDates   { get; set; } = new();
        public List<SupplierResDto> Suppliers    { get; set; }


        public string Uuid                       { get; set; }
        public DateTime CreatedAt                { get; set; }
        public DateTime? UpdatedAt               { get; set; }
        public string CreatedBy                  { get; set; }
        public string? UpdatedBy                 { get; set; }
        public bool IsActive                     { get; set; }
    }
}
