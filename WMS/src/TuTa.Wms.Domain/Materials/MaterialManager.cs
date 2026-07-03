using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using TuTa.Wms.Materials.Aggregates;

namespace TuTa.Wms.Materials
{
    public class MaterialManager : WmsDomainService
    {
        private readonly IMaterialRepository _materialRepository;

        public MaterialManager(IMaterialRepository materialRepository)
        {
            _materialRepository = materialRepository;
        }

        public async Task<Material> CreateMaterialAsync(
            string materialCode,
            string materialName,
            string specs,
            string unit,
            string typeCode,
            string typeName,
            string isHB,
            decimal? safetyStock,
            decimal? fullBoxCount,
            int? expiryDate,
            bool? isQCPJ,
            bool? isPPAP,
            decimal? count,
            decimal? weight,
            string bindType,
            bool isBind,
            string finGoodsList)
        {
            Material material = new Material(materialCode, materialName, specs, unit, typeCode, typeName, isHB
                , safetyStock, fullBoxCount, expiryDate, isQCPJ, isPPAP,count,weight,bindType,isBind,finGoodsList);
            var materialExist = await _materialRepository.FindByMaterialCodeAsync(materialCode).ConfigureAwait(false);
            if (materialExist != null)
            {
                throw new Exception($"物料码为{materialCode}的物料定义已经存在");
            }

            //名称+规格是可以重复的
            //materialExist = await _materialRepository.FindByMaterialNameAndSpecsAsync(materialName, specs).ConfigureAwait(false);
            //if (materialExist != null)
            //{
            //    throw new Exception($"物料名为{materialName}，规格为{specs}的物料定义已经存在");
            //}

            return material;
        }

        public async Task ModifyMaterialAsync(
            Material materialToModify,
            string materialCode,
            string materialName,
            string specs,
            string unit,
            string typeCode,
            string typeName,
            string isHB,
            decimal? safetyStock,
            decimal? fullBoxCount,
            int? expiryDate,
            bool? isQCPJ,
            bool? isPPAP,
            decimal? count,
            decimal? weight,
            string bindType,
            bool isBind,
            string finGoodsList)
        {
            if (materialToModify.MaterialCode != materialCode) //materialCode唯一性判断
            {
                Material materialExist = await _materialRepository.FindByMaterialCodeAsync(materialCode).ConfigureAwait(false);
                if (materialExist != null)
                {
                    throw new Exception($"物料码为{materialCode}的物料定义已经存在");
                }
            }

            //名称+规格是可以重复的
            //if (materialToModify.MaterialName != materialName || materialToModify.Specs != specs)
            //{
            //    Material materialExist = await _materialRepository.FindByMaterialNameAndSpecsAsync(materialName, specs).ConfigureAwait(false);
            //    if (materialExist != null)
            //    {
            //        throw new Exception($"物料名为{materialName}，规格为{specs}的物料定义已经存在");
            //    }
            //}

            materialToModify.ModifyGoodsDefine(materialCode, materialName, specs, unit, typeCode, typeName, isHB,
                safetyStock, fullBoxCount, expiryDate, isQCPJ, isPPAP, count, weight, bindType, isBind, finGoodsList);
        }
    }
}
