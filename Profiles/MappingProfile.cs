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
            CreateMap<ItemPrice, ItemPriceDto>();
            CreateMap<ItemPriceDto, ItemPrice>();
            CreateMap<ItemExpiry, ItemExpiryDto>();
            CreateMap<InventoryUnit, InventoryUnitDto>();
            CreateMap<InventoryUnitDto, InventoryUnit>();
            CreateMap<Inventory, InventoryResDto>()
                .ForMember(dest => dest.Units, opt => opt.MapFrom(src => src.Units.OrderBy(u => u.QuantityInBaseUnits).ToList()))
                // Use a resolution function to safely map Item.ExpDates -> InventoryResDto.Expiries
                .ForMember(dest => dest.Expiries, opt => opt.MapFrom((src, dest, destMember, ctx) =>
                    src.Item != null
                        ? ctx.Mapper.Map<IEnumerable<ItemExpiryDto>>(src.Item.ExpDates.OrderBy(e => e.ExpDate))
                        : new List<ItemExpiryDto>()))
                // Map item price into inventory response so callers retrieving inventory can also see price
                .ForMember(dest => dest.Price, opt => opt.MapFrom((src, dest, destMember, ctx) =>
                    src.Item != null && src.Item.Price != null
                        ? ctx.Mapper.Map<ItemPriceDto>(src.Item.Price)
                        : new ItemPriceDto()));

            CreateMap<Item, ItemResDto>()
                .ForMember(dest => dest.Suppliers, opt => opt.MapFrom(src => src.ItemSuppliers.Select(isu => isu.Supplier)))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price ?? new ItemPrice()))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.StockQuantity : 0m))
                .ForMember(dest => dest.AllowsDecimalQuantities, opt => opt.MapFrom(src => src.Inventory != null && src.Inventory.AllowsDecimalQuantities))
                .ForMember(dest => dest.UnitType, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.UnitType : Models.Enums.UnitType.Each))
                .ForMember(dest => dest.Units, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.Units.OrderBy(u => u.QuantityInBaseUnits).ToList() : new List<InventoryUnit>()))
                .ForMember(dest => dest.ExpDates, opt => opt.MapFrom(src => src.ExpDates.OrderBy(e => e.ExpDate)));

            CreateMap<Item, ItemMiniResDto>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price ?? new ItemPrice()))
                .ForMember(dest => dest.ExpDates, opt => opt.MapFrom(src => src.ExpDates.OrderBy(e => e.ExpDate)))
                .ForMember(dest => dest.AllowsDecimalQuantities, opt => opt.MapFrom(src => src.Inventory != null && src.Inventory.AllowsDecimalQuantities))
                .ForMember(dest => dest.UnitType, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.UnitType : Models.Enums.UnitType.Each));

            // Map from ItemResDto -> ItemMiniResDto so services can map projected DTOs
            CreateMap<ItemResDto, ItemMiniResDto>();

            // Map ItemReqDto -> Item but do not overwrite primary key properties when mapping
            // into an existing entity. Keys are assigned by service logic when creating items.
            CreateMap<ItemReqDto, Item>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.SubId, opt => opt.Ignore())
                .ForMember(d => d.Price, opt => opt.Ignore())
                .ForMember(d => d.ExpDates, opt => opt.Ignore())
                .ForMember(d => d.Inventory, opt => opt.Ignore());

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

            // Order Maper
            CreateMap<Order, OrderResDto>()
                .ForMember(dest => dest.MainStatus, opt => opt.MapFrom(src => src.MainStatus))
                .ForMember(dest => dest.SubStatus, opt => opt.MapFrom(src => src.SubStatus))
                .ForMember(dest => dest.CashierName, opt => opt.MapFrom(src => src.Cashier != null ? src.Cashier.FullName : string.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName  : string.Empty))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.PhoneNumber : string.Empty));
            CreateMap<LoanSettlementLog, LoanSettlementLogResDto>();
            // include mapping from Order -> LoanSettlementLogs will be handled by automapper automatically if property exists
            CreateMap<OrderReqDto, Order>();
            CreateMap<Order, OrderSummaryResDto>()
                .ForMember(dest => dest.MainStatus, opt => opt.MapFrom(src => src.MainStatus))
                .ForMember(dest => dest.SubStatus, opt => opt.MapFrom(src => src.SubStatus));

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
    }
}
