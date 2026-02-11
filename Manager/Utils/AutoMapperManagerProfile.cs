using AutoMapper;
using Manager.Models;

namespace Manager.Utils
{
    public class AutoMapperManagerProfile : Profile
    {
        public AutoMapperManagerProfile()
        {
            CreateMap<Resources.Models.Shipment, Shipment>().ReverseMap();
        }
    }
}
