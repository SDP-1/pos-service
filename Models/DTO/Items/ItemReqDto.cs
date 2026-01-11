using pos_service.Models.DTO.Audits;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Items
{
    public class ItemReqDto : IReqAuditDto
    {
        // Id is optional for create requests. If not supplied (null or 0), the service will
        // assign an Id automatically (next available main Id).
        public int? Id                        { get; set; }

        // SubId is optional. If not supplied, the service will assign the next available
        // SubId for the given main Id (or 0 for a newly created main Id).
        public int? SubId                     { get; set; }

        [Required]
        [StringLength(200)]
        public string Name                    { get; set; }

        [Required]
        [StringLength(40)]
        public string PrintName               { get; set; }

        [StringLength(100)]
        public string? BarCode                { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StockQuantity          { get; set; } = 0;

        public bool AllowsDecimalQuantities   { get; set; } = false;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal BuyingPrice            { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal MarkedPrice            { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal RetailPrice            { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal WholesalePrice         { get; set; }

        [Range(0, 100)]
        public decimal RetailDiscountRatio    { get; set; } = 0.0m;

        [Range(0, 100)]
        public decimal WholesaleDiscountRatio { get; set; } = 0.0m;

        /// <summary>
        /// A list of supplier IDs to associate with this item.
        /// </summary>
        public ICollection<int> SupplierIds   { get; set; } = new List<int>();

        public bool IsActive                  { get; set; } = true;
    }
}
