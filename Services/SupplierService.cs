using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Suppliers;
using pos_service.Repositories;

namespace pos_service.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepo;
        private readonly IItemRepository     _itemRepo;
        private readonly IContactRepository  _contactRepo;
        private readonly IMapper             _mapper;

        public SupplierService(
            ISupplierRepository supplierRepo,
            IItemRepository itemRepo,
            IContactRepository contactRepo,
            IMapper mapper)
        {
            _supplierRepo = supplierRepo;
            _itemRepo     = itemRepo;
            _contactRepo  = contactRepo;
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
            // Enforce unique supplier name
            var existing = await _supplierRepo.GetByNameAsync(dto.Name);
            if (existing != null)
            {
                throw new ArgumentException("This supplier name already exists.");
            }

            var supplier = _mapper.Map<Supplier>(dto);

            // Handle Contacts if provided
            if (dto.Contacts != null && dto.Contacts.Any())
            {
                foreach (var c in dto.Contacts)
                {
                    var contact = _mapper.Map<Contact>(c);
                    contact.Uuid = Guid.NewGuid().ToString();
                    supplier.Contacts.Add(contact);
                }
            }

            // Add supplier first to get Id
            var newSupplier = await _supplierRepo.AddAsync(supplier);

            // Handle Item associations by UUID if provided
            if (dto.ItemUuids != null && dto.ItemUuids.Any())
            {
                foreach (var itemUuid in dto.ItemUuids)
                {
                    var item = await _itemRepo.GetByUuidAsync(itemUuid);
                    if (item != null)
                    {
                        var isu = new ItemSupplier
                        {
                            Uuid        = Guid.NewGuid().ToString(),
                            SuppliersId = newSupplier.Id,
                            ItemsId     = item.Id,
                            ItemsSubId  = item.SubId,
                            Supplier    = newSupplier,
                            Item        = item
                        };
                        newSupplier.ItemSuppliers.Add(isu);
                    }
                }

                // persist changes
                await _supplierRepo.UpdateAsync(newSupplier);
            }

            return _mapper.Map<SupplierResDto>(newSupplier);
        }

        public async Task<bool> UpdateSupplierAsync(int id, SupplierReqDto dto, CurrentUser currentUser)
        {
            // Ensure supplier exists with a lightweight check
            var existing = await _supplierRepo.GetByIdAsync(id);
            if (existing == null)
                throw new ArgumentException($"Supplier with ID {id} was not found.");

            // Check for name conflicts with other suppliers
            if (!string.Equals(existing.Name, dto.Name, StringComparison.Ordinal))
            {
                var other = await _supplierRepo.GetByNameAsync(dto.Name);
                if (other != null && other.Id != id)
                    throw new ArgumentException("This supplier name already exists.");
            }

            // Map scalar fields into a transient Supplier instance for repository update
            var supplierForUpdate = _mapper.Map<Supplier>(dto);
            supplierForUpdate.Id = id;
            supplierForUpdate.UpdatedBy = currentUser?.UserName ?? string.Empty;

            List<Contact>? contactsToSave = null;
            IEnumerable<string>? itemUuidsToSave = null;

            // If DTO provided contacts explicitly, map them (empty list means clear)
            if (dto.Contacts != null)
            {
                contactsToSave = dto.Contacts.Select(c =>
                {
                    var contact = _mapper.Map<Contact>(c);
                    contact.Uuid = Guid.NewGuid().ToString();
                    contact.SupplierId = id;
                    return contact;
                }).ToList();
            }

            // If DTO provided item uuids explicitly, use them (empty list means clear)
            if (dto.ItemUuids != null)
            {
                itemUuidsToSave = dto.ItemUuids;
            }

            // Delegate atomic update to repository which will replace associations as requested
            await _supplierRepo.UpdateWithAssociationsAsync(
                supplierForUpdate,
                contactsToSave  ?? Enumerable.Empty<Contact>(),
                itemUuidsToSave ?? Enumerable.Empty<string>()
            );

            return true;
        }

        // Removed GetSuppliersForDropdownAsync - use GetSuppliersDropdownAsync which returns a minimal DTO.

        public async Task<IEnumerable<SupplierDropdownDto>> GetSuppliersDropdownAsync(CurrentUser currentUser)
        {
            var suppliers = await _supplierRepo.GetAllBasicAsync();

            // Project directly into minimal DTO to avoid unnecessary mapping
            return suppliers.Select(s => new SupplierDropdownDto
            {
                Id = s.Id,
                Name = s.Name
            });
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
