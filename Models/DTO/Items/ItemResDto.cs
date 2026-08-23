using pos_service.Models.DTO.Audits;
using pos_service.Models.DTO.Suppliers;
using pos_service.Models.DTO.Inventory;

namespace pos_service.Models.DTO.Items
{
    /// <summary>
    /// Response DTO representing full product item details including inventory, pricing, expiries, suppliers, and batch counts.
    /// </summary>
    public class ItemResDto : IFullResAuditDto
    {
        /// <summary>
        /// Main item group identifier.
        /// </summary>
        public int Id                            { get; set; }

        /// <summary>
        /// Variant sub-identifier (0 for base item).
        /// </summary>
        public int SubId                         { get; set; }

        /// <summary>
        /// Full internal product name.
        /// </summary>
        public string Name                       { get; set; }

        /// <summary>
        /// Short product name formatted for receipt printing.
        /// </summary>
        public string PrintName                  { get; set; }

        /// <summary>
        /// EAN/UPC barcode string.
        /// </summary>
        public string? BarCode                   { get; set; }

        /// <summary>
        /// Product description or notes.
        /// </summary>
        public string? Description               { get; set; }

        /// <summary>
        /// Inventory stock and packaging hierarchy details.
        /// </summary>
        public InventoryResDto? Inventory        { get; set; }

        /// <summary>
        /// Active price configuration.
        /// </summary>
        public ItemPriceResDto Price             { get; set; }

        /// <summary>
        /// Tracked expiration dates and notification thresholds.
        /// </summary>
        public List<ItemExpiryResDto> ExpDates   { get; set; } = new();

        /// <summary>
        /// Linked supplier contacts.
        /// </summary>
        public List<SupplierResDto> Suppliers    { get; set; }

        /// <summary>
        /// Total count of active batch lots existing for this item.
        /// </summary>
        public int BatchCount                    { get; set; }

        /// <summary>
        /// Unique UUID identifier of the item.
        /// </summary>
        public string Uuid                       { get; set; }

        /// <summary>
        /// Creation timestamp in UTC.
        /// </summary>
        public DateTime CreatedAt                { get; set; }

        /// <summary>
        /// Last modification timestamp in UTC.
        /// </summary>
        public DateTime? UpdatedAt               { get; set; }

        /// <summary>
        /// Creator user UUID or full name.
        /// </summary>
        public string CreatedBy                  { get; set; }

        /// <summary>
        /// Last modifier user UUID or full name.
        /// </summary>
        public string? UpdatedBy                 { get; set; }

        /// <summary>
        /// Soft deletion / active status flag.
        /// </summary>
        public bool IsActive                     { get; set; }
    }
}
