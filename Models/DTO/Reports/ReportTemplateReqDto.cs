using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Reports
{
    public class ReportTemplateReqDto
    {
        [Required]
        [MaxLength(255)]
        public string ReportName                                     { get; set; }

        [MaxLength(1000)]
        public string? Description                                   { get; set; }

        [Required]
        public string HtmlContent                                    { get; set; }

        [Required]
        public bool IsActive                                         { get; set; } = true;

        public ReportParametersDto Parameters                        { get; set; } = new ReportParametersDto();

        public List<SqlPlaceholderMappingDto> SqlPlaceholderMappings { get; set; } = new List<SqlPlaceholderMappingDto>();

        public List<string> SqlTemplateUuids                         { get; set; } = new List<string>();


    }

    public class ReportParametersDto
    {
        public bool StartDate                     { get; set; }
        public bool EndDate                       { get; set; }
        public bool Item                          { get; set; }
        public bool Supplier                      { get; set; }
        public bool User                          { get; set; }
        public bool PaymentMethod                 { get; set; }
        public List<string> TableSqlTemplateUuids { get; set; } = new List<string>();
    }

    public class SqlPlaceholderMappingDto
    {
        public string SqlPlaceholder { get; set; }
        public string ReportValue    { get; set; }
    }
}
