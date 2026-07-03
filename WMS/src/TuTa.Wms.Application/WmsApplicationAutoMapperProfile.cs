using AutoMapper;
using TuTa.Wms.Departments.Aggregates;
using TuTa.Wms.Departments.Dtos;
using TuTa.Wms.Erp.Dtos;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Log;
using TuTa.Wms.Materials.Aggregates;
using TuTa.Wms.Materials.Dtos;
using TuTa.Wms.Users.Dtos;
using TuTa.Wms.Warehouses.Aggregates;
using TuTa.Wms.Warehouses.Dtos;
using TuTa.Wms.Warehouses.Entities;
using Wms.LogTool;

namespace TuTa.Wms;

public class WmsApplicationAutoMapperProfile : Profile
{
    public WmsApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<SqliteLogItem, LogDto>();
        CreateMap<Material, MaterialDto>(MemberList.None);
        CreateMap<ErpStockAftChk, ErpGoodsAftChkDto>(MemberList.None);
        CreateMap<Warehouse, WarehouseDto>(MemberList.None);
        CreateMap<WarehouseArea, WarehouseAreaDto>(MemberList.None);
        CreateMap<Volo.Abp.Identity.IdentityUser, LoginOutput>(MemberList.None);
        CreateMap<Department, DepartmentDto>(MemberList.None);
    }
}
