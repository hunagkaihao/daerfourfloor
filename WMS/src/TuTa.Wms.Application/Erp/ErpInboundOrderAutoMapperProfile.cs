using AutoMapper;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Dto;
using TuTa.Wms.Erp.Entities;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP入库单AutoMapper配置文件
    /// </summary>
    public class ErpInboundOrderAutoMapperProfile : Profile
    {
        public ErpInboundOrderAutoMapperProfile()
        {
            // ERP入库单映射
            CreateMap<ErpInboundOrder, ErpInboundOrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

            CreateMap<ErpInboundOrderDto, ErpInboundOrder>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (InboundOrderStatus)src.Status));

            // ERP入库单项映射
            CreateMap<ErpInboundItem, ErpInboundItemDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

            CreateMap<ErpInboundItemDto, ErpInboundItem>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (InboundItemStatus)src.Status));
        }
    }
}
