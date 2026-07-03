using System;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Repositories;
using TuTa.Wms.Erp.Aggregates;
using Volo.Abp.Domain.Services;
using Volo.Abp;
using System.Linq; // Added missing import for FirstOrDefault

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP车间物料转移领域服务
    /// </summary>
    public class ErpWorkshopMaterialTransferManager : DomainService
    {
        private readonly IErpWorkshopMaterialTransferRepository _erpWorkshopMaterialTransferRepository;

        public ErpWorkshopMaterialTransferManager(IErpWorkshopMaterialTransferRepository erpWorkshopMaterialTransferRepository)
        {
            _erpWorkshopMaterialTransferRepository = erpWorkshopMaterialTransferRepository;
        }

        /// <summary>
        /// 创建车间物料转移任务
        /// </summary>
        /// <param name="startLocation">启动位置</param>
        /// <param name="endLocation">终点位置</param>
        /// <returns>车间物料转移任务</returns>
        public async Task<ErpWorkshopMaterialTransfer> CreateMaterialTransferTaskAsync(string startLocation, string endLocation)
        {
            // 验证位置信息
            if (string.IsNullOrWhiteSpace(startLocation))
            {
                throw new BusinessException("启动位置不能为空");
            }

            if (string.IsNullOrWhiteSpace(endLocation))
            {
                throw new BusinessException("终点位置不能为空");
            }

            if (startLocation.Equals(endLocation, StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException("启动位置和终点位置不能相同");
            }

            // 检查是否已有相同的待处理任务
            var existingTasks = await _erpWorkshopMaterialTransferRepository.FindByLocationsAsync(startLocation, endLocation);
            var pendingTask = existingTasks.FirstOrDefault(t => t.Status == MaterialTransferStatus.Pending);
            
            if (pendingTask != null)
            {
                throw new BusinessException($"已存在从 {startLocation} 到 {endLocation} 的待处理转移任务");
            }

            // 创建转移任务
            var materialTransfer = ErpWorkshopMaterialTransfer.Create(startLocation, endLocation);
            
            return materialTransfer;
        }

        /// <summary>
        /// 更新转移任务状态
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <param name="status">新状态</param>
        /// <returns>更新后的任务</returns>
        public async Task<ErpWorkshopMaterialTransfer> UpdateTaskStatusAsync(Guid id, MaterialTransferStatus status)
        {
            var task = await _erpWorkshopMaterialTransferRepository.GetAsync(id);
            if (task == null)
            {
                throw new BusinessException("转移任务不存在");
            }

            // 验证状态转换的合法性
            ValidateStatusTransition(task.Status, status);

            task.UpdateStatus(status);
            return task;
        }

        /// <summary>
        /// 验证状态转换的合法性
        /// </summary>
        /// <param name="currentStatus">当前状态</param>
        /// <param name="newStatus">新状态</param>
        private void ValidateStatusTransition(MaterialTransferStatus currentStatus, MaterialTransferStatus newStatus)
        {
            var isValidTransition = currentStatus switch
            {
                MaterialTransferStatus.Pending => newStatus == MaterialTransferStatus.InProgress || newStatus == MaterialTransferStatus.Cancelled,
                MaterialTransferStatus.InProgress => newStatus == MaterialTransferStatus.Completed || newStatus == MaterialTransferStatus.Failed,
                MaterialTransferStatus.Completed => false, // 已完成状态不能更改
                MaterialTransferStatus.Cancelled => false, // 已取消状态不能更改
                MaterialTransferStatus.Failed => newStatus == MaterialTransferStatus.Pending, // 失败状态可以重新开始
                _ => false
            };

            if (!isValidTransition)
            {
                throw new BusinessException($"不允许从 {currentStatus} 状态转换到 {newStatus} 状态");
            }
        }
    }
}
