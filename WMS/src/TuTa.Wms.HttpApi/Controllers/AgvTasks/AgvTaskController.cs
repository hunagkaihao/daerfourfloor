using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TuTa.Wms.AgvTasks;
using TuTa.Wms.AgvTasks.Dtos;
using TuTa.Wms.BarcodeLists;
using TuTa.Wms.BarcodeLists.Dtos;

namespace TuTa.Wms.Controllers.AgvTasks
{
    [Route("wms/agvtask")]
    [ApiController]
    public class AgvTaskController : WmsController,IAgvTaskService
    {
        private IAgvTaskService _agvTaskService;

        public AgvTaskController(IAgvTaskService agvTaskService)
        {
            _agvTaskService = agvTaskService;
        }

        [HttpPost("callback")]
        [SwaggerOperation(summary: "CTU回调", Tags = new[] { "AgvTask" })]
        public async Task<ResultAgvTaskDto> CtuCallbackAsync(AgvCallBackRequest input)
        {
            return await _agvTaskService.CtuCallbackAsync(input);
        }
        [HttpPost("paged-list")]
        [SwaggerOperation(summary: "分页获取AGV任务列表", Tags = new[] { "AgvTask" })]
        public async Task<AgvTaskPagedResultDto> GetPagedListAsync(AgvTaskPagedQueryDto input)
        {
            return await _agvTaskService.GetPagedListAsync(input);
        }
    }
}
