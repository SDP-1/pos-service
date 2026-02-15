using AutoMapper;
using pos_service.Data;
using System.Linq;
using pos_service.Models;
using pos_service.Models.DTO.Orders;
using pos_service.Models.Enums;
using pos_service.Repositories;
using Microsoft.Extensions.Logging;
using pos_service.Models.DTO.ReturnedItems;
using pos_service.Models.DTO.OrderItems;

namespace pos_service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IItemRepository _itemRepository;
        private readonly ISettingService _settingService;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository, 
            IItemRepository itemRepository, 
            ISettingService settingService, 
            IMapper mapper, 
            AppDbContext context,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _itemRepository = itemRepository;
            _settingService = settingService;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        public async Task<OrderResDto> RecordSettlementAsync(int orderId, decimal amountPaid, string? description, CurrentUser currentUser)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null) throw new ArgumentException($"Order with ID {orderId} not found");

                if (order.MainStatus != MainOrderStatus.Loan)
                    throw new InvalidOperationException("Only loan orders can accept settlement payments");

                // Validate payment amount: must be positive and must not exceed the remaining due
                var due = order.NetAmount - order.AmountPaid;
                if (due <= 0)
                    throw new InvalidOperationException("Order is already fully settled");
                if (amountPaid <= 0)
                    throw new ArgumentException("AmountPaid must be greater than zero", nameof(amountPaid));
                if (amountPaid > due)
                    throw new InvalidOperationException($"AmountPaid ({amountPaid}) cannot be greater than remaining due amount ({due})");

                // update financials
                order.AmountPaid += amountPaid;
                order.Balance = order.AmountPaid - order.NetAmount;

                // create log
                var remaining = Math.Max(0, order.NetAmount - order.AmountPaid);
                var log = new LoanSettlementLog
                {
                    Uuid = Guid.NewGuid().ToString(),
                    OrderId = order.Id,
                    PaymentDate = DateTime.Now,
                    Description = description,
                    AmountPaid = amountPaid,
                    RemainingBalance = remaining,
                    Status = remaining <= 0 ? LoanSettlementStatus.Completed : LoanSettlementStatus.PartiallySettled
                };

                _context.LoanSettlementLogs.Add(log);

                // If fully settled, update order main status to LoanSettled and balance to 0
                if (remaining <= 0)
                {
                    order.MainStatus = MainOrderStatus.LoanSettled;
                    order.Balance = 0;
                }

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<OrderResDto>(order);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderResDto> CreateOrderAsync(OrderReqDto orderDto, CurrentUser currentUser)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var allowZeroStock                  = await _settingService.GetSettingValueAsync(SettingKey.AllowZeroStock, currentUser);
                var allowOrdersForLoan              = await _settingService.GetSettingValueAsync(SettingKey.AllowOrdesForLoan, currentUser);
                var AllowCreditOrderWithoutCustomer = await _settingService.GetSettingValueAsync(SettingKey.AllowCreditOrderWithoutCustomer, currentUser);
                var calculateLoyaltyForLoanOrders   = await _settingService.GetSettingValueAsync(SettingKey.CalculateLoyaltyPointsForCreditOrders, currentUser);

                var orderItems        = new List<OrderItem>();
                decimal grossAmount   = 0m;
                decimal totalDiscount = 0m;
                decimal totalCost     = 0m;
                int itemCount         = 0;

                var itemsToUpdate   = new Dictionary<Item, decimal>();
                var itemReturnFlags = new Dictionary<Item, bool>();

                foreach (var itemDto in orderDto.OrderItems)
                {
                    var item = await _itemRepository.GetByUuidAsync(itemDto.ItemUuid);
                    if (item == null)
                        throw new ArgumentException($"Item with UUID {itemDto.ItemUuid} not found");

                    if (!item.AllowsDecimalQuantities && itemDto.Quantity % 1 != 0)
                        throw new ArgumentException($"Item {item.PrintName} does not allow decimal quantities");

                    // Stock validation only for non-return items
                    if (!itemDto.IsReturnItem && !allowZeroStock && item.StockQuantity < itemDto.Quantity)
                        throw new ArgumentException($"Insufficient stock for item {item.PrintName}. Available: {item.StockQuantity}, Requested: {itemDto.Quantity}");

                    // Validate ReturnedOrderItemUuid for return items
                    if (itemDto.IsReturnItem && string.IsNullOrWhiteSpace(itemDto.ReturnedOrderItemUuid))
                        throw new ArgumentException($"ReturnedOrderItemUuid is required for return item {item.PrintName}");

                    itemsToUpdate[item]   = itemDto.Quantity;
                    itemReturnFlags[item] = itemDto.IsReturnItem;

                    // Frontend-provided prices and totals (required)
                    var markedPrice   = itemDto.MarkedPrice;
                    var salePrice     = itemDto.SalePrice;
                    var lineTotal     = itemDto.LineTotal;

                    var orderItem = new OrderItem
                    {
                        Uuid                    = Guid.NewGuid().ToString(),
                        OriginalItemUuid        = item.Uuid,
                        AllowsDecimalQuantities = item.AllowsDecimalQuantities,
                        PrintName               = itemDto.PrintName,
                        Quantity                = itemDto.Quantity,
                        PriceAtSale             = salePrice,
                        MarkedPriceAtSale       = markedPrice,
                        CostAtSale              = item.Price?.BuyingPrice ?? 0,
                        LineTotal               = lineTotal,
                        IsReturnItem            = itemDto.IsReturnItem,
                        Description             = itemDto.Description,
                        ReturnedOrderItemUuid   = itemDto.ReturnedOrderItemUuid
                    };

                    orderItems.Add(orderItem);

                    // Only accumulate cost for profit calculation; trust frontend for gross/discount/net/itemCount
                    totalCost += itemDto.Quantity * (item.Price?.BuyingPrice ?? 0);
                }

                // Use frontend-provided (required) order-level totals and item count
                grossAmount   = orderDto.GrossAmount;
                totalDiscount = orderDto.TotalDiscount;
                var netAmount = orderDto.NetAmount;
                itemCount     = orderDto.ItemCount;
                var balance   = orderDto.AmountPaid - netAmount;

                bool hasReturnItems = orderDto.OrderItems.Any(item => item.IsReturnItem);

                if (balance < 0 && !allowOrdersForLoan)
                    throw new InvalidOperationException("Credit(Loan) orders not allowed.");

                // Enforce presence of customer for loan orders unless explicitly allowed
                if (balance < 0 && allowOrdersForLoan && !AllowCreditOrderWithoutCustomer && !orderDto.CustomerId.HasValue)
                    throw new InvalidOperationException("Credit(Loan) orders require a customer.");

                MainOrderStatus initialMainStatus;
                OrderSubStatus? initialSubStatus = null;

                if (hasReturnItems)
                {
                    // Return is a sub-status. Main status depends on payment/loan conditions.
                    initialSubStatus = OrderSubStatus.Return;
                }

                if (balance < 0 && allowOrdersForLoan)
                    initialMainStatus = MainOrderStatus.Loan;
                else if (balance >= 0 && orderDto.AmountPaid >= netAmount)
                    initialMainStatus = MainOrderStatus.Paid;
                else
                    initialMainStatus = MainOrderStatus.Pending;

                var order = new Order
                {
                    OrderNumber   = await _orderRepository.GenerateOrderNumberAsync(),
                    MainStatus    = initialMainStatus,
                    SubStatus     = initialSubStatus,
                    PaymentMethod = orderDto.PaymentMethod,
                    SaleType      = orderDto.SaleType,
                    ItemCount     = itemCount,
                    GrossAmount   = grossAmount,
                    TotalDiscount = totalDiscount,
                    NetAmount     = netAmount,
                    TotalCost     = totalCost,
                    AmountPaid    = orderDto.AmountPaid,
                    Balance       = balance,
                    Description   = orderDto.Description,
                    CashierId     = currentUser.Id,
                    CustomerId    = orderDto.CustomerId,
                    OrderItems    = orderItems
                };

                var createdOrder = await _orderRepository.CreateAsync(order);

                // If this order is a loan (credit), create initial loan settlement log entry representing the created loan (negative balance)
                if (createdOrder.MainStatus == MainOrderStatus.Loan)
                {
                    var initialLog = new LoanSettlementLog
                    {
                        Uuid = Guid.NewGuid().ToString(),
                        OrderId = createdOrder.Id,
                        PaymentDate = DateTime.Now,
                        Description = "Loan created",
                        AmountPaid = createdOrder.AmountPaid,
                        RemainingBalance = Math.Abs(createdOrder.Balance),
                        Status = LoanSettlementStatus.Created
                    };

                    _context.LoanSettlementLogs.Add(initialLog);
                    await _context.SaveChangesAsync();
                }

                foreach (var (item, quantity) in itemsToUpdate)
                {
                    bool isReturn = itemReturnFlags[item];

                    if (isReturn)
                    {
                        // For return items, add the quantity back to stock
                        item.StockQuantity += quantity;
                    }
                    else
                    {
                        // For regular sales, deduct from stock
                        if (allowZeroStock)
                        {
                            var deduct = Math.Min(item.StockQuantity, quantity);
                            item.StockQuantity -= deduct;
                        }
                        else
                        {
                            item.StockQuantity -= quantity;
                        }
                    }
                    _context.Items.Update(item);
                }

                // Update customer loyalty points based on the request DTO (earn/deduct per spec)
                if (order.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(order.CustomerId.Value);
                    if (customer != null)
                    {
                        var suppressEarnForLoan = order.MainStatus == MainOrderStatus.Loan && !calculateLoyaltyForLoanOrders;
                        var points              = CalculateLoyaltyPointsFromReq(orderDto.OrderItems, suppressEarnForLoan);
                        customer.LoyaltyPoints  = Math.Max(0, customer.LoyaltyPoints + points);

                        _context.Customers.Update(customer);
                    }
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

        /// <summary>
        /// Returns order header with order items enriched with returned quantities from the view.
        /// Uses LINQ to query the view-mapped keyless entity and joins in-memory after mapping order.
        /// </summary>
        public async Task<OrderResDto?> GetOrderWithReturnedItemsAsync(string orderNumber, CurrentUser currentUser)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            if (order == null) return null;

            var dto = _mapper.Map<OrderResDto>(order);

            // Load returned items summary rows for this order via repository
            var returnedRows = await _orderRepository.GetReturnedItemsSummaryByOrderNumberAsync(orderNumber);

            // Map returned summary to order items by ReturnedOrderItemUuid. If no return exists, keep ReturnSummary as null.
            foreach (var item in dto.OrderItems)
            {
                if (!string.IsNullOrEmpty(item.Uuid))
                {
                    var match = returnedRows.FirstOrDefault(r => r.ReturnedOrderItemUuid == item.Uuid);
                    item.ReturnSummary = match != null
                        ? _mapper.Map<ReturnedItemsSummaryResDto>(match)
                        : null;
                }
                else
                {
                    item.ReturnSummary = null;
                }
            }

            return dto;
        }

        public async Task<OrderResDto?> GetOrderByUuidAsync(string uuid, CurrentUser currentUser)
        {
            var order = await _orderRepository.GetByUuidAsync(uuid);
            return order == null ? null : _mapper.Map<OrderResDto>(order);
        }

        public async Task<OrderListResDto> GetOrdersAsync(OrderQueryDto query, CurrentUser currentUser)
        {
            var orders     = await _orderRepository.GetAllAsync(query);
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
                var allowZeroStock                  = await _settingService.GetSettingValueAsync(SettingKey.AllowZeroStock, currentUser);
                var allowOrdersForLoan              = await _settingService.GetSettingValueAsync(SettingKey.AllowOrdesForLoan, currentUser);
                var AllowCreditOrderWithoutCustomer = await _settingService.GetSettingValueAsync(SettingKey.AllowCreditOrderWithoutCustomer, currentUser);
                var calculateLoyaltyForLoanOrders   = await _settingService.GetSettingValueAsync(SettingKey.CalculateLoyaltyPointsForCreditOrders, currentUser);

                var existingOrder = await _orderRepository.GetByIdAsync(id);
                if (existingOrder == null)
                    throw new ArgumentException($"Order with ID {id} not found");

                if (existingOrder.MainStatus != MainOrderStatus.Pending)
                    throw new InvalidOperationException("Only pending orders can be modified");

                // Capture old order items for loyalty points adjustment, then restore quantities
                var oldOrderItemsSnapshot = existingOrder.OrderItems.Select(oi => new OrderItem
                {
                    LineTotal = oi.LineTotal,
                    IsReturnItem = oi.IsReturnItem
                }).ToList();

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
                decimal grossAmount   = 0;
                decimal totalDiscount = 0;
                decimal totalCost     = 0;
                int itemCount         = 0;

                var itemsToUpdate = new Dictionary<Item, decimal>();
                var itemReturnFlags = new Dictionary<Item, bool>();

                foreach (var itemDto in orderDto.OrderItems)
                {
                    var item = await _itemRepository.GetByUuidAsync(itemDto.ItemUuid);
                    if (item == null)
                        throw new ArgumentException($"Item with UUID {itemDto.ItemUuid} not found");

                    // Check stock availability (respect AllowZeroStock setting)
                    if (!allowZeroStock && item.StockQuantity < itemDto.Quantity)
                        throw new ArgumentException($"Insufficient stock for item {item.PrintName}. Available: {item.StockQuantity}, Requested: {itemDto.Quantity}");

                    // Add to tracking dictionary
                    itemsToUpdate[item]   = itemDto.Quantity;
                    itemReturnFlags[item] = itemDto.IsReturnItem;

                    // Use frontend-provided prices directly
                    var markedPrice = itemDto.MarkedPrice;
                    var salePrice   = itemDto.SalePrice;
                    var lineTotal   = itemDto.LineTotal;

                    var orderItem = new OrderItem
                    {
                        Uuid                    = Guid.NewGuid().ToString(),
                        OriginalItemUuid        = item.Uuid,
                        AllowsDecimalQuantities = item.AllowsDecimalQuantities,
                        PrintName               = item.PrintName,
                        Quantity                = itemDto.Quantity,
                        PriceAtSale             = salePrice,
                        MarkedPriceAtSale       = markedPrice,
                        CostAtSale              = item.Price?.BuyingPrice ?? 0,
                        LineTotal               = lineTotal,
                        IsReturnItem            = itemDto.IsReturnItem,
                        Description             = itemDto.Description
                    };

                    existingOrder.OrderItems.Add(orderItem);

                    // Only accumulate cost for profit calculation; trust frontend for gross/discount/net/itemCount
                    totalCost += itemDto.Quantity * (item.Price?.BuyingPrice ?? 0);
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

                // Use frontend-provided (required) order totals
                grossAmount   = orderDto.GrossAmount;
                totalDiscount = orderDto.TotalDiscount;
                var netAmount = orderDto.NetAmount;

                // Update order totals
                existingOrder.PaymentMethod = orderDto.PaymentMethod;
                existingOrder.SaleType      = orderDto.SaleType;
                existingOrder.ItemCount     = itemCount;
                existingOrder.GrossAmount   = grossAmount;
                existingOrder.TotalDiscount = totalDiscount;
                existingOrder.NetAmount     = netAmount;
                existingOrder.TotalCost     = totalCost;
                existingOrder.AmountPaid    = orderDto.AmountPaid;
                existingOrder.Balance       = orderDto.AmountPaid - netAmount;

                // Enforce loan setting on update
                if (existingOrder.Balance < 0 && !allowOrdersForLoan)
                    throw new InvalidOperationException("Credit (Loan) orders not allowed.");

                // Enforce presence of customer for loan orders unless allowed by setting
                if (existingOrder.Balance < 0 && allowOrdersForLoan && !AllowCreditOrderWithoutCustomer && !orderDto.CustomerId.HasValue)
                    throw new InvalidOperationException("Loan orders require a customer.");

                // Set main/sub status: Loan for negative balance when allowed, Paid when fully settled, otherwise Pending
                OrderSubStatus? newSubStatus = null;
                if (existingOrder.OrderItems.Any(oi => oi.IsReturnItem))
                    newSubStatus = OrderSubStatus.Return;

                if (existingOrder.Balance < 0 && allowOrdersForLoan)
                {
                    existingOrder.MainStatus = MainOrderStatus.Loan;
                }
                else if (existingOrder.Balance >= 0 && existingOrder.AmountPaid >= existingOrder.NetAmount)
                {
                    existingOrder.MainStatus = pos_service.Models.Enums.MainOrderStatus.Paid;
                }
                else
                {
                    existingOrder.MainStatus = pos_service.Models.Enums.MainOrderStatus.Pending;
                }

                existingOrder.SubStatus = newSubStatus;
                existingOrder.CustomerId = orderDto.CustomerId;

                var updatedOrder = await _orderRepository.UpdateAsync(existingOrder);
                // Adjust customer loyalty points based on delta between new and old order items
                if (existingOrder.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(existingOrder.CustomerId.Value);
                    if (customer != null)
                    {
                        var suppressEarnForLoan = existingOrder.MainStatus == MainOrderStatus.Loan && !calculateLoyaltyForLoanOrders;
                        var oldPoints           = CalculateLoyaltyPointsFromOrderItems(oldOrderItemsSnapshot, false);
                        var newPoints           = CalculateLoyaltyPointsFromOrderItems(existingOrder.OrderItems, suppressEarnForLoan);
                        var delta               = newPoints - oldPoints;
                        customer.LoyaltyPoints  = Math.Max(0, customer.LoyaltyPoints + delta);

                        _context.Customers.Update(customer);
                    }
                }

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
                    var allowZeroStock = await _settingService.GetSettingValueAsync(SettingKey.AllowZeroStock, currentUser);

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

        public async Task<OrderResDto> UpdateOrderStatusAsync(int id, MainOrderStatus status, CurrentUser currentUser)
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
                if (status == MainOrderStatus.Cancelled && order.MainStatus != MainOrderStatus.Cancelled)
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
                if (order.MainStatus == pos_service.Models.Enums.MainOrderStatus.Cancelled && status != pos_service.Models.Enums.MainOrderStatus.Cancelled)
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

                order.MainStatus = status;

                // When un-cancelling or updating main status, if there are return items keep SubStatus; otherwise clear when moving away
                if (order.OrderItems.Any(oi => oi.IsReturnItem))
                {
                    order.SubStatus = pos_service.Models.Enums.OrderSubStatus.Return;
                }
                else
                {
                    order.SubStatus = null;
                }

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


        /// <summary>
        /// Retrieves orders filtered by date range and status.
        /// Key behaviors:
        /// - If StartDate is null, defaults to today's date
        /// - If StartDate > EndDate, automatically swaps the dates
        /// - Returns all active orders in the given date range
        /// - Status filter is optional; if null, returns all statuses
        /// - Results are ordered by CreatedAt in descending order
        /// - Includes related data: OrderItems, Cashier, and Customer
        /// </summary>
        /// <param name="startDate">Start date for the date range filter. Defaults to today if not provided.</param>
        /// <param name="endDate">End date for the date range filter. If null, includes all dates from startDate onwards.</param>
        /// <param name="status">Order status filter. If null, returns all statuses.</param>
        /// <returns>List of active orders matching the criteria, ordered by CreatedAt descending.</returns>
        public async Task<List<OrderResDto>> GetOrdersByDateAndStatusAsync(DateTime? startDate, DateTime? endDate, pos_service.Models.Enums.MainOrderStatus? status, pos_service.Models.Enums.OrderSubStatus? subStatus, CurrentUser currentUser)
        {
            try
            {
                // Call stored procedure via repository
                var orders = await _orderRepository.GetOrdersByDateAndStatusAsync(startDate, endDate, status, subStatus);

                // Map to DTOs
                return _mapper.Map<List<OrderResDto>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by date and status for user {UserId}: StartDate={StartDate}, EndDate={EndDate}, Status={Status}", 
                    currentUser.Id, startDate, endDate, status);
                throw;
            }
        }

        public async Task<List<pos_service.Models.DTO.ReturnedItems.ReturnedItemsSummaryResDto>> GetReturnedItemsSummaryByOrderNumberAsync(string orderNumber, CurrentUser currentUser)
        {
            // Use repository to get view-backed rows
            var rows = await _orderRepository.GetReturnedItemsSummaryByOrderNumberAsync(orderNumber);
            return _mapper.Map<List<pos_service.Models.DTO.ReturnedItems.ReturnedItemsSummaryResDto>>(rows);
        }

        // Calculate loyalty points earned/deducted for a collection of OrderItemReqDto.
        // Rules:
        // - Earn 1 point per 100 Rs for non-return items (integer points only)
        // - Deduct 2 points per 100 Rs for return items
        // - Returns a signed integer (positive => add points, negative => remove points)
        private int CalculateLoyaltyPointsFromReq(IEnumerable<OrderItemReqDto> items, bool suppressEarn = false)
        {
            // Use absolute values so returned line totals (which may be negative) always reduce points.
            var saleTotal   = items.Where(i => !i.IsReturnItem).Sum(i => Math.Abs(i.LineTotal));
            var returnTotal = items.Where(i => i.IsReturnItem).Sum(i => Math.Abs(i.LineTotal));

            int earn        = suppressEarn ? 0 : (int)(saleTotal / 100m);
            int deduct      = (int)(returnTotal / 100m) * 2;

            return earn - deduct;
        }

        // Same calculation but for persisted OrderItem entities
        private int CalculateLoyaltyPointsFromOrderItems(IEnumerable<OrderItem> items, bool suppressEarn = false)
        {
            // Use absolute values so returned line totals (which may be negative) always reduce points.
            var saleTotal   = items.Where(i => !i.IsReturnItem).Sum(i => Math.Abs(i.LineTotal));
            var returnTotal = items.Where(i => i.IsReturnItem).Sum(i => Math.Abs(i.LineTotal));

            int earn        = suppressEarn ? 0 : (int)(saleTotal / 100m);
            int deduct      = (int)(returnTotal / 100m) * 2;

            return earn - deduct;
        }
    }
}
