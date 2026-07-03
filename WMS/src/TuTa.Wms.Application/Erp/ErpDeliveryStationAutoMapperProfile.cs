using AutoMapper;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Dto;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP收料工位AutoMapper配置
    /// </summary>
    public class ErpDeliveryStationAutoMapperProfile : Profile
    {
        public ErpDeliveryStationAutoMapperProfile()
        {
            // 收料工位映射
            CreateMap<ErpDeliveryStation, ErpDeliveryStationDto>();
        }
    }
}
