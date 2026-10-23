using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Erp.Entities
{
    /// <summary>
    /// ERP ASN单据实体
    /// </summary>
    public class ErpAsn : AuditedAggregateRoot<Guid>
    {
        private ErpAsn() { }

        public static ErpAsn Create(
            Guid id,
            string asnCode,
            string orderCode,
            string supplierCode,
            string supplierName,
            string warehouseCode,
            string warehouseName,
            string materialCode,
            string materialName,
            string specs,
            string unit,
            decimal planQuantity,
            string batchCode,
            DateTime? arrivalDate,
            string asnFlag,
            string businessType,
            string processTypeCode,
            string processTypeName,
            DateTime? shipDate,
            string departmentCode,
            string departmentName,
            string personCode,
            string personName,
            string exchangeName,
            string remarks = null,
            string maker = null,
            DateTime? billDate = null,
            string headcmemo = null,
            DateTime? arrivalDateB = null,
            string makeTime = null,
            decimal taxRate = 0,
            decimal exchangeRate = 0,
            long? poDetailId = null,
            long? erpOrderDetailId = null,
            bool isGsp = false,
            string closer = null,
            string free2 = null,
            string free3 = null,
            string free5 = null,
            string materialAddCode = null,
            decimal notArrivedQuantity = 0)
        {
            Check.NotNullOrWhiteSpace(asnCode, nameof(asnCode));

            var asn = new ErpAsn
            {
                Id = id,
                AsnCode = asnCode,
                OrderCode = orderCode,
                SupplierCode = supplierCode,
                SupplierName = supplierName,
                WarehouseCode = warehouseCode,
                WarehouseName = warehouseName,
                MaterialCode = materialCode,
                MaterialName = materialName,
                Specs = specs,
                Unit = unit,
                PlanQuantity = planQuantity,
                BatchCode = batchCode,
                ArrivalDate = arrivalDate,
                AsnFlag = asnFlag,
                BusinessType = businessType,
                ProcessTypeCode = processTypeCode,
                ProcessTypeName = processTypeName,
                ShipDate = shipDate,
                DepartmentCode = departmentCode,
                DepartmentName = departmentName,
                PersonCode = personCode,
                PersonName = personName,
                ExchangeName = exchangeName,
                Status = AsnStatus.Created,
                Remarks = remarks,
                Maker = maker,
                BillDate = billDate,
                Headcmemo = headcmemo,
                ArrivalDateB = arrivalDateB,
                MakeTime = makeTime,
                TaxRate = taxRate,
                ExchangeRate = exchangeRate,
                PoDetailId = poDetailId,
                ErpOrderDetailId = erpOrderDetailId,
                IsGsp = isGsp,
                Closer = closer,
                Free2 = free2,
                Free3 = free3,
                Free5 = free5,
                MaterialAddCode = materialAddCode,
                NotArrivedQuantity = notArrivedQuantity
            };

            return asn;
        }

        [StringLength(50)]
        [Required]
        public virtual string AsnCode { get; private set; }

        [StringLength(50)]
        public virtual string OrderCode { get; private set; }

        [StringLength(20)]
        public virtual string SupplierCode { get; private set; }

        [StringLength(100)]
        public virtual string SupplierName { get; private set; }

        [StringLength(20)]
        public virtual string WarehouseCode { get; private set; }

        [StringLength(100)]
        public virtual string WarehouseName { get; private set; }

        [StringLength(50)]
        public virtual string MaterialCode { get; private set; }

        [StringLength(200)]
        public virtual string MaterialName { get; private set; }

        [StringLength(200)]
        public virtual string Specs { get; private set; }

        [StringLength(10)]
        public virtual string Unit { get; private set; }

        /// <summary>U8到货单单位标识cunitid；使用已有扩展属性持久化，历史记录缺失时为空。</summary>
        [NotMapped]
        public string UnitId => this.GetProperty<string>("U8UnitId") ?? string.Empty;

        /// <summary>保存ASN源单位标识，不使用单位名称或示例编码替代。</summary>
        /// <param name="unitId">ASN的cunitid原值；null按未提供处理，保存为空字符串。</param>
        public void SetUnitId(string unitId)
        {
            this.SetProperty("U8UnitId", unitId ?? string.Empty);
        }

        public virtual decimal PlanQuantity { get; private set; }

        [StringLength(50)]
        public virtual string BatchCode { get; private set; }

        public virtual DateTime? ArrivalDate { get; private set; }

        [StringLength(20)]
        public virtual string AsnFlag { get; private set; }

        [StringLength(50)]
        public virtual string BusinessType { get; private set; }

        [StringLength(20)]
        public virtual string ProcessTypeCode { get; private set; }

        [StringLength(100)]
        public virtual string ProcessTypeName { get; private set; }

        public virtual DateTime? ShipDate { get; private set; }

        [StringLength(20)]
        public virtual string DepartmentCode { get; private set; }

        [StringLength(100)]
        public virtual string DepartmentName { get; private set; }

        [StringLength(20)]
        public virtual string PersonCode { get; private set; }

        [StringLength(50)]
        public virtual string PersonName { get; private set; }

        [StringLength(20)]
        public virtual string ExchangeName { get; private set; }

        [Required]
        public virtual AsnStatus Status { get; private set; }

        public virtual decimal ArrivedQuantity { get; private set; }

        public virtual decimal OutQuantity { get; private set; }

        public virtual decimal InWarehouseQuantity { get; private set; }

        /// <summary>
        /// 已经入库数量
        /// </summary>
        public virtual decimal? AlreadyStockInQuantity { get; private set; }

        public virtual decimal RealQuantity { get; private set; }

        [StringLength(500)]
        public virtual string Remarks { get; private set; }

        [StringLength(50)]
        public virtual string Maker { get; private set; }

        public virtual DateTime? BillDate { get; private set; }

        public virtual long? ErpOrderId { get; private set; }

        public virtual long? ErpOrderDetailId { get; private set; }

        [StringLength(50)]
        public virtual string Headcmemo { get; private set; }

        public virtual DateTime? ArrivalDateB { get; private set; }

        [StringLength(20)]
        public virtual string MakeTime { get; private set; }

        public virtual decimal TaxRate { get; private set; }

        public virtual decimal ExchangeRate { get; private set; }

        public virtual long? PoDetailId { get; private set; }

        public virtual bool IsGsp { get; private set; }

        [StringLength(50)]
        public virtual string Closer { get; private set; }

        [StringLength(100)]
        public virtual string Free2 { get; private set; }

        [StringLength(100)]
        public virtual string Free3 { get; private set; }

        [StringLength(100)]
        public virtual string Free5 { get; private set; }

        [StringLength(50)]
        public virtual string MaterialAddCode { get; private set; }

        public virtual decimal NotArrivedQuantity { get; private set; }

        public virtual decimal StockInQuantity { get; private set; }

        public virtual DateTime? LastStockInTime { get; private set; }

        public virtual bool IsPushedToErp { get; private set; }

        public virtual DateTime? PushTime { get; private set; }

        /// <summary>到货单推送任务UID，失败重试沿用，避免U8重复生单。</summary>
        public virtual long? ArrivalPushUid { get; private set; }

        /// <summary>U8到货单主表ID；历史未关联数据为空。</summary>
        public virtual long? ArrivalVoucherId { get; private set; }

        /// <summary>U8生成的到货单号，用于报检来源关联。</summary>
        [StringLength(50)]
        public virtual string ArrivalVoucherCode { get; private set; }

        /// <summary>U8到货单明细autoid，不能使用本地ASN的GUID代替。</summary>
        public virtual long? ArrivalDetailId { get; private set; }

        /// <summary>最近一次到货单失败原因；成功后清空。</summary>
        [StringLength(2000)]
        public virtual string ArrivalPushError { get; private set; }

        /// <summary>最近一次到货单推送尝试时间。</summary>
        public virtual DateTime? ArrivalPushAttemptTime { get; private set; }

        /// <summary>报检推送UID，同一到货单失败重试沿用。</summary>
        public virtual long? InspectionPushUid { get; private set; }

        /// <summary>是否已经成功生成U8来料报检单。</summary>
        public virtual bool IsInspectionReportPushed { get; private set; }

        /// <summary>来料报检单推送失败原因。</summary>
        [StringLength(2000)]
        public virtual string InspectionPushError { get; private set; }

        /// <summary>在网络请求前保留整单推送UID。</summary>
        public void BeginArrivalPush(long uid)
        {
            ArrivalPushUid = uid;
            ArrivalPushAttemptTime = DateTime.Now;
        }

        /// <summary>记录返回的来源单据关联；缺失明细ID时仍保留已生单状态防止重建。</summary>
        public void CompleteArrivalPush(long? voucherId, string voucherCode, long? detailId)
        {
            ArrivalVoucherId = voucherId;
            ArrivalVoucherCode = voucherCode;
            ArrivalDetailId = detailId;
            ArrivalPushError = voucherId.HasValue && !string.IsNullOrWhiteSpace(voucherCode) && detailId.HasValue
                ? null : "到货单已生成，但U8来源关联不完整，请核对到货单；禁止重复生成";
            MarkAsPushedToErp();
        }

        /// <summary>保存ERP错误供管理页重试，不改变本地组盘结果。</summary>
        public void FailArrivalPush(string error)
        {
            ArrivalPushError = LimitError(error);
        }

        /// <summary>记录来料报检结果；失败时保留UID，成功后禁止重复生单。</summary>
        public void RecordInspectionPush(long uid, bool success, string error)
        {
            InspectionPushUid = uid;
            IsInspectionReportPushed = success;
            InspectionPushError = success ? null : LimitError(error);
        }

        /// <summary>已提交箱码台账，同一箱重复提交不得重复入账；数据使用既有扩展属性保存。</summary>
        public bool HasGroupedBox(string boxKey)
        {
            var data = this.GetProperty<string>("U8GroupedBoxes");
            return data != null && (JsonConvert.DeserializeObject<HashSet<string>>(data) ?? new HashSet<string>()).Contains(boxKey);
        }

        /// <summary>登记已完成组盘的箱标识，与组盘数量在同一事务提交。</summary>
        public void RecordGroupedBox(string boxKey)
        {
            var data = this.GetProperty<string>("U8GroupedBoxes");
            var boxes = data == null ? new HashSet<string>() : JsonConvert.DeserializeObject<HashSet<string>>(data) ?? new HashSet<string>();
            if (!boxes.Add(boxKey)) throw new BusinessException("该箱已组盘，不能重复入账");
            this.SetProperty("U8GroupedBoxes", JsonConvert.SerializeObject(boxes));
        }

        // 复用已映射的ExtraProperties持久化采购推送信息，兼容已有ASN记录，无需增加数据库列。
        /// <summary>采购推送任务号，首次生成后失败重试保持不变。</summary>
        [NotMapped]
        public string PurchaseTaskId => this.GetProperty<string>("U8PurchaseTaskId");
        /// <summary>已冻结的采购入库内层报文，重试不得更换来源或数量。</summary>
        [NotMapped]
        public string PurchasePayload => this.GetProperty<string>("U8PurchasePayload");
        /// <summary>U8已确认采购入库成功，禁止重复生成。</summary>
        [NotMapped]
        public bool IsPurchasePushed => this.GetProperty<string>("U8PurchasePushed") == "true";
        /// <summary>最近采购推送错误，审核与数量等待提示不计为成功。</summary>
        [NotMapped]
        public string PurchasePushError => this.GetProperty<string>("U8PurchaseError");
        /// <summary>冻结首次提交的数据和任务号；返回采购失败时仍保留。</summary>
        public void BeginPurchasePush(string taskId, string payload)
        {
            if (IsPurchasePushed) throw new BusinessException("采购入库已推送");
            this.SetProperty("U8PurchaseTaskId", taskId);
            this.SetProperty("U8PurchasePayload", payload);
        }
        /// <summary>独立记录采购推送结果及原始回执，避免重复回补样品库存。</summary>
        public void RecordPurchasePush(bool success, string message, string data)
        {
            this.SetProperty("U8PurchasePushed", success ? "true" : "false");
            this.SetProperty("U8PurchaseError", success ? null : LimitError(message));
            if (success) this.SetProperty("U8PurchaseReceipt", data);
        }

        /// <summary>限制接口异常文本长度，确保错误信息可落库。</summary>
        private static string LimitError(string error)
        {
            return error?.Length > 2000 ? error.Substring(0, 2000) : error;
        }

        public void SetStatus(AsnStatus status)
        {
            Status = status;
        }

        public void UpdateQuantity(
            decimal arrivedQuantity,
            decimal outQuantity,
            decimal inWarehouseQuantity,
            decimal realQuantity)
        {
            ArrivedQuantity = arrivedQuantity;
            OutQuantity = outQuantity;
            InWarehouseQuantity = inWarehouseQuantity;
            RealQuantity = realQuantity;
        }

        public void SetAlreadyStockInQuantity(decimal? quantity)
        {
            AlreadyStockInQuantity = quantity;
        }

        /// <summary>
        /// 组盘入库推送失败时回滚：还原已入库数量并更新状态
        /// </summary>
        public void RevertAlreadyStockInQuantity(decimal quantity)
        {
            if (quantity <= 0)
            {
                throw new BusinessException("回滚数量必须大于0");
            }

            var current = AlreadyStockInQuantity ?? 0;
            if (current < quantity)
            {
                throw new BusinessException(
                    $"订单号{OrderCode}当前已入库数量{current}小于需要回滚的数量{quantity}");
            }

            AlreadyStockInQuantity = current - quantity;

            if (AlreadyStockInQuantity <= 0)
            {
                Status = AsnStatus.Created;
            }
            else if (AlreadyStockInQuantity >= InWarehouseQuantity)
            {
                Status = AsnStatus.Completed;
            }
            else
            {
                Status = AsnStatus.Received;
            }
        }

        public void AddAlreadyStockInQuantity(decimal quantity)
        {
            AlreadyStockInQuantity = (AlreadyStockInQuantity ?? 0) + quantity;
        }

        /// <summary>
        /// 组盘入库：累加已入库数量并更新状态
        /// </summary>
        public void ApplyAlreadyStockInQuantity(decimal quantity)
        {
            if (quantity <= 0)
            {
                throw new BusinessException("组盘数量必须大于0");
            }

            var current = AlreadyStockInQuantity ?? 0;
            if (current + quantity > InWarehouseQuantity)
            {
                throw new BusinessException(
                    $"订单号{OrderCode}已入库数量{current}加本次组盘数量{quantity}不能超过入库数{InWarehouseQuantity}");
            }

            AlreadyStockInQuantity = current + quantity;

            if (AlreadyStockInQuantity >= InWarehouseQuantity)
            {
                Status = AsnStatus.Completed;
            }
            else
            {
                Status = AsnStatus.Received;
            }
        }

        public void AddStockInQuantity(decimal quantity)
        {
            StockInQuantity += quantity;
            LastStockInTime = DateTime.Now;
            
            if (StockInQuantity >= PlanQuantity && Status != AsnStatus.Completed)
            {
                Status = AsnStatus.Completed;
            }
        }

        public bool IsStockInCompleted()
        {
            return StockInQuantity >= PlanQuantity;
        }

        /// <summary>
        /// 待入库数量 = 应入库数量(InWarehouseQuantity) - 已经入库数量
        /// </summary>
        public decimal GetPendingStockInQuantity()
        {
            var pending = InWarehouseQuantity - (AlreadyStockInQuantity ?? 0);
            return pending > 0 ? pending : 0;
        }

        public void MarkAsPushedToErp()
        {
            IsPushedToErp = true;
            PushTime = DateTime.Now;
        }
    }

    public enum AsnStatus
    {
        Created = 1,
        Received = 2,
        Completed = 3,
        Cancelled = 4
    }
}
