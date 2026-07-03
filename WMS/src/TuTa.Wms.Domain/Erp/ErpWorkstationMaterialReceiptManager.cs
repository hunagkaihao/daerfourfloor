using System;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Repositories;
using TuTa.Wms.Erp.Aggregates;
using Volo.Abp.Domain.Services;
using Volo.Abp;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP工位收料领域服务
    /// </summary>
    public class ErpWorkstationMaterialReceiptManager : DomainService
    {
        private readonly IErpWorkstationMaterialReceiptRepository _erpWorkstationMaterialReceiptRepository;

        public ErpWorkstationMaterialReceiptManager(IErpWorkstationMaterialReceiptRepository erpWorkstationMaterialReceiptRepository)
        {
            _erpWorkstationMaterialReceiptRepository = erpWorkstationMaterialReceiptRepository;
        }

        /// <summary>
        /// 创建工位收料记录
        /// </summary>
        /// <param name="sortingBatch">分拣批次号</param>
        /// <param name="receiptTime">收料时间</param>
        /// <returns>工位收料记录</returns>
        public async Task<ErpWorkstationMaterialReceipt> CreateMaterialReceiptAsync(string sortingBatch, DateTime receiptTime)
        {
            // 检查分拣批次是否已收料
            var existingReceipt = await _erpWorkstationMaterialReceiptRepository.FindBySortingBatchAsync(sortingBatch);
            if (existingReceipt != null)
            {
                throw new BusinessException("该分拣批次已收料，不能重复创建");
            }

            // 验证收料时间不能是未来时间（允许1分钟的时间差）
            if (receiptTime > DateTime.Now.AddMinutes(1))
            {
                throw new BusinessException("收料时间不能是未来时间");
            }

            // 创建收料记录
            var materialReceipt = ErpWorkstationMaterialReceipt.Create(sortingBatch, receiptTime);
            
            return materialReceipt;
        }
    }
}
