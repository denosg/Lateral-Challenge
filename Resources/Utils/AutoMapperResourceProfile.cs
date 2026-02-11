
using AutoMapper;
using Resources.Models;

namespace Resources.Utils
{
    public class AutoMapperResourceProfile : Profile
    {
        public AutoMapperResourceProfile()
        {
            CreateMap<Database.Models.Shipment, Shipment>().ReverseMap();
        }
    }
}
