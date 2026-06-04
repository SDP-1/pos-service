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

        /// <summary>
        /// Retrieves all suppliers from the system.
        /// </summary>
        public async Task<IEnumerable<SupplierResDto>> GetAllSuppliersAsync(CurrentUser currentUser)
        {
            return await _supplierRepo.GetAllAsync();
        }

        /// <summary>
        /// Retrieves a specific supplier by its unique identifier.
        /// </summary>
        public async Task<SupplierResDto?> GetSupplierByIdAsync(int id, CurrentUser currentUser)
        {
            return await _supplierRepo.GetByIdWithDetailsAsync(id);
        }

        /// <summary>
        /// Retrieves a specific supplier along with its associated items by supplier id.
        /// </summary>
        public async Task<SupplierResDto?> GetSupplierWithItemsAsync(int id, CurrentUser currentUser)
        {
            return await _supplierRepo.GetSupplierWithItemsAsync(id);
        }

        /// <summary>
        /// Creates a new supplier including optional contacts and item associations.
        /// Validates uniqueness of the supplier name and persists contacts and item links.
        /// </summary>
        /// <param name="dto">Supplier creation DTO containing name, contacts and item UUIDs.</param>
        /// <param name="currentUser">The current user performing the operation.</param>
        /// <returns>The created supplier represented as SupplierResDto.</returns>
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

        /// <summary>
        /// Updates an existing supplier's details, contacts and item associations.
        /// Performs uniqueness check for the supplier name and merges contact list.
        /// </summary>
        /// <param name="id">The identifier of the supplier to update.</param>
        /// <param name="dto">Supplier update DTO containing new values.</param>
        /// <param name="currentUser">The current user performing the update.</param>
        /// <returns>True if the update succeeded.</returns>
        public async Task<bool> UpdateSupplierAsync(int id, SupplierReqDto dto, CurrentUser currentUser)
        {
            // Ensure supplier exists and load tracked entity with related data
            var existing = await _supplierRepo.GetSupplierByIdAsync(id);
            if (existing == null)
                throw new ArgumentException($"Supplier with ID {id} was not found.");

            // Check for name conflicts with other suppliers
            if (!string.Equals(existing.Name, dto.Name, StringComparison.Ordinal))
            {
                var other = await _supplierRepo.GetByNameAsync(dto.Name);
                if (other != null && other.Id != id)
                    throw new ArgumentException("This supplier name already exists.");
            }


            _mapper.Map(dto, existing);

            await _supplierRepo.UpdateAsync(existing);

            // Merge contacts: update existing, add new, delete removed
            // For PUT updates, a null/empty contacts payload should clear supplier contacts.
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

        /// <summary>
        /// Retrieves lightweight supplier data (Id and Name) for dropdown lists.
        /// Uses a minimal DTO projection to avoid loading full supplier details.
        /// </summary>
        /// <param name="currentUser">The current user requesting the dropdown data.</param>
        /// <returns>A collection of SupplierDropdownDto containing Id and Name.</returns>
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

        /// <summary>
        /// Deletes a supplier by id. Returns false if the supplier was not found.
        /// </summary>
        /// <param name="id">The identifier of the supplier to delete.</param>
        /// <param name="currentUser">The current user performing the deletion.</param>
        /// <returns>True if deletion was successful; otherwise false.</returns>
        public async Task<bool> DeleteSupplierAsync(int id, CurrentUser currentUser)
        {
            var supplier = await _supplierRepo.GetByIdAsync(id);
            if (supplier == null) return false;

            await _supplierRepo.DeleteAsync(new Supplier { Id = supplier.Id });
            return true;
        }
    }
}
