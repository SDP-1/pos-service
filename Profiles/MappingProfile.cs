using AutoMapper;
using pos_service.Models;
using System.Linq;
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
            CreateMap<Item, ItemResDto>()
                .ForMember(dest => dest.Suppliers, opt => opt.MapFrom(src => src.ItemSuppliers.Select(isu => isu.Supplier)));
            CreateMap<Item, ItemMiniResDto>();
            CreateMap<ItemReqDto, Item>();

            // Contact Mappings
            CreateMap<Contact, ContactResDto>();
            CreateMap<ContactReqDto, Contact>();

            // Supplier Mappings
            CreateMap<Supplier, SupplierResDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.ItemSuppliers.Select(isu => isu.Item)));
            CreateMap<SupplierReqDto, Supplier>();

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
            CreateMap<OrderItemReqDto, OrderItem>();

            // Role Mapper
            CreateMap<Role, RoleResDto>();
            CreateMap<RoleReqDto, Role>();
        }
    }
}
