namespace pos_service.Models.DTO.Bills
{
    public class PrintResponseDto
    {
        public bool Printed { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Printer { get; set; } = string.Empty;
        public string? Error { get; set; }
    }

    public class PrintersResponseDto
    {
        public List<string> Printers { get; set; } = new();
        public string? DefaultPrinter { get; set; }
    }
}
