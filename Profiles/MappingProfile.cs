using AutoMapper;
using System;
using System.Linq;
using pos_service.Models;
using pos_service.Models.DTO.Contacts;
using pos_service.Models.DTO.Items;
using pos_service.Models.DTO.Orders;
using pos_service.Models.DTO.OrderItems;
using pos_service.Models.DTO.Roles;
using pos_service.Models.DTO.Suppliers;
using pos_service.Models.DTO.Users;
using pos_service.Models.DTO.Settings;
using pos_service.Models.DTO.ReturnedItems;
using pos_service.Models.DTO.Customers;
using pos_service.Models.DTO.Inventory;

namespace pos_service.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Item Mappings
            // map Item -> ItemResDto.Suppliers from ItemSuppliers join
            CreateMap<ItemExpiry, ItemExpiryResDto>();
            CreateMap<ItemUnit, InventoryUnitResDto>();
            CreateMap<InventoryUnitReqDto, ItemUnit>();

            // Inventory Batch Mappings
            CreateMap<InventoryBatch, pos_service.Models.DTO.InventoryBatches.InventoryBatchResDto>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.Name : null))
                .ForMember(dest => dest.ItemPrintName, opt => opt.MapFrom(src => src.Item != null ? src.Item.PrintName : null))
                .ForMember(dest => dest.ItemBarcode, opt => opt.MapFrom(src => src.Item != null ? src.Item.BarCode : null))
                .ForMember(dest => dest.ItemNumber, opt => opt.MapFrom(src => src.Item != null ? $"{src.Item.Id}-{src.Item.SubId}" : null))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src =>
                    src.Supplier != null ? src.Supplier.Name :
                    (src.Item != null && src.Item.ItemSuppliers.Any() ? src.Item.ItemSuppliers.First().Supplier!.Name : null)));

            CreateMap<InventoryBatch, pos_service.Models.DTO.InventoryBatches.InventoryBatchSelectDto>();

            CreateMap<pos_service.Models.DTO.InventoryBatches.InventoryBatchReqDto, InventoryBatch>();

            // Stock Movement Mappings
            CreateMap<StockMovement, pos_service.Models.DTO.StockMovements.StockMovementResDto>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.PrintName : null))
                .ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.BatchNumber : null))
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByUser != null ? (!string.IsNullOrEmpty(src.CreatedByUser.FullName) ? src.CreatedByUser.FullName : src.CreatedByUser.UserName) : src.CreatedBy));

            // Inventory Batch Log Mappings
            CreateMap<InventoryBatchLog, pos_service.Models.DTO.InventoryBatches.InventoryBatchLogResDto>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.PrintName : null))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Batch != null && src.Batch.Supplier != null ? src.Batch.Supplier.Name : null))
                .ForMember(dest => dest.ActionByName, opt => opt.MapFrom(src => src.ActionByUser != null ? (!string.IsNullOrEmpty(src.ActionByUser.FullName) ? src.ActionByUser.FullName : src.ActionByUser.UserName) : src.ActionBy));

            // Purchase Mappings
            CreateMap<Purchase, pos_service.Models.DTO.Purchases.PurchaseResDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
                .ForMember(dest => dest.Batches, opt => opt.MapFrom(src => src.Batches));
            CreateMap<pos_service.Models.DTO.Purchases.PurchaseReqDto, Purchase>();

            CreateMap<Item, ItemResDto>()
                .ForMember(dest => dest.Suppliers, opt => opt.MapFrom(src => src.ItemSuppliers.Select(isu => isu.Supplier)))
                .ForMember(dest => dest.ExpDates, opt => opt.MapFrom(src => src.ExpDates.OrderBy(e => e.ExpDate)));

            CreateMap<Item, ItemMiniResDto>()
                .ForMember(dest => dest.ExpDates, opt => opt.MapFrom(src => src.ExpDates.OrderBy(e => e.ExpDate)))
                .ForMember(dest => dest.AllowsDecimalQuantities, opt => opt.MapFrom(src => src.AllowsDecimalQuantities))
                .ForMember(dest => dest.UnitType, opt => opt.MapFrom(src => src.Units.Where(u => u.IsBaseUnit).Select(u => u.UnitType).FirstOrDefault() != Models.Enums.UnitType.None
                    ? src.Units.Where(u => u.IsBaseUnit).Select(u => u.UnitType).FirstOrDefault()
                    : (src.Units.OrderBy(u => u.QuantityInBaseUnits).Select(u => u.UnitType).FirstOrDefault() != Models.Enums.UnitType.None
                        ? src.Units.OrderBy(u => u.QuantityInBaseUnits).Select(u => u.UnitType).FirstOrDefault()
                        : Models.Enums.UnitType.Each)));

            // Map from ItemResDto -> ItemMiniResDto so services can map projected DTOs
            CreateMap<ItemResDto, ItemMiniResDto>()
                .ForMember(dest => dest.AllowsDecimalQuantities, opt => opt.MapFrom(src => src.Inventory != null && src.Inventory.AllowsDecimalQuantities))
                .ForMember(dest => dest.UnitType, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.UnitType : Models.Enums.UnitType.Each));

            // Map ItemReqDto -> Item but do not overwrite primary key properties when mapping
            // into an existing entity. Keys are assigned by service logic when creating items.
            CreateMap<ItemReqDto, Item>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.SubId, opt => opt.Ignore())
                .ForMember(d => d.ExpDates, opt => opt.Ignore());

            // Contact Mappings
            CreateMap<Contact, ContactResDto>();
            CreateMap<ContactReqDto, Contact>();

            // Supplier Mappings
            CreateMap<Supplier, SupplierResDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.ItemSuppliers.Select(isu => isu.Item)));
            // When mapping request DTO -> entity, ignore navigation collections because
            // service code will manage Contacts and ItemSuppliers explicitly.
            CreateMap<SupplierReqDto, Supplier>()
                .ForMember(dest => dest.Contacts, opt => opt.Ignore())
                .ForMember(dest => dest.ItemSuppliers, opt => opt.Ignore());

            // User Mappings
            // Map User -> UserResDto including binary ProfileImage similar to ShopResDto.Logo
            CreateMap<User, UserResDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.ProfileImage, opt => opt.MapFrom(src => src.ProfileImage));
            CreateMap<User, CurrentUser>();
            CreateMap<UserReqDto, User>();
            CreateMap<UserLoginReqDto, User>();

            // Order Mapper
            CreateMap<Order, OrderResDto>()
                .ForMember(dest => dest.MainStatus, opt => opt.MapFrom(src => src.MainStatus))
                .ForMember(dest => dest.SubStatus, opt => opt.MapFrom(src => src.SubStatus))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src =>
                    src.OrderItems != null && src.OrderItems.Any()
                        ? (int)Math.Round(src.OrderItems.Where(oi => !oi.IsReturnItem && oi.Quantity > 0).Sum(oi => oi.Quantity))
                        : src.ItemCount))
                .ForMember(dest => dest.CashierName, opt => opt.MapFrom(src => src.Cashier != null ? src.Cashier.FullName : string.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName  : string.Empty))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.PhoneNumber : string.Empty));
            CreateMap<LoanSettlementLog, LoanSettlementLogResDto>();
            // include mapping from Order -> LoanSettlementLogs will be handled by automapper automatically if property exists
            CreateMap<OrderReqDto, Order>();
            CreateMap<Order, OrderSummaryResDto>()
                .ForMember(dest => dest.MainStatus, opt => opt.MapFrom(src => src.MainStatus))
                .ForMember(dest => dest.SubStatus, opt => opt.MapFrom(src => src.SubStatus))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : string.Empty))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src =>
                    src.OrderItems != null && src.OrderItems.Any()
                        ? (int)Math.Round(src.OrderItems.Where(oi => !oi.IsReturnItem && oi.Quantity > 0).Sum(oi => oi.Quantity))
                        : src.ItemCount));

            // Order Maper
            CreateMap<OrderItem, OrderItemResDto>();
            CreateMap<OrderItem, OrderItemMiniResDto>();
            CreateMap<OrderItemReqDto, OrderItem>()
                .ForMember(dest => dest.MarkedPriceAtSale, opt => opt.MapFrom(src => src.MarkedPrice))
                .ForMember(dest => dest.PriceAtSale, opt => opt.MapFrom(src => src.SalePrice))
                .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.LineTotal));

            // Returned items summary view mapping
            CreateMap<ReturnedItemsSummary,ReturnedItemsSummaryResDto>();

            // Role Mapper
            CreateMap<Role, RoleResDto>();
            CreateMap<RoleReqDto, Role>();

            // Customer mappings
            CreateMap<Customer, CustomerResDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));
            CreateMap<CustomerReqDto, Customer>();

            // Setting mappings
            CreateMap<Setting, SettingResDto>();
            CreateMap<Shop, Models.DTO.Settings.ShopResDto>()
                .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => src.Logo));


        }

        /// <summary>Safely deserializes JSON to T, returning null on any error.</summary>
        private static T? SafeDeserialize<T>(string? json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            try { return System.Text.Json.JsonSerializer.Deserialize<T>(json); }
            catch { return null; }
        }

        /// <summary>Safely deserializes JSON to List&lt;T&gt;, returning an empty list on any error.</summary>
        private static List<T> SafeDeserializeList<T>(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new List<T>();
            try { return System.Text.Json.JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>(); }
            catch { return new List<T>(); }
        }
    }
}
