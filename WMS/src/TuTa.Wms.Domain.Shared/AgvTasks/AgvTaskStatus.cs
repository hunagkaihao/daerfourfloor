using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.AgvTasks
{
    public enum AgvTaskStatus
    {
        /// <summary>
        /// 被创建
        /// </summary>
        Created = 0,
        /// <summary>
        /// 等待执行
        /// </summary>
        WaitingExecuting = 1,
        /// <summary>
        /// 执行中
        /// </summary>
        Executing = 2,
        /// <summary>
        /// 任务开始
        /// </summary>
        TaskStart = 3,
        /// <summary>
        /// 任务开始
        /// </summary>
        CellOut = 4,
        /// <summary>
        /// 等待任务继续
        /// </summary>
        WaitingContinue,
        /// <summary>
        /// 等待继续任务响应
        /// </summary>
        WaitingContinueResponse,
        /// <summary>
        /// 继续执行
        /// </summary>
        ContinueExecuting,
        /// <summary>
        /// 等待取消响应
        /// </summary>
        WaitingCancelResponse,
        /// <summary>
        /// 任务完成
        /// </summary>
        Complete = 9,
        /// <summary>
        /// 调度删除任务
        /// </summary>
        Cancel,
        /// <summary>
        /// 设备错误
        /// </summary>
        Error,
        /// <summary>
        /// 异常完成
        /// </summary>
        ExceptionComplete,
    }
}
