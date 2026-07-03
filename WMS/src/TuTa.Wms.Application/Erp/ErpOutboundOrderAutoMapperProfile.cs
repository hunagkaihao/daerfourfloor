using AutoMapper;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Dto;
using TuTa.Wms.Erp.Entities;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP出库单AutoMapper配置
    /// </summary>
    public class ErpOutboundOrderAutoMapperProfile : Profile
    {
        public ErpOutboundOrderAutoMapperProfile()
        {
            // 出库单映射
            CreateMap<ErpOutboundOrder, ErpOutboundOrderDto>()
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => GetOutboundOrderStatusName(src.Status)));

            // 出库单项映射
            CreateMap<ErpOutboundItem, ErpOutboundItemDto>()
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => GetOutboundItemStatusName(src.Status)));
        }

        /// <summary>
        /// 获取出库单状态名称
        /// </summary>
        private static string GetOutboundOrderStatusName(OutboundOrderStatus status)
        {
            return status switch
            {
                OutboundOrderStatus.Created => "已创建",
                OutboundOrderStatus.Outbounding => "出库中",
                OutboundOrderStatus.Completed => "已完成",
                OutboundOrderStatus.Cancelled => "已取消",
                _ => "未知状态"
            };
        }

        /// <summary>
        /// 获取出库项状态名称
        /// </summary>
        private static string GetOutboundItemStatusName(OutboundItemStatus status)
        {
            return status switch
            {
                OutboundItemStatus.Created => "已创建",
                OutboundItemStatus.Outbounding => "出库中",
                OutboundItemStatus.Completed => "已完成",
                OutboundItemStatus.Cancelled => "已取消",
                _ => "未知状态"
            };
        }
    }
}
