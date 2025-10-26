using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Contact;
using pos_service.Repositories;

namespace pos_service.Services
{
    public class ContactService : IContactService
    {
        private readonly IContactRepository _repository;
        private readonly IMapper _mapper;
        public ContactService(IContactRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ContactResDto>> GetAllContactsAsync()
        {
            var contacts = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ContactResDto>>(contacts);
        }

        public async Task<ContactResDto?> GetContactByIdAsync(int id)
        {
            var contact = await _repository.GetByIdAsync(id);
            return _mapper.Map<ContactResDto?>(contact);
        }

        public async Task<ContactResDto> CreateContactAsync(ContactReqDto dto)
        {
            var contact = _mapper.Map<Contact>(dto);
            var newContact = await _repository.AddAsync(contact);
            return _mapper.Map<ContactResDto>(newContact);
        }

        public async Task<bool> UpdateContactAsync(int id, ContactReqDto dto)
        {
            var contactToUpdate = await _repository.GetByIdAsync(id);
            if (contactToUpdate == null) return false;

            _mapper.Map(dto, contactToUpdate);
            await _repository.UpdateAsync(contactToUpdate);
            return true;
        }

        public async Task<bool> DeleteContactAsync(int id)
        {
            var contact = await _repository.GetByIdAsync(id);
            if (contact == null) return false;

            await _repository.DeleteAsync(contact);
            return true;
        }
    }
}
