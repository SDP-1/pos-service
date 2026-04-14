using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Customers;
using pos_service.Repositories;
using pos_service.Services.Common.Cache;

namespace pos_service.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        public CustomerService(ICustomerRepository repository, IMapper mapper, ICacheService cache)
        {
            _repository = repository;
            _mapper     = mapper;
            _cache      = cache;
        }

        public async Task<IEnumerable<CustomerResDto>> GetAllCustomersAsync(CurrentUser currentUser)
        {
            return await _cache.GetOrCreateAsync<IEnumerable<CustomerResDto>>(ServiceCacheKey.Customers, null,
                () => _repository.GetAllAsync());
        }

        public async Task<CustomerResDto?> GetCustomerByIdAsync(int id, CurrentUser currentUser)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<CustomerResDto>> GetCustomersBySearchAsync(string searchTerm, CurrentUser currentUser)
        {
            return await _repository.GetBySearchAsync(searchTerm);
        }

        public async Task<CustomerResDto> CreateCustomerAsync(CustomerReqDto dto, CurrentUser currentUser)
        {
            // Ensure unique phone number
            var existingByPhone = await _repository.GetByPhoneNumberAsync(dto.PhoneNumber);
            if (existingByPhone != null)
                throw new ArgumentException("Customer with this phone number already exists.");

            // Ensure unique email when provided
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existingByEmail = await _repository.GetByEmailAsync(dto.Email!);
                if (existingByEmail != null)
                    throw new ArgumentException("Customer with this email already exists.");
            }

            var customer = _mapper.Map<Customer>(dto);
            var newCust = await _repository.AddAsync(customer);

            RemoveCustomerCache();

            return _mapper.Map<CustomerResDto>(newCust);
        }

        public async Task<bool> UpdateCustomerAsync(int id, CustomerReqDto dto, CurrentUser currentUser)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            // If phone changed, ensure uniqueness
            if (!string.Equals(existing.PhoneNumber, dto.PhoneNumber, StringComparison.Ordinal))
            {
                var other = await _repository.GetByPhoneNumberAsync(dto.PhoneNumber);
                if (other != null && other.Id != id)
                    throw new ArgumentException("Another customer with this phone number already exists.");
            }

            // If email provided/changed, ensure uniqueness
            if (!string.Equals(existing.Email ?? string.Empty, dto.Email ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var otherByEmail = await _repository.GetByEmailAsync(dto.Email!);
                    if (otherByEmail != null && otherByEmail.Id != id)
                        throw new ArgumentException("Another customer with this email already exists.");
                }
            }

            var updated = await _repository.UpdateAsync(id, dto);
            if (updated != null)
                RemoveCustomerCache();

            return updated != null;
        }

        public async Task<bool> DeleteCustomerAsync(int id, CurrentUser currentUser)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (deleted)
                RemoveCustomerCache();

            return deleted;
        }

        private void RemoveCustomerCache()
        {
            _cache.RemovePrimary(ServiceCacheKey.Customers);
        }
    }
}
