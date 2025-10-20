namespace pos_service.Models.DTO
{
    public class SupplierResDto
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public List<Contact> contacts { get; set; }

    }
}
