using AutoMapper;
using Library.ControllerApi.DTOs;
using Library.Data.Entities;

namespace Library.ControllerApi.Mapping;

//This class inherits from AutoMappers profile . I'm going to choose to just use one profile
//in my app. It's entire purpose is holding the configuration to map DTOs to Model/Entities
public class MappingProfile : Profile
{
    //We just use the constructor and set out mapping there
    public MappingProfile()
    {
        CreateMap<InventoryItem, InventoryDto>()
                .ForCtorParam("Sku", o => o.MapFrom(s => s.Product.Sku))
                .ForCtorParam("Name", o => o.MapFrom(s => s.Product.Name));

                //CreateMap<Inventoryit
    }
}