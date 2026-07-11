namespace pos_service.Models.DTO.Reports
{
    public class ReportTemplateResDto
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public string ReportName { get; set; }
        public string? Description { get; set; }
        public string HtmlContent { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public ReportParametersDto Parameters { get; set; } = new ReportParametersDto();

        public List<SqlPlaceholderMappingDto> SqlPlaceholderMappings { get; set; } = new List<SqlPlaceholderMappingDto>();

        public List<SqlTemplateResDto> SqlTemplates { get; set; } = new List<SqlTemplateResDto>();


    }
}
