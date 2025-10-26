using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Contact;
using pos_service.Models.DTO.Item;
using pos_service.Models.DTO.Supplier;
using pos_service.Models.DTO.User;
using pos_service.Repositories;

namespace pos_service.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Item Mappings
            CreateMap<Item, ItemResDto>();
            CreateMap<Item, BaseitemResDto>();
            CreateMap<ItemReqDto, Item>();

            // Contact Mappings
            CreateMap<Contact, ContactResDto>();
            CreateMap<ContactReqDto, Contact>();

            // Supplier Mappings
            CreateMap<Supplier, SupplierResDto>();
            CreateMap<SupplierReqDto, Supplier>();

            // User Mappings
            CreateMap<User, UserResDto>();
            CreateMap<UserReqDto, User>();
            CreateMap<UserLoginReqDto, User>();
        }
    }
}
