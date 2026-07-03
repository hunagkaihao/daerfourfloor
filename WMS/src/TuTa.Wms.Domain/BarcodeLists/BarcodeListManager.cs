using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TuTa.Wms.BarcodeLists.Aggregates;
using TuTa.Wms.BarcodeLists.ValueObjects;
using TuTa.Wms.Stocks;

namespace TuTa.Wms.BarcodeLists
{
    public class BarcodeListManager:WmsDomainService
    {
        private readonly IBarcodeListRepository _barcodeListRepository;

        public BarcodeListManager(IBarcodeListRepository barcodeListRepository)
        {
            _barcodeListRepository = barcodeListRepository;
        }

        public async Task<BarcodeList> CreateBarcodeListAsync(
            string barcode,
            //string barcodeId,
            string purchaseId,
            DateTime? slDate,
            SupplierInfoOfBarcodeList supplierInformation,
            MaterialInfoOfBarcodeList materialInformation,
            WarehouseInfoOfBarcodeList warehouseInformation,
            CountInfoOfBarcodeList countInformation,
            int isTag,
            StockInType stockInType,
            string batchCode,
            string bLCode,
            string bHCode,
            string mh)
        {
            var listExist = await _barcodeListRepository.FindByBarcodeAsync(barcode).ConfigureAwait(false);
            if (listExist != null)
            {
                throw new Exception($"收料码为{barcode}的检验结论信息，请勿重复添加");
            }

            BarcodeList list = new BarcodeList(
                GuidGenerator.Create(),
                barcode,
                //barcodeId,
                purchaseId,
                slDate,
                supplierInformation,
                materialInformation,
                warehouseInformation,
                countInformation,
                isTag,
                stockInType,
                batchCode,
                bLCode,
                bHCode,
                mh);

            return list;
        }
    }
}
