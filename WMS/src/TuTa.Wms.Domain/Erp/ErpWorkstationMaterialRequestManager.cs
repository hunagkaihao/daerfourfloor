using System;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Repositories;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP工位叫料任务管理器
    /// </summary>
    public class ErpWorkstationMaterialRequestManager : WmsDomainService
    {
        private readonly IErpWorkstationMaterialRequestRepository _erpWorkstationMaterialRequestRepository;

        public ErpWorkstationMaterialRequestManager(
            IErpWorkstationMaterialRequestRepository erpWorkstationMaterialRequestRepository)
        {
            _erpWorkstationMaterialRequestRepository = erpWorkstationMaterialRequestRepository;
        }

        /// <summary>
        /// 创建ERP工位叫料任务
        /// </summary>
        public async Task<ErpWorkstationMaterialRequest> CreateMaterialRequestAsync(
            string sortingBatch,
            string deliveryPointLocation,
            DateTime deliveryTime)
        {
            // 验证输入参数
            ValidateMaterialRequestData(sortingBatch, deliveryPointLocation, deliveryTime);

            // 检查分拣批次是否已存在
            var existingRequest = await _erpWorkstationMaterialRequestRepository.ExistsBySortingBatchAsync(sortingBatch);
            if (existingRequest)
                throw new Exception($"分拣批次{sortingBatch}已存在，不能重复创建");

            var materialRequest = new ErpWorkstationMaterialRequest(
                GuidGenerator.Create(),
                sortingBatch,
                deliveryPointLocation,
                deliveryTime);

            return materialRequest;
        }

        /// <summary>
        /// 更新任务状态
        /// </summary>
        public void UpdateRequestStatus(ErpWorkstationMaterialRequest request, MaterialRequestStatus newStatus)
        {
            if (request == null)
                throw new Exception("物料请求任务不能为空");

            request.UpdateStatus(newStatus);
        }

        /// <summary>
        /// 添加备注
        /// </summary>
        public void AddRemarks(ErpWorkstationMaterialRequest request, string remarks)
        {
            if (request == null)
                throw new Exception("物料请求任务不能为空");

            request.AddRemarks(remarks);
        }

        /// <summary>
        /// 验证物料请求数据
        /// </summary>
        public void ValidateMaterialRequestData(
            string sortingBatch,
            string deliveryPointLocation,
            DateTime deliveryTime)
        {
            if (string.IsNullOrWhiteSpace(sortingBatch))
                throw new Exception("分拣批次不能为空");

            if (string.IsNullOrWhiteSpace(deliveryPointLocation))
                throw new Exception("配送点位置不能为空");

            if (deliveryTime == default)
                throw new Exception("配送时间不能为空");

            if (deliveryTime < DateTime.Now)
                throw new Exception("配送时间不能早于当前时间");
        }

        /// <summary>
        /// 验证状态转换是否有效
        /// </summary>
        public bool IsValidStatusTransition(MaterialRequestStatus currentStatus, MaterialRequestStatus newStatus)
        {
            switch (currentStatus)
            {
                case MaterialRequestStatus.Created:
                    return newStatus == MaterialRequestStatus.Processing || 
                           newStatus == MaterialRequestStatus.Cancelled;
                
                case MaterialRequestStatus.Processing:
                    return newStatus == MaterialRequestStatus.Completed || 
                           newStatus == MaterialRequestStatus.Failed;
                
                case MaterialRequestStatus.Completed:
                case MaterialRequestStatus.Cancelled:
                case MaterialRequestStatus.Failed:
                    return false; // 这些状态是终态，不能再转换
                
                default:
                    return false;
            }
        }
    }
}
