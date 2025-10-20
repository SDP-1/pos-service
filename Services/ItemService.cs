using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO;
using pos_service.Repositories;

namespace pos_service.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the ItemService.
        /// </summary>
        public ItemService(
            IItemRepository itemRepository,
            ISupplierRepository supplierRepository,
            IMapper mapper)
        {
            _itemRepository = itemRepository;
            _supplierRepository = supplierRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all items from the database.
        /// </summary>
        public async Task<IEnumerable<ItemResDto>> GetAllItemsAsync()
        {
            var items = await _itemRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ItemResDto>>(items);
        }

        /// <summary>
        /// Retrieves a specific item by its composite key.
        /// </summary>
        public async Task<ItemResDto?> GetItemByIdAsync(int id, int subId)
        {
            var item = await _itemRepository.GetByIdAsync(id, subId);
            return _mapper.Map<ItemResDto?>(item);
        }

        /// <summary>
        /// Creates a new item in the database.
        /// </summary>
        public async Task<ItemResDto?> CreateItemAsync(ItemReqDto itemDto)
        {
            if (await _itemRepository.ItemExistsAsync(itemDto.Id, itemDto.SubId))
            {
                // An item with this composite key already exists.
                return null;
            }

            var item = _mapper.Map<Item>(itemDto);

            // Handle Supplier Linking
            if (itemDto.SupplierIds != null && itemDto.SupplierIds.Any())
            {
                foreach (var supplierId in itemDto.SupplierIds)
                {
                    var supplier = await _supplierRepository.GetByIdAsync(supplierId);
                    if (supplier != null)
                    {
                        item.Suppliers.Add(supplier);
                    }
                }
            }

            var newItem = await _itemRepository.AddAsync(item);
            return _mapper.Map<ItemResDto>(newItem);
        }

        /// <summary>
        /// Updates an existing item.
        /// </summary>
        public async Task<bool> UpdateItemAsync(int id, int subId, ItemReqDto itemDto)
        {
            // Fetch the item with its related suppliers to update them
            var itemToUpdate = await _itemRepository.GetByIdWithSuppliersAsync(id, subId);
            if (itemToUpdate == null)
            {
                // Item not found.
                return false;
            }

            // Map flat properties from DTO to entity
            _mapper.Map(itemDto, itemToUpdate);

            // Handle Supplier Linking
            itemToUpdate.Suppliers.Clear(); // Clear existing links
            if (itemDto.SupplierIds != null && itemDto.SupplierIds.Any())
            {
                foreach (var supplierId in itemDto.SupplierIds)
                {
                    var supplier = await _supplierRepository.GetByIdAsync(supplierId);
                    if (supplier != null)
                    {
                        itemToUpdate.Suppliers.Add(supplier);
                    }
                }
            }

            await _itemRepository.UpdateAsync(itemToUpdate);
            return true;
        }

        /// <summary>
        /// Deletes an item from the database.
        /// </summary>
        public async Task<bool> DeleteItemAsync(int id, int subId)
        {
            var itemToDelete = await _itemRepository.GetByIdAsync(id, subId);
            if (itemToDelete == null)
            {
                // Item not found.
                return false;
            }

            await _itemRepository.DeleteAsync(itemToDelete);
            return true;
        }

        // --- NEW METHODS ---

        /// <summary>
        /// Gets all item variants under a single main ID.
        /// </summary>
        public async Task<IEnumerable<ItemResDto>> GetItemsByMainIdAsync(int id)
        {
            var items = await _itemRepository.GetByMainIdAsync(id);
            return _mapper.Map<IEnumerable<ItemResDto>>(items);
        }

        /// <summary>
        /// Gets a single item by its barcode.
        /// </summary>
        public async Task<IEnumerable<ItemResDto>> GetItemByBarCodeAsync(string barCode)
        {
            var items = await _itemRepository.GetByBarCodeAsync(barCode);
            return _mapper.Map<IEnumerable<ItemResDto>>(items);
        }

        /// <summary>
        /// Gets a single item by its unique Guid (Uuid).
        /// </summary>
        public async Task<ItemResDto?> GetItemByUuidAsync(Guid uuid)
        {
            var item = await _itemRepository.GetByUuidAsync(uuid);
            return _mapper.Map<ItemResDto?>(item);
        }

        /// <summary>
        /// Adds a specified quantity to an item's stock.
        /// </summary>
        public async Task<ItemResDto?> AddStockAsync(int id, int subId, decimal quantity)
        {
            var item = await _itemRepository.GetByIdAsync(id, subId);
            if (item == null)
            {
                return null; // Item not found
            }

            // Handle null StockQuantity by initializing to 0
            item.StockQuantity = item.StockQuantity + quantity;

            await _itemRepository.UpdateAsync(item);
            return _mapper.Map<ItemResDto>(item);
        }

        /// <summary>
        /// Gets the stock quantity for all variants of a main item.
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetQuantitiesByMainIdAsync(int id)
        {
            var items = await _itemRepository.GetByMainIdAsync(id);
            // Creates a dictionary like: { "1001/0": 50, "1001/1": 25 }
            return items.ToDictionary(
                item => $"{item.Id}/{item.SubId}",
                item => item.StockQuantity
            );
        }

        /// <summary>
        /// Gets the stock quantity for an item by its UUID.
        /// </summary>
        public async Task<decimal?> GetQuantityByUuidAsync(Guid uuid)
        {
            var item = await _itemRepository.GetByUuidAsync(uuid);
            // Returns the quantity, or 0 if the item is found but stock is null.
            // Returns null if the item is not found at all.
            return item?.StockQuantity ?? (item != null ? 0m : null);
        }

        /// <summary>
        /// Gets the stock quantity for an item by its composite key.
        /// </summary>
        public async Task<decimal?> GetQuantityByIdAsync(int id, int subId)
        {
            var item = await _itemRepository.GetByIdAsync(id, subId);
            // Returns the quantity, or 0 if the item is found but stock is null.
            // Returns null if the item is not found at all.
            return item?.StockQuantity ?? (item != null ? 0m : null);
        }
    }
}
