using System;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Repositories;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP物料管理器
    /// </summary>
    public class ErpMaterialManager : WmsDomainService
    {
        private readonly IErpMaterialRepository _erpMaterialRepository;

        public ErpMaterialManager(
            IErpMaterialRepository erpMaterialRepository)
        {
            _erpMaterialRepository = erpMaterialRepository;
        }

        /// <summary>
        /// 创建或更新ERP物料
        /// </summary>
        public async Task<ErpMaterial> CreateOrUpdateMaterialAsync(
            string materialCode,
            string materialName,
            string unitCode,
            long syncTimeStamp,
            string syncType)
        {
            // 检查物料代号是否已存在
            var existingMaterial = await _erpMaterialRepository.FindByMaterialCodeAsync(materialCode);
            
            if (existingMaterial != null)
            {
                // 更新现有物料
                existingMaterial.Update(materialName, unitCode, syncTimeStamp, syncType);
                return existingMaterial;
            }
            else
            {
                // 创建新物料
                var material = new ErpMaterial(
                    GuidGenerator.Create(),
                    materialCode,
                    materialName,
                    unitCode,
                    syncTimeStamp,
                    syncType);

                return material;
            }
        }

        /// <summary>
        /// 验证物料数据
        /// </summary>
        public void ValidateMaterialData(
            string materialCode,
            string materialName,
            string unitCode,
            string syncType)
        {
            if (string.IsNullOrWhiteSpace(materialCode))
                throw new Exception("材料代号不能为空");

            if (string.IsNullOrWhiteSpace(materialName))
                throw new Exception("材料名称不能为空");

            if (string.IsNullOrWhiteSpace(unitCode))
                throw new Exception("单位不能为空");

            if (string.IsNullOrWhiteSpace(syncType))
                throw new Exception("操作类型不能为空");

            // 验证操作类型
            if (syncType != "insert" && syncType != "modify" && syncType != "delete")
                throw new Exception("操作类型必须是 insert、modify 或 delete");
        }
    }
}
