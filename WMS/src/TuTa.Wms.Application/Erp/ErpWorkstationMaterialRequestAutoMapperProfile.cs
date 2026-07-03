using AutoMapper;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Dto;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP工位叫料任务AutoMapper配置
    /// </summary>
    public class ErpWorkstationMaterialRequestAutoMapperProfile : Profile
    {
        public ErpWorkstationMaterialRequestAutoMapperProfile()
        {
            CreateMap<ErpWorkstationMaterialRequest, ErpWorkstationMaterialRequestDto>()
                .ForMember(dest => dest.StatusDescription, opt => opt.MapFrom(src => GetStatusDescription(src.Status)));

            CreateMap<ErpWorkstationMaterialRequestRequestDto, ErpWorkstationMaterialRequest>();
        }

        private static string GetStatusDescription(MaterialRequestStatus status)
        {
            return status switch
            {
                MaterialRequestStatus.Created => "已创建",
                MaterialRequestStatus.Processing => "处理中",
                MaterialRequestStatus.Completed => "已完成",
                MaterialRequestStatus.Cancelled => "已取消",
                MaterialRequestStatus.Failed => "处理失败",
                _ => "未知状态"
            };
        }
    }
}
