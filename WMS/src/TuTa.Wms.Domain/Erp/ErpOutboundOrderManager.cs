using System;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Repositories;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP出库单管理器
    /// </summary>
    public class ErpOutboundOrderManager : WmsDomainService
    {
        private readonly IErpOutboundOrderRepository _erpOutboundOrderRepository;

        public ErpOutboundOrderManager(
            IErpOutboundOrderRepository erpOutboundOrderRepository)
        {
            _erpOutboundOrderRepository = erpOutboundOrderRepository;
        }

        /// <summary>
        /// 创建ERP出库单
        /// </summary>
        public async Task<ErpOutboundOrder> CreateOutboundOrderAsync(
            string outboundOrderNo,
            string warehouseCode,
            DateTime planOutboundDate,
            string outboundReason = null,
            string sourceDocument = null,
            string sourceDocumentNo = null,
            string handler = null,
            string remarks = null)
        {
            // 检查出库单号是否已存在
            var existingOrder = await _erpOutboundOrderRepository.FindByOutboundOrderNoAsync(outboundOrderNo);
            if (existingOrder != null)
                throw new Exception($"出库单号{outboundOrderNo}已存在，不能重复创建");

            var outboundOrder = new ErpOutboundOrder(
                GuidGenerator.Create(),
                outboundOrderNo,
                warehouseCode,
                planOutboundDate,
                outboundReason,
                sourceDocument,
                sourceDocumentNo,
                handler,
                remarks);

            return outboundOrder;
        }

        /// <summary>
        /// 验证出库单数据
        /// </summary>
        public void ValidateOutboundOrderData(
            string outboundOrderNo,
            string warehouseCode,
            DateTime planOutboundDate)
        {
            if (string.IsNullOrWhiteSpace(outboundOrderNo))
                throw new Exception("出库单号不能为空");

            if (string.IsNullOrWhiteSpace(warehouseCode))
                throw new Exception("仓库代号不能为空");

            if (planOutboundDate == default)
                throw new Exception("计划出库日期不能为空");
        }
    }
}
