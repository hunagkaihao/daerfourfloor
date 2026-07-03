using AutoMapper;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Dto;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP物料AutoMapper配置
    /// </summary>
    public class ErpMaterialAutoMapperProfile : Profile
    {
        public ErpMaterialAutoMapperProfile()
        {
            // 物料映射
            CreateMap<ErpMaterial, ErpMaterialDto>();
        }
    }
}
