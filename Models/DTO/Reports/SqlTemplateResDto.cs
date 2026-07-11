namespace pos_service.Models.DTO.Reports
{
    public class SqlTemplateResDto
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public string TemplateName { get; set; }
        public string? Description { get; set; }
        public string SqlQuery { get; set; }
        public List<SqlPlaceholderDto> Placeholders { get; set; } = new List<SqlPlaceholderDto>();
        public List<SqlSelectValueDto> SelectValues { get; set; } = new List<SqlSelectValueDto>();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
