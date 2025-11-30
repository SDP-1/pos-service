using AutoMapper;
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

        public OrderService(IOrderRepository orderRepository, IItemRepository itemRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _itemRepository  = itemRepository;
            _mapper          = mapper;
        }

        public async Task<OrderResDto> CreateOrderAsync(OrderReqDto orderDto, CurrentUser currentUser)
        {
            // Validate items and get current item data
            var orderItems = new List<OrderItem>();
            decimal grossAmount = 0;
            decimal totalDiscount = 0;
            decimal totalCost = 0;
            int itemCount = 0;

            foreach (var itemDto in orderDto.OrderItems)
            {
                var item = await _itemRepository.GetByUuidAsync(itemDto.ItemUuid);
                if (item == null)
                    throw new ArgumentException($"Item with UUID {itemDto.ItemUuid} not found");

                if (!item.AllowsDecimalQuantities && itemDto.Quantity % 1 != 0)
                    throw new ArgumentException($"Item {item.PrintName} does not allow decimal quantities");

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
                    OriginalItemUuid = item.Uuid,
                    AllowsDecimalQuantities = item.AllowsDecimalQuantities,
                    ItemPrintName = item.PrintName,
                    Quantity = itemDto.Quantity,
                    PriceAtSale = basePrice,
                    DiscountRatioAtSale = discountRatio,
                    CostAtSale = item.BuyingPrice,
                    LineTotal = lineTotal,
                };

                orderItems.Add(orderItem);

                grossAmount += itemDto.Quantity * basePrice;
                totalDiscount += itemDto.Quantity * basePrice * (discountRatio / 100);
                totalCost += itemDto.Quantity * item.BuyingPrice;
                itemCount++;
            }

            var netAmount = grossAmount - totalDiscount;
            var balance = orderDto.AmountPaid - netAmount;

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

            var createdOrder = await _orderRepository.CreateAsync(order);
            return _mapper.Map<OrderResDto>(createdOrder);
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
            var existingOrder = await _orderRepository.GetByIdAsync(id);
            if (existingOrder == null)
                throw new ArgumentException($"Order with ID {id} not found");

            if (existingOrder.Status != OrderStatus.Pending)
                throw new InvalidOperationException("Only pending orders can be modified");

            // For simplicity, we'll recreate the order items
            // In a real scenario, you might want to handle updates more granularly
            existingOrder.OrderItems.Clear();

            // Recalculate order totals (similar to CreateOrderAsync)
            decimal grossAmount = 0;
            decimal totalDiscount = 0;
            decimal totalCost = 0;
            int itemCount = 0;

            foreach (var itemDto in orderDto.OrderItems)
            {
                if (itemDto.IsDeleted == true) continue;

                var item = await _itemRepository.GetByUuidAsync(itemDto.ItemUuid);
                if (item == null)
                    throw new ArgumentException($"Item with UUID {itemDto.ItemUuid} not found");

                var basePrice = orderDto.SaleType == SaleType.Wholesale ? item.WholesalePrice : item.RetailPrice;
                var discountRatio = orderDto.SaleType == SaleType.Wholesale ?
                    item.WholesaleDiscountRatio : item.RetailDiscountRatio;

                if (itemDto.DiscountRatio > 0)
                    discountRatio = itemDto.DiscountRatio;

                var priceAfterDiscount = basePrice * (1 - discountRatio / 100);
                var lineTotal = itemDto.Quantity * priceAfterDiscount;

                var orderItem = new OrderItem
                {
                    OriginalItemUuid        = item.Uuid,
                    AllowsDecimalQuantities = item.AllowsDecimalQuantities,
                    ItemPrintName           = item.PrintName,
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

            // Update order totals
            existingOrder.PaymentMethod = orderDto.PaymentMethod;
            existingOrder.SaleType      = orderDto.SaleType;
            existingOrder.ItemCount     = itemCount;
            existingOrder.GrossAmount   = grossAmount;
            existingOrder.TotalDiscount = totalDiscount;
            existingOrder.NetAmount     = grossAmount - totalDiscount;
            existingOrder.TotalCost     = totalCost;
            existingOrder.AmountPaid    = orderDto.AmountPaid;
            existingOrder.Balance       = orderDto.AmountPaid - (grossAmount - totalDiscount);
            existingOrder.CustomerId    = orderDto.CustomerId;

            var updatedOrder = await _orderRepository.UpdateAsync(existingOrder);
            return _mapper.Map<OrderResDto>(updatedOrder);
        }

        public async Task<bool> DeleteOrderAsync(int id, CurrentUser currentUser)
        {
            return await _orderRepository.DeleteAsync(id);
        }

        public async Task<OrderResDto> UpdateOrderStatusAsync(int id, OrderStatus status, CurrentUser currentUser)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new ArgumentException($"Order with ID {id} not found");

            order.Status = status;
            //order.UpdatedAt = DateTime.UtcNow;
            //order.UpdatedBy = currentUser.Uuid;

            var updatedOrder = await _orderRepository.UpdateAsync(order);
            return _mapper.Map<OrderResDto>(updatedOrder);
        }
    }
}
