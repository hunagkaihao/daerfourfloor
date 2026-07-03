using System;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Repositories;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP收料工位管理器
    /// </summary>
    public class ErpDeliveryStationManager : WmsDomainService
    {
        private readonly IErpDeliveryStationRepository _erpDeliveryStationRepository;

        public ErpDeliveryStationManager(
            IErpDeliveryStationRepository erpDeliveryStationRepository)
        {
            _erpDeliveryStationRepository = erpDeliveryStationRepository;
        }

        /// <summary>
        /// 创建或更新ERP收料工位
        /// </summary>
        public async Task<ErpDeliveryStation> CreateOrUpdateDeliveryStationAsync(
            string deliveryCode,
            long syncTimeStamp,
            string syncType)
        {
            // 检查配送位置代号是否已存在
            var existingStation = await _erpDeliveryStationRepository.FindByDeliveryCodeAsync(deliveryCode);
            
            if (existingStation != null)
            {
                // 更新现有收料工位
                existingStation.Update(syncTimeStamp, syncType);
                return existingStation;
            }
            else
            {
                // 创建新收料工位
                var deliveryStation = new ErpDeliveryStation(
                    GuidGenerator.Create(),
                    deliveryCode,
                    syncTimeStamp,
                    syncType);

                return deliveryStation;
            }
        }

        /// <summary>
        /// 验证收料工位数据
        /// </summary>
        public void ValidateDeliveryStationData(
            string deliveryCode,
            string syncType)
        {
            if (string.IsNullOrWhiteSpace(deliveryCode))
                throw new Exception("配送位置代号不能为空");

            if (string.IsNullOrWhiteSpace(syncType))
                throw new Exception("操作类型不能为空");

            // 验证操作类型
            if (syncType != "insert" && syncType != "modify" && syncType != "delete")
                throw new Exception("操作类型必须是 insert、modify 或 delete");
        }
    }
}
