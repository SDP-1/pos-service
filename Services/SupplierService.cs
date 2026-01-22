using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Suppliers;
using pos_service.Repositories;

namespace pos_service.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepo;
        private readonly IMapper             _mapper;

        public SupplierService(ISupplierRepository supplierRepo, IMapper mapper)
        {
            _supplierRepo = supplierRepo;
            _mapper       = mapper;
        }

        public async Task<IEnumerable<SupplierResDto>> GetAllSuppliersAsync(CurrentUser currentUser)
        {
            var suppliers = await _supplierRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<SupplierResDto>>(suppliers);
        }

        public async Task<SupplierResDto?> GetSupplierByIdAsync(int id, CurrentUser currentUser)
        {
            var supplier = await _supplierRepo.GetByIdWithDetailsAsync(id);
            return _mapper.Map<SupplierResDto?>(supplier);
        }

        public async Task<SupplierResDto?> GetSupplierWithItemsAsync(int id, CurrentUser currentUser)
        {
            var supplier = await _supplierRepo.GetSupplierWithItemsAsync(id);
            return _mapper.Map<SupplierResDto?>(supplier);
        }

        public async Task<SupplierResDto> CreateSupplierAsync(SupplierReqDto dto, CurrentUser currentUser)
        {
            var supplier = _mapper.Map<Supplier>(dto);

            var newSupplier = await _supplierRepo.AddAsync(supplier);
            return _mapper.Map<SupplierResDto>(newSupplier);
        }

        public async Task<bool> UpdateSupplierAsync(int id, SupplierReqDto dto, CurrentUser currentUser)
        {
            var supplierToUpdate = await _supplierRepo.GetByIdWithDetailsAsync(id);
            if (supplierToUpdate == null) return false;

            _mapper.Map(dto, supplierToUpdate);

            await _supplierRepo.UpdateAsync(supplierToUpdate);
            return true;
        }

        public async Task<bool> DeleteSupplierAsync(int id, CurrentUser currentUser)
        {
            var supplier = await _supplierRepo.GetByIdAsync(id);
            if (supplier == null) return false;

            await _supplierRepo.DeleteAsync(supplier);
            return true;
        }
    }
}
