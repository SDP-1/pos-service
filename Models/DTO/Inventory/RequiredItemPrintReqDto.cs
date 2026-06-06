using System.Collections.Generic;

namespace pos_service.Models.DTO.Inventory
{
    public class RequiredItemPrintReqDto
    {
        public int SupplierId                      { get; set; }
        public List<RequiredItemPrintItem> Items   { get; set; } = new();
    }

    public class RequiredItemPrintItem
    {
        public string ItemUuid                     { get; set; }
        public decimal? RequiredStock              { get; set; }
    }
}
