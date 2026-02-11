using AutoMapper;
using Client.Models;

namespace Client.Utils
{
    public class AutoMapperClientProfile : Profile
    {
        public AutoMapperClientProfile()
        {
            CreateMap<Manager.Models.Shipment, Shipment>().ReverseMap();
            CreateMap<ShipmentPut, Manager.Models.Shipment>();
            CreateMap<ShipmentPost, Manager.Models.Shipment>();
            CreateMap<ShipmentPost, Shipment>();
            CreateMap<ShipmentPut, Shipment>();
        }
    }
}
