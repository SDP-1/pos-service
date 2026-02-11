using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Suppliers;
using pos_service.Models.Enums;
using pos_service.Repositories;

namespace pos_service.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepo;
        private readonly IItemRepository     _itemRepo;
        private readonly IContactService     _contactService;
        private readonly IMapper             _mapper;

        public SupplierService(
            ISupplierRepository supplierRepo,
            IItemRepository itemRepo,
            IContactService contactService,
            IMapper mapper)
        {
            _supplierRepo   = supplierRepo;
            _itemRepo       = itemRepo;
            _contactService = contactService;
            _mapper         = mapper;
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
                    var contact  = _mapper.Map<Contact>(c);
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

            // Update supplier scalar fields
            existing.Name      = dto.Name;
            existing.Address   = dto.Address;
            existing.IsActive  = dto.IsActive;
            existing.UpdatedBy = currentUser?.UserName ?? string.Empty;

            await _supplierRepo.UpdateAsync(existing);

            // Merge contacts: update existing, add new, delete removed
            if (dto.Contacts != null)
                await _contactService.MergeContactsAsync(ContactOwnerType.Supplier, id, dto.Contacts);

            // Update item associations if provided
            if (dto.ItemUuids != null)
                await UpdateItemAssociationsAsync(id, dto.ItemUuids);

            return true;
        }

        /// <summary>
        /// Updates item associations for a supplier.
        /// Deletes existing associations and creates new ones based on item UUIDs.
        /// </summary>
        private async Task UpdateItemAssociationsAsync(int supplierId, IEnumerable<string> itemUuids)
        {
            await _supplierRepo.DeleteItemAssociationsBySupplierId(supplierId);

            if (itemUuids.Any())
            {
                var items = await _itemRepo.GetByUuidsAsync(itemUuids);
                var itemSuppliers = items.Select(it => new ItemSupplier
                {
                    Uuid        = Guid.NewGuid().ToString(),
                    SuppliersId = supplierId,
                    ItemsId     = it.Id,
                    ItemsSubId  = it.SubId
                }).ToList();

                if (itemSuppliers.Any())
                    await _supplierRepo.AddItemAssociationsAsync(itemSuppliers);
            }
        }

        // Removed GetSuppliersForDropdownAsync - use GetSuppliersDropdownAsync which returns a minimal DTO.

        public async Task<IEnumerable<SupplierDropdownDto>> GetSuppliersDropdownAsync(CurrentUser currentUser)
        {
            var suppliers = await _supplierRepo.GetAllBasicAsync();

            // Project directly into minimal DTO to avoid unnecessary mapping
            return suppliers.Select(s => new SupplierDropdownDto
            {
                Id   = s.Id,
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
