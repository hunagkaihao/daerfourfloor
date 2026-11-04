using System;
using Newtonsoft.Json;
using TuTa.Wms.AgvTasks.Aggregaes;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Stocks.Aggregates;
using Volo.Abp.Data;

namespace TuTa.Wms.StockOutHistories;

/// <summary>已完成出库搬运的不可变位置快照；持久化为库存扩展字段，实际分批出库时复用，不提前记出库数量。</summary>
public class StockOutHistoryRoute
{
    /// <summary>库存扩展字段键；只由真实完成回调写入，不依据物料或容器猜测历史任务。</summary>
    private const string PropertyKey = "StockOutHistoryRoute";

    /// <summary>本条实际出库对应的库存标识；旧记录或多来源汇总时为空。</summary>
    public Guid? StockId { get; set; }

    /// <summary>关联的已完成AGV任务主键；直接出库及旧记录为空。</summary>
    public int? AgvTaskId { get; set; }

    /// <summary>已完成出库搬运任务的请求编号快照。</summary>
    public string AgvTaskReqCode { get; set; }

    /// <summary>实际出库来源库位编号快照；未知来源为空。</summary>
    public string StartCellCode { get; set; }

    /// <summary>实际出库来源库位名称快照。</summary>
    public string StartCellName { get; set; }

    /// <summary>实际出库来源容器编号快照。</summary>
    public string StartBoxCode { get; set; }

    /// <summary>实际出库来源容器名称快照。</summary>
    public string StartBoxName { get; set; }

    /// <summary>关联搬运的目标库位编号快照；无明确目标时为空。</summary>
    public string EndCellCode { get; set; }

    /// <summary>目标库位名称快照。</summary>
    public string EndCellName { get; set; }

    /// <summary>目标容器编号快照；无实体容器时为空。</summary>
    public string EndBoxCode { get; set; }

    /// <summary>目标容器名称快照。</summary>
    public string EndBoxName { get; set; }

    /// <summary>捕获本次真实起终点及固定容器信息；输送线终点没有实体容器时允许为空。</summary>
    public static StockOutHistoryRoute Capture(Stock stock, AgvTask task, Cell start, Box startBox, Cell end, Box endBox)
    {
        return new StockOutHistoryRoute {
            StockId = stock.Id, AgvTaskId = task.Id, AgvTaskReqCode = task.ReqCode,
            StartCellCode = start.CellCode, StartCellName = start.CellName,
            StartBoxCode = startBox.BoxCode, StartBoxName = startBox.BoxName,
            EndCellCode = end.CellCode, EndCellName = end.CellName,
            EndBoxCode = endBox?.BoxCode, EndBoxName = endBox?.BoxName,
        };
    }

    /// <summary>仅在搬运成功后保存路线；不修改库存数量、出库单或ERP状态。</summary>
    public void SaveTo(Stock stock)
    {
        stock.SetProperty(PropertyKey, JsonConvert.SerializeObject(this));
    }

    /// <summary>下一次实际搬运前清除旧路线，防止回库或移库后再次到达同一点位时误关联。</summary>
    public static void ClearFrom(Stock stock) => stock.ExtraProperties.Remove(PropertyKey);

    /// <summary>读取与当前库存及落位一致的路线；回库、移库后的位置不匹配时不关联旧任务。</summary>
    public static StockOutHistoryRoute ReadFrom(Stock stock)
    {
        var json = stock.GetProperty<string>(PropertyKey);
        if (string.IsNullOrWhiteSpace(json)) return null;
        var route = JsonConvert.DeserializeObject<StockOutHistoryRoute>(json);
        if (route == null || route.StockId != stock.Id || !route.AgvTaskId.HasValue
            || route.EndCellCode != stock.CellData?.CellCode || route.EndBoxCode != stock.BoxData?.BoxCode)
            return null;
        return route;
    }
}

