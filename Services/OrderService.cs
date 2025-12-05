using AutoMapper;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Order;
using pos_service.Models.Enums;
using pos_service.Repositories;

namespace pos_service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IItemRepository  _itemRepository;
        private readonly IMapper          _mapper;
        private readonly AppDbContext     _context; 

        public OrderService(IOrderRepository orderRepository, IItemRepository itemRepository, IMapper mapper, AppDbContext context)
        {
            _orderRepository = orderRepository;
            _itemRepository  = itemRepository;
            _mapper          = mapper;
            _context         = context;
        }

        public async Task<OrderResDto> CreateOrderAsync(OrderReqDto orderDto, CurrentUser currentUser)
        {
            // Start a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate items and get current item data
                var orderItems        = new List<OrderItem>();
                decimal grossAmount   = 0;
                decimal totalDiscount = 0;
                decimal totalCost     = 0;
                int itemCount         = 0;

                // Dictionary to track items and their quantities for stock reduction
                var itemsToUpdate = new Dictionary<Item, decimal>();

                foreach (var itemDto in orderDto.OrderItems)
                {
                    var item = await _itemRepository.GetByUuidAsync(itemDto.ItemUuid);
                    if (item == null)
                        throw new ArgumentException($"Item with UUID {itemDto.ItemUuid} not found");

                    if (!item.AllowsDecimalQuantities && itemDto.Quantity % 1 != 0)
                        throw new ArgumentException($"Item {item.PrintName} does not allow decimal quantities");

                    // Check stock availability
                    if (item.StockQuantity < itemDto.Quantity)
                        throw new ArgumentException($"Insufficient stock for item {item.PrintName}. Available: {item.StockQuantity}, Requested: {itemDto.Quantity}");

                    // Add to tracking dictionary for later update
                    itemsToUpdate[item] = itemDto.Quantity;

                    // Calculate prices based on sale type
                    var basePrice = orderDto.SaleType == SaleType.Wholesale ? item.WholesalePrice : item.RetailPrice;
                    var discountRatio = orderDto.SaleType == SaleType.Wholesale ?
                        item.WholesaleDiscountRatio : item.RetailDiscountRatio;

                    // Use provided discount ratio if specified
                    if (itemDto.DiscountRatio > 0)
                        discountRatio = itemDto.DiscountRatio;

                    var priceAfterDiscount = basePrice * (1 - discountRatio / 100);
                    var lineTotal = itemDto.Quantity * priceAfterDiscount;

                    var orderItem = new OrderItem
                    {
                        Uuid                    = Guid.NewGuid().ToString(),
                        OriginalItemUuid        = item.Uuid,
                        AllowsDecimalQuantities = item.AllowsDecimalQuantities,
                        PrintName           = item.PrintName,
                        Quantity                = itemDto.Quantity,
                        PriceAtSale             = basePrice,
                        DiscountRatioAtSale     = discountRatio,
                        CostAtSale              = item.BuyingPrice,
                        LineTotal               = lineTotal,
                    };

                    orderItems.Add(orderItem);

                    grossAmount   += itemDto.Quantity * basePrice;
                    totalDiscount += itemDto.Quantity * basePrice * (discountRatio / 100);
                    totalCost     += itemDto.Quantity * item.BuyingPrice;
                    itemCount     ++;
                }

                var netAmount = grossAmount - totalDiscount;
                var balance = orderDto.AmountPaid - netAmount;

                // Create the order
                var order = new Order
                {
                    OrderNumber   = await _orderRepository.GenerateOrderNumberAsync(),
                    Status        = OrderStatus.Pending,
                    PaymentMethod = orderDto.PaymentMethod,
                    SaleType      = orderDto.SaleType,
                    ItemCount     = itemCount,
                    GrossAmount   = grossAmount,
                    TotalDiscount = totalDiscount,
                    NetAmount     = netAmount,
                    TotalCost     = totalCost,
                    AmountPaid    = orderDto.AmountPaid,
                    Balance       = balance,
                    CashierId     = currentUser.Id,
                    CustomerId    = orderDto.CustomerId,
                    OrderItems    = orderItems,
                };

                // Save the order first
                var createdOrder = await _orderRepository.CreateAsync(order);

                // Update item quantities
                foreach (var (item, quantity) in itemsToUpdate)
                {
                    item.StockQuantity -= quantity;
                    _context.Items.Update(item);
                }

                // Save all changes (order and item updates)
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return _mapper.Map<OrderResDto>(createdOrder);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderResDto?> GetOrderAsync(int id, CurrentUser currentUser)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order == null ? null : _mapper.Map<OrderResDto>(order);
        }
        public async Task<OrderResDto?> GetOrderByOrderNumberAsync(string orderNumber, CurrentUser currentUser)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            return order == null ? null : _mapper.Map<OrderResDto>(order);
        }

        public async Task<OrderResDto?> GetOrderByUuidAsync(string uuid, CurrentUser currentUser)
        {
            var order = await _orderRepository.GetByUuidAsync(uuid);
            return order == null ? null : _mapper.Map<OrderResDto>(order);
        }

        public async Task<OrderListResDto> GetOrdersAsync(OrderQueryDto query, CurrentUser currentUser)
        {
            var orders = await _orderRepository.GetAllAsync(query);
            var totalCount = await _orderRepository.GetCountAsync(query);

            return new OrderListResDto
            {
                Orders     = _mapper.Map<List<OrderSummaryResDto>>(orders),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize   = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };
        }

        public async Task<OrderResDto> UpdateOrderAsync(int id, OrderReqDto orderDto, CurrentUser currentUser)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingOrder = await _orderRepository.GetByIdAsync(id);
                if (existingOrder == null)
                    throw new ArgumentException($"Order with ID {id} not found");

                if (existingOrder.Status != OrderStatus.Pending)
                    throw new InvalidOperationException("Only pending orders can be modified");

                // Restore quantities from old order items
                foreach (var oldItem in existingOrder.OrderItems)
                {
                    var item = await _itemRepository.GetByUuidAsync(oldItem.OriginalItemUuid);
                    if (item != null)
                    {
                        item.StockQuantity += oldItem.Quantity;
                        _context.Items.Update(item);
                    }
                }

                // Clear old items
                existingOrder.OrderItems.Clear();

                // For simplicity, we'll recreate the order items
                // In a real scenario, you might want to handle updates more granularly
                decimal grossAmount = 0;
                decimal totalDiscount = 0;
                decimal totalCost = 0;
                int itemCount = 0;

                // Dictionary to track items and their quantities for stock reduction
                var itemsToUpdate = new Dictionary<Item, decimal>();

                foreach (var itemDto in orderDto.OrderItems)
                {
                    if (itemDto.IsDeleted == true) continue;

                    var item = await _itemRepository.GetByUuidAsync(itemDto.ItemUuid);
                    if (item == null)
                        throw new ArgumentException($"Item with UUID {itemDto.ItemUuid} not found");

                    // Check stock availability
                    if (item.StockQuantity < itemDto.Quantity)
                        throw new ArgumentException($"Insufficient stock for item {item.PrintName}. Available: {item.StockQuantity}, Requested: {itemDto.Quantity}");

                    // Add to tracking dictionary
                    itemsToUpdate[item] = itemDto.Quantity;

                    var basePrice = orderDto.SaleType == SaleType.Wholesale ? item.WholesalePrice : item.RetailPrice;
                    var discountRatio = orderDto.SaleType == SaleType.Wholesale ?
                        item.WholesaleDiscountRatio : item.RetailDiscountRatio;

                    if (itemDto.DiscountRatio > 0)
                        discountRatio = itemDto.DiscountRatio;

                    var priceAfterDiscount = basePrice * (1 - discountRatio / 100);
                    var lineTotal = itemDto.Quantity * priceAfterDiscount;

                    var orderItem = new OrderItem
                    {
                        Uuid                    = Guid.NewGuid().ToString(),
                        OriginalItemUuid        = item.Uuid,
                        AllowsDecimalQuantities = item.AllowsDecimalQuantities,
                        PrintName               = item.PrintName,
                        Quantity                = itemDto.Quantity,
                        PriceAtSale             = basePrice,
                        DiscountRatioAtSale     = discountRatio,
                        CostAtSale              = item.BuyingPrice,
                        LineTotal               = lineTotal
                    };

                    existingOrder.OrderItems.Add(orderItem);

                    grossAmount += itemDto.Quantity * basePrice;
                    totalDiscount += itemDto.Quantity * basePrice * (discountRatio / 100);
                    totalCost += itemDto.Quantity * item.BuyingPrice;
                    itemCount++;
                }

                // Update item quantities
                foreach (var (item, quantity) in itemsToUpdate)
                {
                    item.StockQuantity -= quantity;
                    _context.Items.Update(item);
                }

                // Update order totals
                existingOrder.PaymentMethod = orderDto.PaymentMethod;
                existingOrder.SaleType = orderDto.SaleType;
                existingOrder.ItemCount = itemCount;
                existingOrder.GrossAmount = grossAmount;
                existingOrder.TotalDiscount = totalDiscount;
                existingOrder.NetAmount = grossAmount - totalDiscount;
                existingOrder.TotalCost = totalCost;
                existingOrder.AmountPaid = orderDto.AmountPaid;
                existingOrder.Balance = orderDto.AmountPaid - (grossAmount - totalDiscount);
                existingOrder.CustomerId = orderDto.CustomerId;

                var updatedOrder = await _orderRepository.UpdateAsync(existingOrder);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return _mapper.Map<OrderResDto>(updatedOrder);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteOrderAsync(int id, CurrentUser currentUser, bool isPermanent = false)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var isActiveOnly = !isPermanent;

            try
            {
                var order = await _orderRepository.GetByIdAsync(id, isActiveOnly);
                if (order == null) return false;

                // Restore quantities from order items
                if (order.IsActive)
                {
                    foreach (var orderItem in order.OrderItems)
                    {
                        var item = await _itemRepository.GetByUuidAsync(orderItem.OriginalItemUuid);
                        if (item != null)
                        {
                            item.StockQuantity += orderItem.Quantity;
                            _context.Items.Update(item);
                        }
                    }

                    // Soft delete the order
                    order.IsActive = false;

                    await _context.SaveChangesAsync();
                }
                
                if (isPermanent)
                    await _orderRepository.DeleteAsync(order);

                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderResDto> UpdateOrderStatusAsync(int id, OrderStatus status, CurrentUser currentUser)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                    throw new ArgumentException($"Order with ID {id} not found");

                // Handle status-specific logic
                if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
                {
                    // Restore stock when order is cancelled
                    foreach (var orderItem in order.OrderItems)
                    {
                        var item = await _itemRepository.GetByUuidAsync(orderItem.OriginalItemUuid);
                        if (item != null)
                        {
                            item.StockQuantity += orderItem.Quantity;
                            _context.Items.Update(item);
                        }
                    }
                }
                else if (order.Status == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
                {
                    // Reduce stock when order is uncancelled
                    foreach (var orderItem in order.OrderItems)
                    {
                        var item = await _itemRepository.GetByUuidAsync(orderItem.OriginalItemUuid);
                        if (item != null && item.StockQuantity < orderItem.Quantity)
                        {
                            throw new InvalidOperationException($"Insufficient stock for item {item.PrintName}. Available: {item.StockQuantity}, Required: {orderItem.Quantity}");
                        }

                        if (item != null)
                        {
                            item.StockQuantity -= orderItem.Quantity;
                            _context.Items.Update(item);
                        }
                    }
                }

                order.Status = status;

                var updatedOrder = await _orderRepository.UpdateAsync(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return _mapper.Map<OrderResDto>(updatedOrder);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
