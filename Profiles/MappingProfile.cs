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

            CreateMap<Item, ItemResDto>()
                .ForMember(dest => dest.Suppliers, opt => opt.MapFrom(src => src.ItemSuppliers.Select(isu => isu.Supplier)))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price ?? new ItemPrice()))
                .ForMember(dest => dest.ExpDates, opt => opt.MapFrom(src => src.ExpDates.OrderBy(e => e.ExpDate)));

            CreateMap<Item, ItemMiniResDto>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price ?? new ItemPrice()))
                .ForMember(dest => dest.ExpDates, opt => opt.MapFrom(src => src.ExpDates.OrderBy(e => e.ExpDate)));

            // Map ItemReqDto -> Item but do not overwrite primary key properties when mapping
            // into an existing entity. Keys are assigned by service logic when creating items.
            CreateMap<ItemReqDto, Item>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.SubId, opt => opt.Ignore())
                .ForMember(d => d.Price, opt => opt.Ignore())
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
            CreateMap<User, UserResDto>();
            CreateMap<User, CurrentUser>();
            CreateMap<UserReqDto, User>();
            CreateMap<UserLoginReqDto, User>();

            // Order Maper
            CreateMap<Order, OrderResDto>();
            CreateMap<OrderReqDto, Order>();
            CreateMap<Order, OrderSummaryResDto>();

            // Order Maper
            CreateMap<OrderItem, OrderItemResDto>();
            CreateMap<OrderItem, OrderItemMiniResDto>();
            CreateMap<OrderItemReqDto, OrderItem>()
                .ForMember(dest => dest.MarkedPriceAtSale, opt => opt.MapFrom(src => src.MarkedPrice))
                .ForMember(dest => dest.PriceAtSale, opt => opt.MapFrom(src => src.SalePrice))
                .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.LineTotal));

            // Role Mapper
            CreateMap<Role, RoleResDto>();
            CreateMap<RoleReqDto, Role>();
        }
    }
}
