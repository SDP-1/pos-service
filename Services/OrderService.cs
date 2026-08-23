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
using pos_service.Services.Common.Cache;

namespace pos_service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository          _orderRepository;
        private readonly IItemRepository           _itemRepository;
        private readonly IInventoryBatchRepository _batchRepository;
        private readonly ICustomerRepository       _customerRepository;
        private readonly ISettingService           _settingService;
        private readonly ICacheService             _cache;
        private readonly IMapper                   _mapper;
        private readonly ILogger<OrderService>     _logger;

        public OrderService(
            IOrderRepository orderRepository, 
            IItemRepository itemRepository, 
            IInventoryBatchRepository batchRepository,
            ICustomerRepository customerRepository,
            ISettingService settingService,
            ICacheService cache,
            IMapper mapper, 
            ILogger<OrderService> logger)
        {
            _orderRepository     = orderRepository;
            _itemRepository      = itemRepository;
            _batchRepository     = batchRepository;
            _customerRepository  = customerRepository;
            _settingService      = settingService;
            _cache               = cache;
            _mapper              = mapper;
            _logger              = logger;
        }
        /// <summary>
        /// Records a settlement payment for a loan order. Validates amounts and updates order financials.
        /// Commits a LoanSettlementLog entry and updates the order balance and status when fully settled.
        /// </summary>
        /// <param name="orderId">Target order identifier.</param>
        /// <param name="amountPaid">Amount paid in this settlement.</param>
        /// <param name="description">Optional description for the settlement log.</param>
        /// <param name="currentUser">Current user performing the operation.</param>
        /// <returns>Updated order DTO after recording the settlement.</returns>

        public async Task<OrderResDto> RecordSettlementAsync(int orderId, decimal amountPaid, string? description, CurrentUser currentUser)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new ArgumentException($"Order with ID {orderId} not found");

            // Only orders in credit/loan status are eligible for debt settlement
            if (order.MainStatus != MainOrderStatus.Loan)
                throw new InvalidOperationException("Only loan orders can accept settlement payments");

            // Validate payment amount against outstanding unpaid balance
            var due = order.NetAmount - order.AmountPaid;
            if (due <= 0)
                throw new InvalidOperationException("Order is already fully settled");
            if (amountPaid <= 0)
                throw new ArgumentException("AmountPaid must be greater than zero", nameof(amountPaid));
            if (amountPaid > due)
                throw new InvalidOperationException($"AmountPaid ({amountPaid}) cannot be greater than remaining due amount ({due})");

            // Update cumulative payment and recalculate remaining balance
            order.AmountPaid += amountPaid;
            order.Balance = order.AmountPaid - order.NetAmount;

            // Generate immutable audit log entry for this payment instalment
            var remaining = Math.Max(0, order.NetAmount - order.AmountPaid);
            var log = new LoanSettlementLog
            {
                Uuid             = Guid.NewGuid().ToString(),
                OrderId          = order.Id,
                PaymentDate      = DateTime.Now,
                Description      = description,
                AmountPaid       = amountPaid,
                RemainingBalance = remaining,
                Status           = remaining <= 0 ? LoanSettlementStatus.Completed : LoanSettlementStatus.PartiallySettled
            };

            // Transition order to LoanSettled status when debt is completely cleared
            if (remaining <= 0)
            {
                order.MainStatus = MainOrderStatus.LoanSettled;
                order.Balance = 0;
            }

            await _orderRepository.SaveRecordSettlementAsync(order, log);

            return _mapper.Map<OrderResDto>(order);
        }

        /// <summary>
        /// Creates a new order in the system.
        /// Validates stock, customer and settings, persists order and adjusts inventory and loyalty points.
        /// </summary>
        /// <param name="orderDto">The order request DTO containing items and totals.</param>
        /// <param name="currentUser">The current user creating the order.</param>
        /// <returns>The created order details.</returns>
        public async Task<OrderResDto> CreateOrderAsync(OrderReqDto orderDto, CurrentUser currentUser)
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

            var batchesToUpdate     = new List<InventoryBatch>();
            var stockMovementsToAdd = new List<StockMovement>();

            foreach (var itemDto in orderDto.OrderItems)
            {
                var item = await _itemRepository.GetByUuidAsync(itemDto.ItemUuid);
                if (item == null)
                    throw new ArgumentException($"Item with UUID {itemDto.ItemUuid} not found");

                var allowsDecimal = item.AllowsDecimalQuantities;

                if (!allowsDecimal && itemDto.Quantity % 1 != 0)
                    throw new ArgumentException($"Item {item.PrintName} does not allow decimal quantities");

                var availableStock = (await _batchRepository.GetActiveBatchesByItemUuidAsync(item.Uuid, false)).Sum(b => b.RemainingQuantity);

                // Stock validation only for non-return items
                if (!itemDto.IsReturnItem && !allowZeroStock && availableStock < itemDto.Quantity)
                    throw new ArgumentException($"Insufficient stock for item {item.PrintName}. Available: {availableStock}, Requested: {itemDto.Quantity}");

                // Validate ReturnedOrderItemUuid for return items
                if (itemDto.IsReturnItem && string.IsNullOrWhiteSpace(itemDto.ReturnedOrderItemUuid))
                    throw new ArgumentException($"ReturnedOrderItemUuid is required for return item {item.PrintName}");

                itemsToUpdate[item]   = itemDto.Quantity;
                itemReturnFlags[item] = itemDto.IsReturnItem;

                // Frontend-provided prices and totals (required)
                var markedPrice   = itemDto.MarkedPrice;
                var salePrice     = itemDto.SalePrice;
                var lineTotal     = itemDto.LineTotal;

                // ═══ BATCH ALLOCATION & COST AT SALE RESOLUTION ═══
                string? allocatedBatchUuid = null;
                decimal costAtSale = 0m;

                if (!itemDto.IsReturnItem)
                {
                    InventoryBatch? batch = null;

                    // 1. Manual cashier override if specified
                    if (!string.IsNullOrWhiteSpace(itemDto.BatchUuid))
                    {
                        batch = await _batchRepository.GetByUuidAsync(itemDto.BatchUuid);
                    }

                    // 2. Automatic FEFO fallback if no manual batch specified or found
                    if (batch == null)
                    {
                        var fefoBatches = await _batchRepository.GetFefoBatchesAsync(item.Uuid, itemDto.Quantity);
                        batch = fefoBatches.FirstOrDefault(b => b.RemainingQuantity > 0)
                             ?? fefoBatches.FirstOrDefault();
                    }

                    if (batch == null)
                    {
                        var itemBatches = await _batchRepository.GetActiveBatchesByItemUuidAsync(item.Uuid, includeExpired: true);
                        batch = itemBatches.FirstOrDefault();
                    }

                    if (batch != null)
                    {
                        allocatedBatchUuid = batch.Uuid;
                        costAtSale         = batch.CostPrice;

                        // Deduct batch remaining stock (clamped to 0)
                        batch.RemainingQuantity = Math.Max(0m, batch.RemainingQuantity - itemDto.Quantity);
                        if (batch.RemainingQuantity <= 0m)
                        {
                            batch.RemainingQuantity = 0m;
                            if (!allowZeroStock)
                            {
                                batch.Status = BatchStatus.Depleted;
                            }
                        }

                        if (!batchesToUpdate.Any(b => b.Id == batch.Id))
                        {
                            batchesToUpdate.Add(batch);
                        }

                        // Create Sale stock movement
                        stockMovementsToAdd.Add(new StockMovement
                        {
                            Uuid          = Guid.NewGuid().ToString(),
                            BatchUuid     = batch.Uuid,
                            ItemUuid      = item.Uuid,
                            MovementType  = StockMovementType.SALE,
                            Quantity      = itemDto.Quantity,
                            Direction     = StockMovementDirection.OUT,
                            CostPrice     = batch.CostPrice,
                            ReferenceType = StockMovementReferenceType.ORDER,
                            Reason        = "POS sale item",
                            CreatedAt     = DateTime.UtcNow,
                            CreatedBy     = currentUser.Uuid
                        });
                    }
                }
                else
                {
                    // Return Item batch handling
                    InventoryBatch? returnBatch = null;

                    // 1. Explicit BatchUuid from request
                    if (!string.IsNullOrWhiteSpace(itemDto.BatchUuid))
                    {
                        returnBatch = await _batchRepository.GetByUuidAsync(itemDto.BatchUuid);
                    }

                    // 2. Lookup original OrderItem's batch if not provided or not found
                    if (returnBatch == null && !string.IsNullOrWhiteSpace(itemDto.ReturnedOrderItemUuid))
                    {
                        var originalOrderItem = await _orderRepository.GetOrderItemByUuidAsync(itemDto.ReturnedOrderItemUuid);
                        if (originalOrderItem != null && !string.IsNullOrWhiteSpace(originalOrderItem.BatchUuid))
                        {
                            returnBatch = await _batchRepository.GetByUuidAsync(originalOrderItem.BatchUuid);
                        }
                    }

                    // 3. Fallback to item's active or latest batch
                    if (returnBatch == null)
                    {
                        var itemBatches = await _batchRepository.GetActiveBatchesByItemUuidAsync(item.Uuid, includeExpired: true);
                        returnBatch = itemBatches.OrderByDescending(b => b.CreatedAt).FirstOrDefault();
                    }

                    if (returnBatch != null)
                    {
                        allocatedBatchUuid = returnBatch.Uuid;
                        costAtSale         = returnBatch.CostPrice;

                        returnBatch.RemainingQuantity += itemDto.Quantity;
                        if (returnBatch.Status == BatchStatus.Depleted)
                        {
                            returnBatch.Status = BatchStatus.Active;
                        }

                        if (!batchesToUpdate.Any(b => b.Id == returnBatch.Id))
                        {
                            batchesToUpdate.Add(returnBatch);
                        }

                        // Create SaleReturn stock movement
                        stockMovementsToAdd.Add(new StockMovement
                        {
                            Uuid          = Guid.NewGuid().ToString(),
                            BatchUuid     = returnBatch.Uuid,
                            ItemUuid      = item.Uuid,
                            MovementType  = StockMovementType.SALE_RETURN,
                            Quantity      = itemDto.Quantity,
                            Direction     = StockMovementDirection.IN,
                            CostPrice     = returnBatch.CostPrice,
                            ReferenceType = StockMovementReferenceType.ORDER_RETURN,
                            ReferenceUuid = itemDto.ReturnedOrderItemUuid,
                            Reason        = "Customer return item",
                            CreatedAt     = DateTime.UtcNow,
                            CreatedBy     = currentUser.Uuid
                        });
                    }
                }

                var orderItem = new OrderItem
                {
                    Uuid                    = Guid.NewGuid().ToString(),
                    OriginalItemUuid        = item.Uuid,
                    BatchUuid               = allocatedBatchUuid,
                    AllowsDecimalQuantities = item.AllowsDecimalQuantities,
                    PrintName               = itemDto.PrintName,
                    Quantity                = itemDto.Quantity,
                    PriceAtSale             = salePrice,
                    MarkedPriceAtSale       = markedPrice,
                    CostAtSale              = costAtSale,
                    LineTotal               = lineTotal,
                    IsReturnItem            = itemDto.IsReturnItem,
                    Description             = itemDto.Description,
                    ReturnedOrderItemUuid   = itemDto.ReturnedOrderItemUuid
                };

                orderItems.Add(orderItem);

                // Only accumulate cost for profit calculation; trust frontend for gross/discount/net/itemCount
                totalCost += itemDto.Quantity * costAtSale;
            }

            // Use frontend-provided (required) order-level totals and item count (sum of quantities)
            grossAmount   = orderDto.GrossAmount;
            totalDiscount = orderDto.TotalDiscount;
            var netAmount = orderDto.NetAmount;
            itemCount     = orderDto.ItemCount > 0 
                ? orderDto.ItemCount 
                : (int)Math.Round(orderItems.Where(item => !item.IsReturnItem && item.Quantity > 0).Sum(item => item.Quantity));
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
                Uuid          = Guid.NewGuid().ToString(),
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
                OrderItems    = orderItems,
                CreatedBy     = currentUser.Uuid
            };

            LoanSettlementLog? initialLog = null;

            // If this order is a loan (credit), create initial loan settlement log entry representing the created loan (negative balance)
            if (order.MainStatus == MainOrderStatus.Loan)
            {
                initialLog = new LoanSettlementLog
                {
                    Uuid             = Guid.NewGuid().ToString(),
                    PaymentDate      = DateTime.Now,
                    Description      = "Loan created",
                    AmountPaid       = order.AmountPaid,
                    RemainingBalance = Math.Abs(order.Balance),
                    Status           = LoanSettlementStatus.Created
                };
            }

            Customer? customerToUpdate = null;
            // Update customer loyalty points based on the request DTO (earn/deduct per spec)
            if (order.CustomerId.HasValue)
            {
                var customer = await _customerRepository.GetEntityByIdAsync(order.CustomerId.Value);
                if (customer != null)
                {
                    var suppressEarnForLoan = order.MainStatus == MainOrderStatus.Loan && !calculateLoyaltyForLoanOrders;
                    var points              = CalculateLoyaltyPointsFromReq(orderDto.OrderItems, suppressEarnForLoan);
                    customer.LoyaltyPoints  = Math.Max(0, customer.LoyaltyPoints + points);

                    customerToUpdate        = customer;
                }
            }

            var createdOrder = await _orderRepository.SaveCreateOrderAsync(order, initialLog, customerToUpdate, batchesToUpdate, stockMovementsToAdd);

            // Clear items cache to reflect inventory changes
            _cache.Remove(ServiceCacheKey.Items);

            return _mapper.Map<OrderResDto>(createdOrder);
        }

        /// <summary>
        /// Retrieves an order by its primary key id.
        /// </summary>
        /// <param name="id">Order id.</param>
        /// <param name="currentUser">Current user context.</param>
        /// <returns>Order DTO when found; otherwise null.</returns>

        public async Task<OrderResDto?> GetOrderAsync(int id, CurrentUser currentUser)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order == null ? null : _mapper.Map<OrderResDto>(order);
        }

        /// <summary>
        /// Retrieves an order by its order number.
        /// </summary>
        /// <param name="orderNumber">Order number string.</param>
        /// <param name="currentUser">Current user context.</param>
        /// <returns>Order DTO when found; otherwise null.</returns>
        public async Task<OrderResDto?> GetOrderByOrderNumberAsync(string orderNumber, CurrentUser currentUser)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            return order == null ? null : _mapper.Map<OrderResDto>(order);
        }

        /// <summary>
        /// Returns order header with order items enriched with returned quantities from the returned-items summary view.
        /// </summary>
        /// <param name="orderNumber">Order number to fetch.</param>
        /// <param name="currentUser">Current user context.</param>
        /// <returns>
        /// An <see cref="OrderResDto"/> when found, with each <c>OrderItems.ReturnSummary</c> populated when applicable; otherwise null.
        /// Returns null when no order exists for the specified order number.
        /// </returns>
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

        /// <summary>
        /// Retrieves an order by its unique UUID identifier.
        /// </summary>
        /// <param name="uuid">The UUID of the order to retrieve.</param>
        /// <param name="currentUser">The current user requesting the order.</param>
        /// <returns>The order details if found, otherwise null.</returns>
        public async Task<OrderResDto?> GetOrderByUuidAsync(string uuid, CurrentUser currentUser)
        {
            var order    = await _orderRepository.GetByUuidAsync(uuid);
            return order == null ? null : _mapper.Map<OrderResDto>(order);
        }

        /// <summary>
        /// Retrieves a single order by its UUID identifier.
        /// </summary>
        /// <param name="uuid">Order UUID.</param>
        /// <param name="currentUser">Current user context.</param>
        /// <returns>Order DTO when found; otherwise null.</returns>

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

        /// <summary>
        /// Updates an existing order with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the order to update.</param>
        /// <param name="orderDto">The order data transfer object containing updated information.</param>
        /// <param name="currentUser">The current user updating the order.</param>
        /// <returns>The updated order details.</returns>
        public async Task<OrderResDto> UpdateOrderAsync(int id, OrderReqDto orderDto, CurrentUser currentUser)
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

            // Clear old items
            existingOrder.OrderItems.Clear();

            decimal grossAmount   = 0;
            decimal totalDiscount = 0;
            decimal totalCost     = 0;
            int itemCount         = 0;

            foreach (var itemDto in orderDto.OrderItems)
            {
                var item = await _itemRepository.GetByUuidAsync(itemDto.ItemUuid);
                if (item == null)
                    throw new ArgumentException($"Item with UUID {itemDto.ItemUuid} not found");

                var availableStock = (await _batchRepository.GetActiveBatchesByItemUuidAsync(item.Uuid, false)).Sum(b => b.RemainingQuantity);

                if (!item.AllowsDecimalQuantities && itemDto.Quantity % 1 != 0)
                    throw new ArgumentException($"Item {item.PrintName} does not allow decimal quantities");

                // Check stock availability (respect AllowZeroStock setting)
                if (!itemDto.IsReturnItem && !allowZeroStock && availableStock < itemDto.Quantity)
                    throw new ArgumentException($"Insufficient stock for item {item.PrintName}. Available: {availableStock}, Requested: {itemDto.Quantity}");

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
                    CostAtSale              = 0,
                    LineTotal               = lineTotal,
                    IsReturnItem            = itemDto.IsReturnItem,
                    Description             = itemDto.Description
                };

                existingOrder.OrderItems.Add(orderItem);
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

            if (existingOrder.Balance < 0 && !allowOrdersForLoan)
                throw new InvalidOperationException("Credit (Loan) orders not allowed.");

            if (existingOrder.Balance < 0 && allowOrdersForLoan && !AllowCreditOrderWithoutCustomer && !orderDto.CustomerId.HasValue)
                throw new InvalidOperationException("Loan orders require a customer.");

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

            Customer? customerToUpdate = null;
            if (existingOrder.CustomerId.HasValue)
            {
                var customer = await _customerRepository.GetEntityByIdAsync(existingOrder.CustomerId.Value);
                if (customer != null)
                {
                    var suppressEarnForLoan = existingOrder.MainStatus == MainOrderStatus.Loan && !calculateLoyaltyForLoanOrders;
                    var oldPoints           = CalculateLoyaltyPointsFromOrderItems(oldOrderItemsSnapshot, false);
                    var newPoints           = CalculateLoyaltyPointsFromOrderItems(existingOrder.OrderItems, suppressEarnForLoan);
                    var delta               = newPoints - oldPoints;
                    customer.LoyaltyPoints  = Math.Max(0, customer.LoyaltyPoints + delta);

                    customerToUpdate = customer;
                }
            }

            var updatedOrder = await _orderRepository.SaveUpdateOrderAsync(existingOrder, customerToUpdate);

            return _mapper.Map<OrderResDto>(updatedOrder);
        }

        /// <summary>
        /// Deletes an order with the specified identifier.
        /// Performs soft-delete by default, reversing all stock, stock movements, and customer loyalty changes. Use isPermanent=true to remove permanently.
        /// </summary>
        /// <param name="id">The unique identifier of the order to delete.</param>
        /// <param name="currentUser">The current user deleting the order.</param>
        /// <param name="isPermanent">permanent delete - Default false.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        public async Task<bool> DeleteOrderAsync(int id, CurrentUser currentUser, bool isPermanent = false)
        {
            var isActiveOnly = !isPermanent;
            var order = await _orderRepository.GetByIdAsync(id, isActiveOnly);
            if (order == null) return false;

            var batchesToUpdate = new List<InventoryBatch>();
            var stockMovementsToAdd = new List<StockMovement>();

            // 1. Reverse Inventory Batch stock and create audit stock movement records
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                foreach (var orderItem in order.OrderItems)
                {
                    InventoryBatch? batch = null;

                    // Locate the batch allocated at checkout
                    if (!string.IsNullOrWhiteSpace(orderItem.BatchUuid))
                    {
                        batch = batchesToUpdate.FirstOrDefault(b => b.Uuid == orderItem.BatchUuid)
                             ?? await _batchRepository.GetByUuidAsync(orderItem.BatchUuid);
                    }

                    // Fallback to active/latest batch for the item if original batch record not found
                    if (batch == null && !string.IsNullOrWhiteSpace(orderItem.OriginalItemUuid))
                    {
                        var activeBatches = await _batchRepository.GetActiveBatchesByItemUuidAsync(orderItem.OriginalItemUuid, includeExpired: true);
                        batch = activeBatches.OrderByDescending(b => b.CreatedAt).FirstOrDefault();
                    }

                    if (batch != null)
                    {
                        if (!orderItem.IsReturnItem)
                        {
                            // Standard sale item: stock was deducted -> add it back to inventory
                            batch.RemainingQuantity += orderItem.Quantity;
                            if (batch.Status == BatchStatus.Depleted && batch.RemainingQuantity > 0)
                            {
                                batch.Status = BatchStatus.Active;
                            }

                            // Record Inbound stock movement
                            stockMovementsToAdd.Add(new StockMovement
                            {
                                Uuid          = Guid.NewGuid().ToString(),
                                BatchUuid     = batch.Uuid,
                                ItemUuid      = batch.ItemUuid,
                                MovementType  = StockMovementType.MANUAL_ADJUST_IN,
                                Quantity      = orderItem.Quantity,
                                Direction     = StockMovementDirection.IN,
                                CostPrice     = orderItem.CostAtSale > 0 ? orderItem.CostAtSale : batch.CostPrice,
                                ReferenceType = StockMovementReferenceType.ORDER_DELETE,
                                ReferenceUuid = order.Uuid,
                                Reason        = $"Restored stock from deleted order {order.OrderNumber}",
                                CreatedAt     = DateTime.UtcNow,
                                CreatedBy     = currentUser?.Uuid
                            });
                        }
                        else
                        {
                            // Return item: stock was added back during customer return -> deduct it from inventory
                            batch.RemainingQuantity = Math.Max(0m, batch.RemainingQuantity - orderItem.Quantity);
                            if (batch.RemainingQuantity <= 0)
                            {
                                batch.Status = BatchStatus.Depleted;
                            }

                            // Record Outbound stock movement
                            stockMovementsToAdd.Add(new StockMovement
                            {
                                Uuid          = Guid.NewGuid().ToString(),
                                BatchUuid     = batch.Uuid,
                                ItemUuid      = batch.ItemUuid,
                                MovementType  = StockMovementType.MANUAL_ADJUST_OUT,
                                Quantity      = orderItem.Quantity,
                                Direction     = StockMovementDirection.OUT,
                                CostPrice     = orderItem.CostAtSale > 0 ? orderItem.CostAtSale : batch.CostPrice,
                                ReferenceType = StockMovementReferenceType.ORDER_DELETE,
                                ReferenceUuid = order.Uuid,
                                Reason        = $"Reversed returned stock from deleted order {order.OrderNumber}",
                                CreatedAt     = DateTime.UtcNow,
                                CreatedBy     = currentUser?.Uuid
                            });
                        }

                        if (!batchesToUpdate.Any(b => b.Id == batch.Id))
                        {
                            batchesToUpdate.Add(batch);
                        }
                    }
                }
            }

            // 2. Reverse Customer Loyalty Points
            Customer? customerToUpdate = null;
            if (order.CustomerId.HasValue)
            {
                var customer = await _customerRepository.GetEntityByIdAsync(order.CustomerId.Value);
                if (customer != null)
                {
                    var calculateLoyaltyForLoanOrders = await _settingService.GetSettingValueAsync(SettingKey.CalculateLoyaltyPointsForCreditOrders, currentUser);
                    var suppressEarnForLoan = order.MainStatus == MainOrderStatus.Loan && !calculateLoyaltyForLoanOrders;
                    var pointsEarnedOrDeducted = CalculateLoyaltyPointsFromOrderItems(order.OrderItems, suppressEarnForLoan);

                    // Reverse: if points were earned (positive), deduct them; if deducted (negative), add back
                    customer.LoyaltyPoints = Math.Max(0, customer.LoyaltyPoints - pointsEarnedOrDeducted);
                    customerToUpdate = customer;
                }
            }

            // 3. Atomically persist changes
            await _orderRepository.SaveDeleteOrderAsync(order, isPermanent, customerToUpdate, batchesToUpdate, stockMovementsToAdd);

            // 4. Invalidate item cache so frontend reflects the restored stock
            _cache.Remove(ServiceCacheKey.Items);

            return true;
        }

        /// <summary>
        /// Updates the status of an existing order.
        /// </summary>
        /// <param name="id">The unique identifier of the order to update.</param>
        /// <param name="status">The new status to set for the order.</param>
        /// <param name="currentUser">The current user updating the order status.</param>
        /// <returns>The updated order details.</returns>
        public async Task<OrderResDto> UpdateOrderStatusAsync(int id, MainOrderStatus status, CurrentUser currentUser)
        {
            var allowZeroStockSetting = await _settingService.GetByKeyAsync(SettingKey.AllowZeroStock, currentUser);
            var allowZeroStock = allowZeroStockSetting?.SettingValue ?? false;

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new ArgumentException($"Order with ID {id} not found");

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

            var updatedOrder = await _orderRepository.SaveUpdateOrderStatusAsync(order);

            return _mapper.Map<OrderResDto>(updatedOrder);
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
            /// <summary>
            /// Retrieves returned-items summary rows from the returned-items view for a given order number.
            /// </summary>
            /// <param name="orderNumber">Order number to query returned items for.</param>
            /// <param name="currentUser">Current user context.</param>
            var rows = await _orderRepository.GetReturnedItemsSummaryByOrderNumberAsync(orderNumber);
            return _mapper.Map<List<pos_service.Models.DTO.ReturnedItems.ReturnedItemsSummaryResDto>>(rows);
        }

        public async Task<List<OrderResDto>> GetInactiveOrdersAsync(CurrentUser currentUser)
        {
            var orders = await _orderRepository.GetInactiveOrdersAsync();
            return _mapper.Map<List<OrderResDto>>(orders);
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
