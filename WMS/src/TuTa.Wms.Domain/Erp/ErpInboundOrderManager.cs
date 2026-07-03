using System;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Repositories;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP入库单管理器
    /// </summary>
    public class ErpInboundOrderManager : WmsDomainService
    {
        private readonly IErpInboundOrderRepository _erpInboundOrderRepository;

        public ErpInboundOrderManager(
            IErpInboundOrderRepository erpInboundOrderRepository)
        {
            _erpInboundOrderRepository = erpInboundOrderRepository;
        }

        /// <summary>
        /// 创建ERP入库单
        /// </summary>
        public async Task<ErpInboundOrder> CreateInboundOrderAsync(
            string inboundOrderNo,
            string warehouseCode,
            DateTime planInboundDate,
            string inboundReason = null,
            string sourceDocument = null,
            string sourceDocumentNo = null,
            string handler = null,
            string remarks = null)
        {
            // 检查入库单号是否已存在
            var existingOrder = await _erpInboundOrderRepository.FindByInboundOrderNoAsync(inboundOrderNo);
            if (existingOrder != null)
                throw new Exception($"入库单号{inboundOrderNo}已存在，不能重复创建");

            var inboundOrder = new ErpInboundOrder(
                GuidGenerator.Create(),
                inboundOrderNo,
                warehouseCode,
                planInboundDate,
                inboundReason,
                sourceDocument,
                sourceDocumentNo,
                handler,
                remarks);

            return inboundOrder;
        }

        /// <summary>
        /// 验证入库单数据
        /// </summary>
        public void ValidateInboundOrderData(
            string inboundOrderNo,
            string warehouseCode,
            DateTime planInboundDate)
        {
            if (string.IsNullOrWhiteSpace(inboundOrderNo))
                throw new Exception("入库单号不能为空");

            if (string.IsNullOrWhiteSpace(warehouseCode))
                throw new Exception("仓库代号不能为空");

            if (planInboundDate == default)
                throw new Exception("计划入库日期不能为空");

            
        }
    }
}
