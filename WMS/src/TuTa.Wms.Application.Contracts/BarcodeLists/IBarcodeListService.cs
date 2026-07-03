using System;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.BarcodeLists.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.BarcodeLists
{
    public interface IBarcodeListService:IApplicationService
    {
        //查询物料条码
        Task<BarcodeDto> GetBarcodeAsync(string barcode);

        //查询物料条码打印
        Task<BarcodeDto> GetBarcodePrintAsync(string barcode);
    }
}
