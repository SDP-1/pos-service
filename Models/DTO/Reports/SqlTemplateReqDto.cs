using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Reports
{
    public class SqlTemplateReqDto
    {
        [Required]
        [MaxLength(255)]
        public string TemplateName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string SqlQuery { get; set; }

        public List<SqlPlaceholderDto> Placeholders { get; set; } = new List<SqlPlaceholderDto>();

        public List<SqlSelectValueDto> SelectValues { get; set; } = new List<SqlSelectValueDto>();

        public bool IsActive { get; set; } = true;
    }

    public class SqlPlaceholderDto
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public string DataType { get; set; } // String, Date, Number
        public bool Required { get; set; }
    }

    public class SqlSelectValueDto
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
