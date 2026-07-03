using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
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
