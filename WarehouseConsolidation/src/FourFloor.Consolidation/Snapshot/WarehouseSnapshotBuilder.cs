using FourFloor.Consolidation.Clients;
using FourFloor.Consolidation.Models.Planning;

namespace FourFloor.Consolidation.Snapshot;

public sealed class WarehouseSnapshotBuilder(
    WmsStockClient stockClient,
    WmsCellClient cellClient,
    WmsAgvTaskClient agvTaskClient)
{
    public async Task<WarehouseSnapshot> BuildAsync(CancellationToken cancellationToken)
    {
        var stocksTask = stockClient.GetStocksAsync(cancellationToken);
        var cellsTask = cellClient.GetAllCellsAsync(cancellationToken);
        var activeTasksTask = agvTaskClient.GetActiveTasksAsync(cancellationToken);
        await Task.WhenAll(stocksTask, cellsTask, activeTasksTask);

        var stocks = await stocksTask;
        var cells = await cellsTask;
        var activeTasks = await activeTasksTask;
        var activeBoxes = activeTasks
            .Select(task => task.BoxCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var pallets = stocks
            .Where(stock => stock.Id != Guid.Empty)
            .Where(stock => !string.IsNullOrWhiteSpace(stock.BoxCode))
            .Where(stock => !string.IsNullOrWhiteSpace(stock.CellCode))
            .GroupBy(stock => stock.BoxCode!, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var orderedStocks = group.ToList();
                var stockIds = orderedStocks.Select(stock => stock.Id).Distinct().Order().ToList();
                var barcodes = orderedStocks
                    .Select(stock => stock.Barcode)
                    .Where(barcode => !string.IsNullOrWhiteSpace(barcode))
                    .Select(barcode => barcode!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var cellCodes = orderedStocks
                    .Select(stock => stock.CellCode)
                    .Where(code => !string.IsNullOrWhiteSpace(code))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (barcodes.Count == 0 || cellCodes.Count != 1)
                {
                    return null;
                }

                return new PalletSnapshot
                {
                    PalletKey = string.Join("-", stockIds.Select(id => id.ToString("N"))),
                    CurrentBoxCode = group.Key,
                    CurrentCellCode = cellCodes[0]!,
                    StockIds = stockIds,
                    Barcodes = barcodes,
                    GroupBarcode = barcodes[0],
                    MaterialCode = orderedStocks.Select(stock => stock.MaterialCode).FirstOrDefault(code => !string.IsNullOrWhiteSpace(code)),
                    HasActiveTask = orderedStocks.Any(stock => stock.HasTask) || activeBoxes.Contains(group.Key)
                };
            })
            .Where(pallet => pallet is not null)
            .Cast<PalletSnapshot>()
            .ToDictionary(pallet => pallet.PalletKey, StringComparer.OrdinalIgnoreCase);

        var duplicateCells = pallets.Values
            .GroupBy(pallet => pallet.CurrentCellCode, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        if (duplicateCells.Count > 0)
        {
            throw new InvalidOperationException($"以下库位存在多个托盘快照：{string.Join("、", duplicateCells)}");
        }

        var palletByCell = pallets.Values.ToDictionary(
            pallet => pallet.CurrentCellCode,
            pallet => pallet.PalletKey,
            StringComparer.OrdinalIgnoreCase);

        var cellStates = cells
            .Where(cell => !string.IsNullOrWhiteSpace(cell.CellCode))
            .GroupBy(cell => cell.CellCode!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var cell = group.First();
                    palletByCell.TryGetValue(group.Key, out var palletKey);
                    return new CellState
                    {
                        CellCode = group.Key,
                        CellStatus = cell.CellStatus ?? string.Empty,
                        RunStatus = cell.RunStatus ?? string.Empty,
                        CellType = cell.CellType,
                        PalletKey = palletKey
                    };
                },
                StringComparer.OrdinalIgnoreCase);

        var palletKeyByStockId = pallets.Values
            .SelectMany(pallet => pallet.StockIds.Select(stockId => (stockId, pallet.PalletKey)))
            .ToDictionary(item => item.stockId, item => item.PalletKey);

        return new WarehouseSnapshot
        {
            Cells = cellStates,
            Pallets = pallets,
            PalletKeyByStockId = palletKeyByStockId
        };
    }
}
