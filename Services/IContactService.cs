using pos_service.Models.DTO.Contact;

namespace pos_service.Services
{
    public interface IContactService
    {
        Task<ContactResDto?> GetContactByIdAsync(int id);
        Task<IEnumerable<ContactResDto>> GetAllContactsAsync();
        Task<ContactResDto> CreateContactAsync(ContactReqDto dto);
        Task<bool> UpdateContactAsync(int id, ContactReqDto dto);
        Task<bool> DeleteContactAsync(int id);
    }
}
