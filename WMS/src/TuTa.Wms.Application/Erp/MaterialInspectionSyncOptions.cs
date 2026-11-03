namespace TuTa.Wms.Erp;

/// <summary>Erp:MaterialInspectionSync配置；每轮从服务器当天00:00开始遍历U8全部分页，不限制总条数。</summary>
public class MaterialInspectionSyncOptions
{
    /// <summary>是否启用定时同步，默认启用；停用不影响已保存数据的查询。</summary>
    public bool Enabled { get; set; } = true;
    /// <summary>同步间隔，单位分钟，有效范围1至10080，默认60分钟。</summary>
    public int IntervalMinutes { get; set; } = 60;
}
