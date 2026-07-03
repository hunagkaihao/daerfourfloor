using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuTa.Wms.BarcodeChecks.Aggregates;
using TuTa.Wms.BarcodeLists.ValueObjects;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.Stocks;

using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.BarcodeLists.Aggregates
{
    public class BarcodeList:AuditedAggregateRoot<Guid>
    {
        private BarcodeList()
        {

        }

        internal BarcodeList(
            Guid id
            ,string barcode
            //, string barcodeId
            , string purchaseId
            , DateTime? slDate
            , SupplierInfoOfBarcodeList supplier
            , MaterialInfoOfBarcodeList material
            , WarehouseInfoOfBarcodeList warehouse
            , CountInfoOfBarcodeList receiveCount
            , int isTag
            , StockInType stockInType
            , string batchCode
            , string bLCode
            , string bHCode
            , string mh
            ) :base(id)
        {
            Barcode = Check.NotNullOrWhiteSpace(barcode,nameof(barcode));
            //BarcodeId = Check.NotNullOrWhiteSpace(barcodeId,nameof(barcodeId));
            PurchaseId = purchaseId;
            Supplier = Check.NotNull(supplier,nameof(supplier));
            SLDate = slDate;
            Material = Check.NotNull(material,nameof(material)) ;
            Warehouse = Check.NotNull(warehouse, nameof(warehouse));
            IsTag = isTag;
            ReceiveCount = Check.NotNull(receiveCount, nameof(receiveCount));
            StockInType = stockInType;
            BatchCode = batchCode;
            BLCode = bLCode;
            BHCode = bHCode;
            Status = ChkResultListStatus.Create;
            InBoundedCount = 0;
            InBindCount = 0;

            isCheckOut = "0";
            isCheckOutCount = 0;
            PRDT_MH = mh;
        }

        public void ModifyChkResultList(
            MaterialInfoOfBarcodeList materialInformation,
            CountInfoOfBarcodeList countInformation,
            SupplierInfoOfBarcodeList supplierInformation,
            WarehouseInfoOfBarcodeList warehouseInformation,
            int isTag,
            DateTime? slDate,
            StockInType stockInType,
            string batchCode,
            string bLCode,
            string bHCode,
            string mh)
        {
            if (Status != ChkResultListStatus.Create)
                throw new Exception($"收料码为{Barcode}的入库单已经在入库中或已经入库完成，不能修改");

            Material = Check.NotNull(materialInformation, nameof(materialInformation));
            ReceiveCount = Check.NotNull(countInformation, nameof(countInformation));
            Supplier = Check.NotNull(supplierInformation, nameof(supplierInformation));
            Warehouse = Check.NotNull(warehouseInformation, nameof(warehouseInformation));
            SLDate = slDate;
            IsTag = isTag;
            StockInType = stockInType;
            BatchCode = batchCode;
            BLCode = bLCode;
            BHCode = bHCode;
            PRDT_MH = mh;
        }


        /// <summary>
        /// 收料条形码
        /// </summary>
        [StringLength(30)]
        [Required]
        public virtual string Barcode { get; private set; }

        /// <summary>
        /// 收料单号
        /// </summary>
        [StringLength(40)]
        public virtual string BarcodeId { get; private set; }

        /// <summary>
        /// 采购单号
        /// </summary>
        [StringLength(40)]
        public virtual string PurchaseId { get; private set; }

        /// <summary>
        /// 供应商信息
        /// </summary>
        [Required]
        public virtual SupplierInfoOfBarcodeList Supplier { get; private set; }

        /// <summary>
        /// 收料日期
        /// </summary>
        [Column(TypeName = "datetime")]
        public virtual DateTime? SLDate { get; private set; }

        /// <summary>
        /// 物料信息
        /// </summary>
        [Required]
        public virtual MaterialInfoOfBarcodeList Material { get; private set; }

        /// <summary>
        /// 存储仓库信息
        /// </summary>
        [Required]
        public virtual WarehouseInfoOfBarcodeList Warehouse { get; private set; }

        /// <summary>
        /// 是否需要检验，1需要2不需要
        /// </summary>
        public virtual int IsTag {  get; private set; }

        /// <summary>
        /// 数量信息
        /// </summary>
        [Required]
        public virtual CountInfoOfBarcodeList ReceiveCount { get; private set; }

        /// <summary>
        /// 入库类型  1(正常采购） 4(委托加工） 7(超期复检） 18 车间退货入仓
        /// </summary>
        public virtual StockInType StockInType { get; private set; }

        /// <summary>
        /// 生产批号
        /// </summary>
        [StringLength(180)]
        public virtual string BatchCode { get; private set; }

        /// <summary>
        /// 备料单号
        /// </summary>
        [StringLength(30)]
        public virtual string BLCode { get; private set; }

        /// <summary>
        /// 备货单号
        /// </summary>
        [StringLength(30)]
        public virtual string BHCode { get; private set; }

        /// <summary>
        /// 入库状态
        /// </summary>
        public virtual ChkResultListStatus Status { get; private set; }

        /// <summary>
        /// 已入库数
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal InBoundedCount { get; set; }

        /// <summary>
        /// 已绑定数
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal InBindCount { get; set; }

        public virtual string isCheckOut { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal isCheckOutCount { get; set; }


        [StringLength(60)]
        public virtual string PRDT_MH { get; set; }
    }
}
