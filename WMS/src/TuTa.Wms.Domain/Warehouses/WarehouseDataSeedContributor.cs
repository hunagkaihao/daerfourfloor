using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuTa.Wms.Warehouses.Aggregates;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace TuTa.Wms.Warehouses
{
    public class WarehouseDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly WarehouseManager _warehouseManager;

        public WarehouseDataSeedContributor(
            IWarehouseRepository warehouseRepository,
            WarehouseManager warehouseManager)
        {
            _warehouseRepository = warehouseRepository;
            _warehouseManager = warehouseManager;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            try
            {
                Warehouse warehouse = await _warehouseManager.CreateWarehouseAsync("01", "山遇风综合库", WarehouseType.PK);
                warehouse.AddArea("001", "正常区", "存放合格品区");
                warehouse.AddArea("002", "待处理区", "存放不合格品");
                warehouse.AddArea("003", "暂存区", "暂存区");
                await _warehouseRepository.InsertAsync(warehouse).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }
    }
}
