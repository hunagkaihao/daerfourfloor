using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using TuTa.Wms.Erp.Dtos;
using System.Collections.Generic;
using TuTa.Wms.PickLists.Dtos;

namespace TuTa.Wms.Erp
{
    public interface IErpMidTableService : IApplicationService
    {
        //public Task<ErpGoodsAftChkDto> GetAftChkGoodsToGroupBoxByBarcodeAsync(string barcode);

        public Task<List<ErpPickManDto>> GetPickerNamesAsync(string nameTip);
    }
}
