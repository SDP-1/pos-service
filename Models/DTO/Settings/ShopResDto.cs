using System;

namespace pos_service.Models.DTO.Settings
{
    public class ShopResDto
    {
        public int Id              { get; set; }
        public string Uuid         { get; set; }
        public string Name         { get; set; }
        public string? Address     { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email       { get; set; }
        public byte[]? Logo        { get; set; }
    }
}
