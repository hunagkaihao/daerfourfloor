using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TuTa.Wms.ChkResultLists.Entities;
using TuTa.Wms.ChkResultLists.Events;
using TuTa.Wms.ChkResultLists.ValueObjects;
using TuTa.Wms.Stocks;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.ChkResultLists.Aggregates
{
    public class ChkResultList : AuditedAggregateRoot<Guid>
    {
        private ChkResultList()
        {            
        }

        internal ChkResultList(
            Guid id,
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
            : base(id)
        {
            Barcode = Check.NotNullOrWhiteSpace(barcode, nameof(barcode));
            Material = Check.NotNull(materialInformation, nameof(materialInformation));
            ReceiveCount = Check.NotNull(countInformation, nameof(countInformation));
            CheckData = Check.NotNull(checkInformation, nameof(checkInformation));
            Supplier = Check.NotNull(supplierInformation, nameof(supplierInformation));
            Warehouse = Check.NotNull(warehouseInformation, nameof(warehouseInformation));
            StockInType = stockInType;

            BatchCode = batchCode;
            BLCode = bLCode;
            BHCode = bHCode;

            Status = ChkResultListStatus.Create;
            InBoundedCount = 0;
            ChkResultBoxes = new List<ChkResultBox>();

            //超期复检入库，则向相应库存对象发送更新检测数据事件
            if (CheckData.CheckType == EnumCheckType.ReCheck && StockInType == StockInType.RecheckStockIn)
            {
                AddLocalEvent(new RecheckResultGettedEvent()
                {
                    BarcodeOfRecheckStock = this.Barcode,
                    CheckOrderCode = this.CheckData.CheckOrderCode,
                    CheckDate = this.CheckData.CheckDate,
                    CheckNo = this.CheckData.CheckNo,
                    CheckNoBeforeReCheck = this.CheckData.CheckNoBeforeReCheck,
                    CheckType = this.CheckData.CheckType,
                    CheckResult = this.CheckData.CheckResult,
                    PassCnt = this.CheckData.PassCnt
                });

                //if (CheckData.CheckResult == 1)
                //    AddLocalEvent(new RecheckResultGettedEvent()
                //    {
                //        BarcodeOfRecheckStock = Barcode,
                //        CheckOrderCode = this.CheckData.CheckOrderCode,
                //        CheckDate = this.CheckData.CheckDate,
                //        CheckNo = this.CheckData.CheckNo,
                //        CheckNoBeforeReCheck = this.CheckData.CheckNoBeforeReCheck,
                //        CheckType = this.CheckData.CheckType,
                //        CheckResult = this.CheckData.CheckResult,
                //        PassCnt = this.CheckData.PassCnt
                //    });
                //else if (CheckData.CheckResult == 2)
                //    AddLocalEvent(new ChkResultList_RecheckUnPassEvent()
                //    {
                //        BarcodeOfRecheckStock = Barcode,
                //        CheckOrderCode = this.CheckData.CheckOrderCode,
                //        CheckDate = this.CheckData.CheckDate,
                //        CheckNo = this.CheckData.CheckNo,
                //        CheckNoBeforeReCheck = this.CheckData.CheckNoBeforeReCheck,
                //        CheckType = this.CheckData.CheckType,
                //        CheckResult = this.CheckData.CheckResult,
                //        PassCnt = this.CheckData.PassCnt
                //    });
            }
        }
        
        public void ModifyChkResultList(
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
            if (Status != ChkResultListStatus.Create)
                throw new Exception($"收料码为{Barcode}的入库单已经在入库中或已经入库完成，不能修改");

            Material = Check.NotNull(materialInformation, nameof(materialInformation));
            ReceiveCount = Check.NotNull(countInformation, nameof(countInformation));
            CheckData = Check.NotNull(checkInformation, nameof(checkInformation));
            Supplier = Check.NotNull(supplierInformation, nameof(supplierInformation));
            Warehouse = Check.NotNull(warehouseInformation, nameof(warehouseInformation));
            StockInType = stockInType;

            BatchCode = batchCode;
            BLCode = bLCode;
            BHCode = bHCode;

            //超期复检入库，如果检验结果为合格，则发送解冻结事件，不合格发送冻结事件
            if (CheckData.CheckType == EnumCheckType.ReCheck && StockInType == StockInType.RecheckStockIn)
            {
                AddLocalEvent(new RecheckResultGettedEvent()
                {
                    BarcodeOfRecheckStock = this.Barcode,
                    CheckOrderCode = this.CheckData.CheckOrderCode,
                    CheckDate = this.CheckData.CheckDate,
                    CheckNo = this.CheckData.CheckNo,
                    CheckNoBeforeReCheck = this.CheckData.CheckNoBeforeReCheck,
                    CheckType = this.CheckData.CheckType,
                    CheckResult = this.CheckData.CheckResult,
                    PassCnt = this.CheckData.PassCnt
                });

                //if (CheckData.CheckResult == 1)
                //    AddLocalEvent(new RecheckResultGettedEvent()
                //    {
                //        BarcodeOfRecheckStock = Barcode,
                //        CheckOrderCode = this.CheckData.CheckOrderCode,
                //        CheckDate = this.CheckData.CheckDate,
                //        CheckNo = this.CheckData.CheckNo,
                //        CheckNoBeforeReCheck = this.CheckData.CheckNoBeforeReCheck,
                //        CheckType = this.CheckData.CheckType,
                //        CheckResult = this.CheckData.CheckResult
                //    });
                //else if (CheckData.CheckResult == 2)
                //    AddLocalEvent(new ChkResultList_RecheckUnPassEvent()
                //    {
                //        BarcodeOfRecheckStock = Barcode,
                //        CheckOrderCode = this.CheckData.CheckOrderCode,
                //        CheckDate = this.CheckData.CheckDate,
                //        CheckNo = this.CheckData.CheckNo,
                //        CheckNoBeforeReCheck = this.CheckData.CheckNoBeforeReCheck,
                //        CheckType = this.CheckData.CheckType,
                //        CheckResult = this.CheckData.CheckResult
                //    });
            }
        }

        //检后入库，一期检验都是合格的，均需要入库
        public void BindToBoxAndCell(
            Guid boxId, string boxCode, string boxName,
            Guid cellId, string cellCode, string cellName,
            int? areaId, string areaCode, string areaName,
            Guid houseId, string houseCode, string houseName,
            decimal inboundCount)
        {
            if (inboundCount <= 0)
                throw new Exception("入库数量不能小于等于0");

            if (inboundCount + InBoundedCount > CheckData.PassCnt)
                throw new Exception($"入库{inboundCount}数量后，总入库数量超过了该检验结果的总通过数");

            //东方电子一期只有一个仓库，仓库码为01
            if (houseCode != "01")
                throw new Exception($"实际入库仓库码为{houseCode}，有效仓库码应为01");

            //东方电子一期入库必须指定库区
            if (areaId == null || areaCode == null || areaName == null)
                throw new Exception("未指定入库库区或入库库区数据不全");

            //东方电子一期的入库仓库只能是01或26
            if (Warehouse.TargetWarehouseCode != "01" && Warehouse.TargetWarehouseCode != "26" && Warehouse.TargetWarehouseCode != "04")
                throw new Exception($"Erp指定的入库仓库{Warehouse.TargetWarehouseCode}无法识别，应为01或26或04");

            //东方电子一期的ERP指定01综合库，代表入正常区
            if (Warehouse.TargetWarehouseCode == "01" && areaName != "正常区")
                throw new Exception($"Erp指定的入库仓库为01，对应库区为\"正常区\"，但实际入库库区为{areaName}");

            //东方电子一期的ERP指定26暂存库，代表入暂存区
            if (Warehouse.TargetWarehouseCode == "26" && areaName != "暂存区")
                throw new Exception($"Erp指定的入库仓库为26，对应库区为\"暂存区\"，但实际入库库区为{areaName}");

            //东方电子一期的ERP指定04待处理库，代表入待处理区
            if (Warehouse.TargetWarehouseCode == "04" && areaName != "待处理区")
                throw new Exception($"Erp指定的入库仓库为04，对应库区为\"待处理区\"，但实际入库库区为{areaName}");

            InBoundedCount += inboundCount;

            ChkResultBox boxExist = ChkResultBoxes.FirstOrDefault(o => o.BoxId == boxId);
            if (boxExist == null)
            {
                ChkResultBox newBoxAftChk = new ChkResultBox(Id, boxId, boxCode, boxName, inboundCount);
                newBoxAftChk.BindToCell(
                    houseId, houseCode, houseName,
                    areaId, areaCode, areaName,
                    cellId, cellCode, cellName);
                ChkResultBoxes.Add(newBoxAftChk);
            }
            else
            {
                boxExist.AddBindCount(inboundCount);
            }
            
            if (InBoundedCount < CheckData.PassCnt)
                Status = ChkResultListStatus.Used;
            else
                Status = ChkResultListStatus.Finished;
        }

        //入库单物料绑定到容器
        public void BindToBox(Guid boxId, string boxCode, string boxName, decimal bindCntToBox)
        {
            ChkResultBox boxExist = ChkResultBoxes.FirstOrDefault(o => o.BoxId == boxId);
            if (boxExist != null)
                throw new Exception($"检后物料已经绑定到Id为{boxId}的容器，请勿重复绑定");

            decimal countBinded = 0;
            foreach (var box in ChkResultBoxes)
            {
                countBinded += box.CountInBox;
            }

            if (countBinded + bindCntToBox > CheckData.PassCnt)
                throw new Exception($"新增绑定容器{bindCntToBox}后，总绑定到容器的数量超过该检验结果的合格放行总数{CheckData.PassCnt}");

            ChkResultBoxes.Add(new ChkResultBox(Id, boxId, boxCode, boxName, bindCntToBox));

            Status = ChkResultListStatus.Used;
        }

        public void setStatus(ChkResultListStatus status)
        {
            Status = status;
        }

        public void BindToCell(
            Guid houseId, string houseCode, string houseName,
            int? areaId, string areaCode, string areaName,
            Guid cellId, string cellCode, string cellName,
            Guid boxIdToBind)
        {
            ChkResultBox boxExist = ChkResultBoxes.FirstOrDefault(o => o.BoxId == boxIdToBind);
            if (boxExist == null)
                throw new Exception($"该检后物料未绑定过Id为{boxIdToBind}的容器，不能入库");

            if (boxExist.Status == BoxAftChkStatus.BindedToCell)
                throw new Exception($"检后物料所在的Id为{boxIdToBind}的容器已经绑定到库位，请勿重复绑定库位");

            if (InBoundedCount + boxExist.CountInBox > CheckData.PassCnt)
                throw new Exception($"新入库{boxExist.CountInBox}后，总入库数超过此检后物料总的合格放行数量{CheckData.PassCnt}");

            InBoundedCount += boxExist.CountInBox;
            boxExist.BindToCell(
                houseId, houseCode, houseName,
                areaId, areaCode, areaName,
                cellId, cellCode, cellName);

            if (InBoundedCount == CheckData.PassCnt)
                Status = ChkResultListStatus.Finished;
            else
                Status = ChkResultListStatus.Used;
        }

        /// <summary>
        /// 收料条形码，一次收料生成唯一性条码，WMS作为物料识别码，但可以分成多份与不同的容器进行绑定
        /// </summary>
        [StringLength(30)]
        [Required]
        public virtual string Barcode { get; private set; }

        /// <summary>
        /// 入库类型  1(正常采购） 2（生产入库：指半成品） 4(委托加工） 7(超期复检）
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
        /// 物料信息
        /// </summary>
        [Required]
        public virtual MaterialInfoOfChkRsltList Material { get; private set; }

        /// <summary>
        /// 数量信息
        /// </summary>
        [Required]
        public virtual CountInfoOfChkRsltList ReceiveCount { get; private set; }

        /// <summary>
        /// 检测信息
        /// </summary>
        [Required]
        public virtual CheckInfoOfChkRsltList CheckData { get; private set; }

        /// <summary>
        /// 供应商信息
        /// </summary>
        [Required]
        public virtual SupplierInfoOfChkRsltList Supplier { get; private set; }

        /// <summary>
        /// 存储仓库信息
        /// </summary>
        [Required]
        public virtual WarehouseInfoOfChkRsltList Warehouse { get; private set; }

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
        /// 绑定容器后产生的入库容器
        /// </summary>
        public List<ChkResultBox> ChkResultBoxes { get; private set; }
    }
}
