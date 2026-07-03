using AutoMapper;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Dto;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP工位收料AutoMapper配置
    /// </summary>
    public class ErpWorkstationMaterialReceiptAutoMapperProfile : Profile
    {
        public ErpWorkstationMaterialReceiptAutoMapperProfile()
        {
            // 从聚合根到DTO的映射
            CreateMap<ErpWorkstationMaterialReceipt, ErpWorkstationMaterialReceiptDto>()
                .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreationTime));

            // 从DTO到聚合根的映射（如果需要的话）
            CreateMap<ErpWorkstationMaterialReceiptRequestDto, ErpWorkstationMaterialReceipt>();
        }
    }
}
