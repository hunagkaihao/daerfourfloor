using System.Collections.Generic;

namespace TuTa.Wms.AgvTasks.Dtos
{
    /// <summary>
    /// AGV任务分页查询结果
    /// </summary>
    public class AgvTaskPagedResultDto
    {
        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)((TotalCount + PageSize - 1) / PageSize);

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPreviousPage => PageIndex > 1;

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage => PageIndex < TotalPages;

        /// <summary>
        /// AGV任务列表
        /// </summary>
        public List<AgvTaskDto> Items { get; set; } = new List<AgvTaskDto>();
    }
}
