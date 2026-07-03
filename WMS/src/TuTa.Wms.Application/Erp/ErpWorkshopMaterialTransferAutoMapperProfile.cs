using AutoMapper;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Dto;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP车间物料转移AutoMapper配置
    /// </summary>
    public class ErpWorkshopMaterialTransferAutoMapperProfile : Profile
    {
        public ErpWorkshopMaterialTransferAutoMapperProfile()
        {
            // 从聚合根到DTO的映射
            CreateMap<ErpWorkshopMaterialTransfer, ErpWorkshopMaterialTransferDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

            // 从DTO到聚合根的映射（如果需要的话）
            CreateMap<ErpWorkshopMaterialTransferDto, ErpWorkshopMaterialTransfer>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (MaterialTransferStatus)src.Status));
        }
    }
}
