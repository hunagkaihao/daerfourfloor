using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Boxes.ValueObjects;
using TuTa.Wms.Materials;
using TuTa.Wms.Materials.Aggregates;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;

namespace TuTa.Wms.Boxes
{
    public class BoxManager : WmsDomainService
    {
        private readonly IBoxRepository _boxRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IMaterialRepository _materialRepository;

        public BoxManager(IBoxRepository boxRepository,
            IStockRepository stockRepository,
            IMaterialRepository materialRepository)
        {
            _boxRepository = boxRepository;
            _stockRepository = stockRepository;
            _materialRepository = materialRepository;
        }

        public async Task<Box> CreateBoxAsync(string boxCode, string boxName, string boxTypeName, BoxSpecsValObj boxSpecs)
        {
            Box box = new Box(GuidGenerator.Create(), boxCode, boxName, boxTypeName, boxSpecs);

            var boxExist = await _boxRepository.FindByBoxCodeAsync(box.BoxCode, false, false).ConfigureAwait(false);
            if (boxExist != null)
                throw new Exception($"容器码为{box.BoxCode}的容器已经存在");

            boxExist = await _boxRepository.FindByBoxNameAsync(box.BoxName, false, false).ConfigureAwait(false);
            if (boxExist != null)
                throw new Exception($"容器名为{box.BoxName}的容器已经存在");

            return box;
        }

        public async Task ModifyBoxAsync(Box boxToModify, string codeNew, string nameNew, string boxTypeNameNew, BoxSpecsValObj boxSpecsNew)
        {
            if (boxToModify.BoxCode != codeNew)
            {
                var boxExist = await _boxRepository.FindByBoxCodeAsync(codeNew, false, false).ConfigureAwait(false);
                if (boxExist != null)
                    throw new Exception($"容器码为{codeNew}的容器已经存在");
            }

            if (boxToModify.BoxName != nameNew)
            {
                var boxExist = await _boxRepository.FindByBoxNameAsync(nameNew, false, false).ConfigureAwait(false);
                if (boxExist != null)
                    throw new Exception($"容器名为{nameNew}的容器已经存在");
            }

            boxToModify.ModifyBox(codeNew, nameNew, boxTypeNameNew, boxSpecsNew);            
        }
    }
}
