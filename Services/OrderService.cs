using AutoMapper;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Orders;
using pos_service.Models.Enums;
using pos_service.Repositories;

namespace pos_service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IItemRepository _itemRepository;
        private readonly ISettingService _settingService;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public OrderService(IOrderRepository orderRepository, IItemRepository itemRepository, ISettingService settingService, IMapper mapper, AppDbContext context)
        {
            _orderRepository = orderRepository;
            _itemRepository = itemRepository;
            _settingService = settingService;
            _mapper = mapper;
            _context = context;
        }

        public async Task<OrderResDto> CreateOrderAsync(OrderReqDto orderDto, CurrentUser currentUser)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var allowZeroStock = (await _settingService.GetByKeyAsync(SettingKey.AllowZeroStock, currentUser))?.SettingValue ?? false;
                var allowOrdersForLoan = (await _settingService.GetByKeyAsync(SettingKey.AllowOrdesForsLoan, currentUser))?.SettingValue ?? false;

                var orderItems = new List<OrderItem>();
                decimal grossAmount = 0m;
                decimal totalDiscount = 0m;
                decimal totalCost = 0m;
                int itemCount = 0;

                var itemsToUpdate = new Dictionary<Item, decimal>();

                foreach (var itemDto in orderDto.OrderItems)
                {
                    var item = await _itemRepository.GetByUuidAsync(itemDto.ItemUuid);
                    if (item == null)
                        throw new ArgumentException($"Item with UUID {itemDto.ItemUuid} not found");

                    if (!item.AllowsDecimalQuantities && itemDto.Quantity % 1 != 0)
                        throw new ArgumentException($"Item {item.PrintName} does not allow decimal quantities");

                    if (!allowZeroStock && item.StockQuantity < itemDto.Quantity)
                        throw new ArgumentException($"Insufficient stock for item {item.PrintName}. Available: {item.StockQuantity}, Requested: {itemDto.Quantity}");

                    itemsToUpdate[item] = itemDto.Quantity;

                    var basePrice = orderDto.SaleType == SaleType.Wholesale ? item.WholesalePrice : item.RetailPrice;
                    var discountRatio = orderDto.SaleType == SaleType.Wholesale ? item.WholesaleDiscountRatio : item.RetailDiscountRatio;
                    if (itemDto.DiscountRatio > 0)
                        discountRatio = itemDto.DiscountRatio;

                    var priceAfterDiscount = basePrice * (1 - discountRatio / 100);
                    var lineTotal = itemDto.Quantity * priceAfterDiscount;

                    var orderItem = new OrderItem
                    {
                        Uuid = Guid.NewGuid().ToString(),
                        OriginalItemUuid = item.Uuid,
                        AllowsDecimalQuantities = item.AllowsDecimalQuantities,
                        PrintName = item.PrintName,
                        Quantity = itemDto.Quantity,
                        PriceAtSale = basePrice,
                        DiscountRatioAtSale = discountRatio,
                        CostAtSale = item.BuyingPrice,
                        LineTotal = lineTotal
                    };

                    orderItems.Add(orderItem);

                    grossAmount += itemDto.Quantity * basePrice;
                    totalDiscount += itemDto.Quantity * basePrice * (discountRatio / 100);
                    totalCost += itemDto.Quantity * item.BuyingPrice;
                    itemCount++;
                }

                var netAmount = grossAmount - totalDiscount;
                var balance = orderDto.AmountPaid - netAmount;

                if (balance < 0 && !allowOrdersForLoan)
                    throw new InvalidOperationException("Negative balance not allowed. Enable setting AllowOrdesForsLoan to allow credit/loan sales.");

                OrderStatus initialStatus;
                if (balance < 0 && allowOrdersForLoan)
                    initialStatus = OrderStatus.Loan;
                else if (balance >= 0 && orderDto.AmountPaid >= netAmount)
                    initialStatus = OrderStatus.Paid;
                else
                    initialStatus = OrderStatus.Pending;

                var order = new Order
                {
                    OrderNumber = await _orderRepository.GenerateOrderNumberAsync(),
                    Status = initialStatus,
                    PaymentMethod = orderDto.PaymentMethod,
                    SaleType = orderDto.SaleType,
                    ItemCount = itemCount,
                    GrossAmount = grossAmount,
                    TotalDiscount = totalDiscount,
                    NetAmount = netAmount,
                    TotalCost = totalCost,
                    AmountPaid = orderDto.AmountPaid,
                    Balance = balance,
                    CashierId = currentUser.Id,
                    CustomerId = orderDto.CustomerId,
                    OrderItems = orderItems
                };

                var createdOrder = await _orderRepository.CreateAsync(order);

                foreach (var (item, quantity) in itemsToUpdate)
                {
                    if (allowZeroStock)
                    {
                        var deduct = Math.Min(item.StockQuantity, quantity);
                        item.StockQuantity -= deduct;
                    }
                    else
                    {
                        item.StockQuantity -= quantity;
                    }
                    _context.Items.Update(item);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<OrderResDto>(createdOrder);
            }
            catch
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
                var allowZeroStock = (await _settingService.GetByKeyAsync(SettingKey.AllowZeroStock, currentUser))?.SettingValue ?? false;
                var allowOrdersForLoan = (await _settingService.GetByKeyAsync(SettingKey.AllowOrdesForsLoan, currentUser))?.SettingValue ?? false;

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

                var itemsToUpdate = new Dictionary<Item, decimal>();

                foreach (var itemDto in orderDto.OrderItems)
                {
                    var item = await _itemRepository.GetByUuidAsync(itemDto.ItemUuid);
                    if (item == null)
                        throw new ArgumentException($"Item with UUID {itemDto.ItemUuid} not found");

                    // Check stock availability (respect AllowZeroStock setting)
                    if (!allowZeroStock && item.StockQuantity < itemDto.Quantity)
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
                    if (allowZeroStock)
                    {
                        var deduct = Math.Min(item.StockQuantity, quantity);
                        item.StockQuantity -= deduct;
                    }
                    else
                    {
                        item.StockQuantity -= quantity;
                    }

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

                // Enforce loan setting on update
                if (existingOrder.Balance < 0 && !allowOrdersForLoan)
                    throw new InvalidOperationException("Negative balance not allowed. Enable setting AllowOrdesForsLoan to allow credit/loan sales.");

                // Set status: Loan for negative balance when allowed, Paid when fully settled, otherwise Pending
                if (existingOrder.Balance < 0 && allowOrdersForLoan)
                {
                    existingOrder.Status = OrderStatus.Loan;
                }
                else if (existingOrder.Balance >= 0 && existingOrder.AmountPaid >= existingOrder.NetAmount)
                {
                    existingOrder.Status = OrderStatus.Paid;
                }
                else
                {
                    existingOrder.Status = OrderStatus.Pending;
                }
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
                // Restore quantities from order items only when order is active and we care about stock
                if (order.IsActive)
                {
                    var allowZeroStockSetting = await _settingService.GetByKeyAsync(SettingKey.AllowZeroStock, currentUser);
                    var allowZeroStock = allowZeroStockSetting?.SettingValue ?? false;

                    foreach (var orderItem in order.OrderItems)
                    {
                        var item = await _itemRepository.GetByUuidAsync(orderItem.OriginalItemUuid);
                        if (item != null)
                        {
                            // If AllowZeroStock is enabled, do not increase stock when deleting orders (since we may not have reduced it previously)
                            if (!allowZeroStock)
                            {
                                item.StockQuantity += orderItem.Quantity;
                                _context.Items.Update(item);
                            }
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
                var allowZeroStockSetting = await _settingService.GetByKeyAsync(SettingKey.AllowZeroStock, currentUser);
                var allowZeroStock = allowZeroStockSetting?.SettingValue ?? false;

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
                            // Only restore stock when we had previously deducted it (i.e. AllowZeroStock disabled)
                            if (!allowZeroStock)
                            {
                                item.StockQuantity += orderItem.Quantity;
                                _context.Items.Update(item);
                            }
                        }
                    }
                }
                else if (order.Status == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
                {
                    // Reduce stock when order is uncancelled
                    foreach (var orderItem in order.OrderItems)
                    {
                        var item = await _itemRepository.GetByUuidAsync(orderItem.OriginalItemUuid);
                        if (!allowZeroStock)
                        {
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
                        else
                        {
                            // allowZeroStock: only deduct available quantity, never go below zero
                            if (item != null)
                            {
                                var deduct = Math.Min(item.StockQuantity, orderItem.Quantity);
                                item.StockQuantity -= deduct;
                                _context.Items.Update(item);
                            }
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
