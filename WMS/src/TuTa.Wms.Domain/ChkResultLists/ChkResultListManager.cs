using System;
using System.Threading.Tasks;
using TuTa.Wms.ChkResultLists.Aggregates;
using TuTa.Wms.ChkResultLists.ValueObjects;
using TuTa.Wms.Stocks;

namespace TuTa.Wms.ChkResultLists
{
    public class ChkResultListManager : WmsDomainService
    {
        private readonly IChkResultListRepository _chkResultListRepository;

        public ChkResultListManager(IChkResultListRepository chkResultListRepository)
        {
            _chkResultListRepository = chkResultListRepository;
        }

        public async Task<ChkResultList> CreateChkResultListAsync(
            string barcode,
            MaterialInfoOfChkRsltList materialInformation,
            CountInfoOfChkRsltList countInformation,
            CheckInfoOfChkRsltList checkInformation,
            SupplierInfoOfChkRsltList supplierInformation,
            WarehouseInfoOfChkRsltList warehouseInformation,
            StockInType stockInType,
            string batchCode,
            string bLCode,
            string bHCode)
        {
            var listExist = await _chkResultListRepository.FindByBarcodeAndCheckTypeAsync(barcode, checkInformation.CheckType).ConfigureAwait(false);
            if (listExist != null)
            {
                throw new Exception($"已经存在检验类型为{checkInformation.CheckType}，收料码为{barcode}的检验结论信息，请勿重复添加");
            }

            ChkResultList list = new ChkResultList(
                GuidGenerator.Create(),
                barcode,
                materialInformation,
                countInformation,
                checkInformation,
                supplierInformation,
                warehouseInformation,
                stockInType,
                batchCode,
                bLCode,
                bHCode);

            return list;
        }
    }
}
