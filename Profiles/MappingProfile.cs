using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO;

namespace pos_service.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Item Mappings
            CreateMap<Item, ItemResDto>();
            CreateMap<ItemReqDto, Item>();
            CreateMap<Item, ItemSupplierResDto>(); 

            // Contact Mappings
            CreateMap<Contact, ContactResDto>();
            CreateMap<ContactReqDto, Contact>();

            // Supplier Mappings
            CreateMap<Supplier, SupplierResDto>();
            CreateMap<SupplierReqDto, Supplier>();
        }
    }
}
