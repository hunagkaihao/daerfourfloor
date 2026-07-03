using System;
using System.Threading.Tasks;
using TuTa.Wms.Warehouses.Aggregates;

namespace TuTa.Wms.Warehouses
{
    public class WarehouseManager : WmsDomainService
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseManager(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<Warehouse> CreateWarehouseAsync(
            string warehouseCode,
            string warehouseName,
            WarehouseType warehouseType,
            string warehouseRemark = null,
            string warehouseFlag = null,
            string warehouseOrder = null)
        {
            var houseExist = await _warehouseRepository.FindByNameAsync(warehouseName).ConfigureAwait(false);
            if (houseExist != null)
                throw new Exception($"仓库名为{warehouseName}的仓库已经存在");

            houseExist = await _warehouseRepository.FindByCodeAsync(warehouseCode).ConfigureAwait(false);
            if (houseExist != null)
                throw new Exception($"仓库编号为{warehouseCode}的仓库已经存在");

            Warehouse house = new Warehouse(
                GuidGenerator.Create(), 
                warehouseCode, 
                warehouseName, 
                warehouseType, 
                warehouseRemark,
                warehouseFlag,
                warehouseOrder);

            return house;
        }

        public async Task UpdateWarehouseAsync(
            Warehouse warehouse,
            string warehouseCodeNew,
            string warehouseNameNew,
            WarehouseType warehouseTypeNew,
            string warehouseRemarkNew,
            string warehouseFlagNew,
            string warehouseOrderNew)
        {
            if (warehouse.WarehouseName != warehouseNameNew)
            {
                var houseExist = await _warehouseRepository.FindByNameAsync(warehouseNameNew).ConfigureAwait(false);
                if (houseExist != null)
                    throw new Exception($"仓库名为{warehouseNameNew}的仓库已经存在");
            }

            if (warehouse.WarehouseCode != warehouseCodeNew)
            {
                var houseExist = await _warehouseRepository.FindByCodeAsync(warehouseCodeNew).ConfigureAwait(false);
                if (houseExist != null)
                    throw new Exception($"仓库编号为{warehouseCodeNew}的仓库已经存在");
            }
            
            warehouse.Update(warehouseCodeNew, warehouseNameNew, warehouseTypeNew,
                warehouseRemarkNew, warehouseFlagNew, warehouseOrderNew);
        }
    }
}
