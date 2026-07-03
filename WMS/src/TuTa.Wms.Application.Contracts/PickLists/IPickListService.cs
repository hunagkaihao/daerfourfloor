using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.PickLists.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.PickLists
{
    public interface IPickListService : IApplicationService
    {   
        /// <summary>
        /// 获取未完成的领料项数量
        /// </summary>
        /// <returns></returns>
        Task<int> GetUnFinishedPickItmCountAsync();

        /// <summary>
        /// 查询领料项
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<List<PickItemDto>> GetUnFinishedPickItemsAsync(PickItemQueryDto para);

        /// <summary>
        /// 分页查询领料项
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<PagedResultDto<PickItemDto>> GetPagedUnFinishedPickItemsAsync(PagedPickItemQueryDto para);

        /// <summary>
        /// 获取指定领料单中指定物料的领料建议
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<List<PickStockDto>> AllocatePickStocksAsync(PickStockAllocateDto para);

        /// <summary>
        /// 释放指定领料单中指定物料的领料建议
        /// </summary>
        /// <param name="pickItemId"></param>
        /// <returns></returns>
        Task<ResponseDto> ReleasePickStockAsync(int pickItemId);
        
        /// <summary>
        /// 获取物料对应领用单
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="boxCode"></param>
        /// <returns></returns>
        Task<GetByBarcodeBoxDto> GetByBarcodeBoxCode(string barcode,string boxCode);

        /// <summary>
        /// 下架
        /// </summary>
        /// <param name="startCellCode"></param>
        /// <param name="endCellCode"></param>
        /// <param name="pickListCode"></param>
        /// <param name="uniqueCode"></param>
        /// <returns></returns>
        Task<ResponseDto> PickOutDownAsync(string startCellCode,string endCellCode, string pickListCode, string uniqueCode, string operatorName = null);
        Task<ResponseDto> CheckDownAsync(string startCellCode, string endCellCode);
        Task<ResponseDto> NoHaveDownAsync(int count,string type,string area,string endArea);

        /// <summary>
        /// 周转区出库
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="boxCode"></param>
        /// <param name="count"></param>
        /// <param name="pickListCode"></param>
        /// <param name="uniqueCode"></param>
        /// <returns></returns>
        Task<ResponseDto> PickOutByZZ(string barcode, string boxCode, decimal count, string pickListCode, string uniqueCode, string operatorName = null);


        Task<ResponseDto> PickOutByBox(string barcode, string boxCode, decimal count, string pickListCode, string uniqueCode,string nextBoxCode,string nextCellCode, string operatorName = null);

        /// <summary>
        /// 领料
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<ResponseDto> PickOutAsync(string pickListCode, string pickItemUniqueCode, PickOutDto para);

        /// <summary>
        /// 创建无计划领用单
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<ResponseDto> CreateNoPlanPickListAsync(NoPlanPickOutCreateDto para);

        /// <summary>
        /// 删除无计划领用单
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<ResponseDto> DeleteNoPlanPickListAsync(NoPlanPickListDelDto para);

        /// <summary>
        /// 编辑无计划领用单
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<ResponseDto> EditNoPlanPickListAsync(NoPlanPickListEditDto para);

        /// <summary>
        /// 查询无计划领用单
        /// </summary>
        /// <returns></returns>
        Task<PagedResultDto<PickItemDto>> GetPagedNoPlanPickListAsync(PagedNoPlanPickItemsQueryDto para);

        /// <summary>
        /// 获取所有的无计划领料类型
        /// </summary>
        /// <returns></returns>
        List<NoPlanPickTypeDto> GetAllNoPlanPickTypes();

        Task<List<string>> GetErpDeptTypeDetail();
    }
}
