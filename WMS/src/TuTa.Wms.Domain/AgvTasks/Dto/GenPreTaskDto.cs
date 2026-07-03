using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.AgvTasks.Dto
{
    public class GenPreTaskDto
    {

        public GenPreTaskDto(string reqCode, string positionCode, string nextTask, string agvTyp)
        {
            this.reqCode = reqCode;
            this.positionCode = positionCode;
            this.nextTask = nextTask;
            this.agvTyp = agvTyp;
        }

        public GenPreTaskDto(string positionCode, string nextTask, string agvTyp)
        {
            this.reqCode = Guid.NewGuid().ToString("N");
            this.positionCode = positionCode;
            this.nextTask = nextTask;
            this.agvTyp = agvTyp;
            this.useableLayers = "1";
            this.cacheCount = "1";
            this.update = 1;
            this.priority = "1";
        }

        /// <summary>
        /// 任务请求编号，唯一
        /// </summary>
        public string reqCode { get; set; }

        /// <summary>
        /// 任务点位呼叫号
        /// </summary>
        public string positionCode { get; set; }

        /// <summary>
        ///     预调度时间（s）表示真实任务预计多久后生成，传-1清空点位全部的预调度任务
        /// </summary>
        public string nextTask { get; set; }

        /// <summary>
        /// AGV 类型，预调度需要指定车型
        /// </summary>
        public string agvTyp { get; set; }


        /// <summary>
        /// 优先级
        /// </summary>
        public string priority { get; set; }
        /// <summary>
        /// 需求空仓位数
        /// </summary>
        public string useableLayers { get; set; }

        /// <summary>
        /// 缓存料箱数
        /// 
        /// </summary>
        public string cacheCount { get; set; }

        /// <summary>
        /// 是否更新，默认为 0
        /// 0：不更新之前的预调度任务，新增一个预调度任务
        /// 1：更新预调度任务，根据缓存料箱数与需求空仓位数计算需要的预调度任务数
        /// </summary>
        public int update { get; set; }
    }
}
