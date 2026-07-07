using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TuTa.Wms.AgvTasks.Aggregaes;
using TuTa.Wms.AgvTasks.Dto;
using Wms.HttpApiTool;

namespace TuTa.Wms.AgvTasks
{
    public class RcsApiManager:WmsDomainService
    {
        private readonly AGVOptions _aGVOptions;
        public string CMSServer { get; set; }
        public string AGVEnable { get; set; }

        public RcsApiManager(IOptionsSnapshot<AGVOptions> aGVOptions)
        {
            _aGVOptions = aGVOptions.Value;
            CMSServer = aGVOptions.Value.Server;
            AGVEnable = aGVOptions.Value.Enable;
        }
        /// <summary>
        /// 创建AGV任务
        /// </summary>
        /// <returns></returns>
        public async Task<ResultAgvTaskDto> CreateTaskAsync(string reqCode)
        {
            GenAgvTaskDto genAgvTaskDto = new GenAgvTaskDto(reqCode);
            var response =
              await HttpApiHelper.PostAsync<GenAgvTaskDto, ResultAgvTaskDto>("TTWMS",
              $"{CMSServer}/rcms/services/rest/hikRpcService/genAgvSchedulingTask", genAgvTaskDto);
            return response;
        }
        public async Task<ResultAgvTaskDto> CreateTaskAsync(string reqCode, string taskTyp, string[] userCallCodePath, string taskCode, string podCode)
        {
            if (AGVEnable.Equals("true"))
            {
                GenAgvTaskDto genAgvTaskDto = new GenAgvTaskDto(reqCode, taskTyp, userCallCodePath, taskCode, podCode);
                var response =
                  await HttpApiHelper.PostAsync<GenAgvTaskDto, ResultAgvTaskDto>("TTWMS",
                  $"{CMSServer}/rcms/services/rest/hikRpcService/genAgvSchedulingTask", genAgvTaskDto);
                return response;
            }
            else
            {
                return new ResultAgvTaskDto("0", "成功", reqCode, "");
            }

        }


        public async Task<ResultAgvTaskDto> CreateCTUPre(string position,string nextTask,string agvTyp)
        {
            if (AGVEnable.Equals("true"))
            {
                GenPreTaskDto genAgvTaskDto = new GenPreTaskDto(position,nextTask,agvTyp);
                var response =
                  await HttpApiHelper.PostAsync<GenPreTaskDto, ResultAgvTaskDto>("TTWMS",
                  $"{CMSServer}/rcms/services/rest/hikRpcService/genPreScheduleTask", genAgvTaskDto);
                return response;
            }
            else
            {
                return new ResultAgvTaskDto("0", "成功", "", "");
            }

        }


        /// <summary>
        /// 创建CTU任务
        /// </summary>
        /// <param name="reqCode"></param>
        /// <param name="taskTyp"></param>
        /// <param name="ctnrTyp"></param>
        /// <param name="userCallCodePath"></param>
        /// <param name="taskCode"></param>
        /// <returns></returns>
        public async Task<ResultAgvTaskDto> CreateCTUTaskAsync(string reqCode, string taskTyp, string ctnrTyp, string[] userCallCodePath, string taskCode, string boxCode)
        {
            if (AGVEnable.Equals("true"))
            {
                GenAgvTaskDto genAgvTaskDto = new GenAgvTaskDto(reqCode, taskTyp, ctnrTyp, taskCode, userCallCodePath, boxCode);
                var response =
                  await HttpApiHelper.PostAsync<GenAgvTaskDto, ResultAgvTaskDto>("TTWMS",
                  $"{CMSServer}/rcms/services/rest/hikRpcService/genAgvSchedulingTask", genAgvTaskDto);
                return response;
            }
            else
            {
                return new ResultAgvTaskDto("0", "成功", reqCode, "");
            }

        }


        /// <summary>
        /// 创建入库任务
        /// </summary>
        /// <param name="reqCode"></param>
        /// <param name="taskTyp"></param>
        /// <param name="ctnrTyp"></param>
        /// <param name="userCallCodePath"></param>
        /// <param name="taskCode"></param>
        /// <returns></returns>
        public async Task<ResultAgvTaskDto> CreateStockTaskAsync(string reqCode, string taskTyp, string ctnrTyp, string[] userCallCodePath, string taskCode, string boxCode,string podCode)
        {
            if (AGVEnable.Equals("true"))
            {
                GenAgvTaskDto genAgvTaskDto = new GenAgvTaskDto(reqCode, taskTyp, ctnrTyp, taskCode, userCallCodePath, boxCode, podCode);
                var response =
                  await HttpApiHelper.PostAsync<GenAgvTaskDto, ResultAgvTaskDto>("TTWMS",
                  $"{CMSServer}/rcms/services/rest/hikRpcService/genAgvSchedulingTask", genAgvTaskDto);
                return response;
            }
            else
            {
                return new ResultAgvTaskDto("0", "成功", reqCode, "");
            }

        }

        /*
        /// <summary>
        /// 继续执行AGV任务
        /// </summary>
        /// <returns></returns>
        public async Task<ResultAgvTaskDto> ContinueTaskAsync(string reqCode)
        {
            ContinueAgvTaskDto continueAgvTaskDto = new ContinueAgvTaskDto(reqCode);
            var response =
  await _httpClientFactory.PostAsync<ContinueAgvTaskDto, ResultAgvTaskDto>("TTWMS",
  $"{CMSServer}/rcms/services/rest/hikRpcService/continueTask", continueAgvTaskDto);
            Log.Information("继续执行AGV任务");
            return response;
        }
        */

        public async Task<ResultAgvTaskDto> CancelTaskAsync(string reqCode, string taskCode)
        {
            CancelAgvTaskDto cancelAgvTaskDto = new CancelAgvTaskDto(reqCode, taskCode);
            var response =
  await HttpApiHelper.PostAsync<CancelAgvTaskDto, ResultAgvTaskDto>("TTWMS",
  $"{CMSServer}/rcms/services/rest/hikRpcService/cancelTask", cancelAgvTaskDto);
            return response;
        }

        public async Task<ResultAgvTaskStatusDto> FindTaskSatusAsync(string reqCode,
      List<string> taskCodes)
        {
            // 通过access token 获取用户信息
            //Dictionary<string, string> headers = new Dictionary<string, string>
            //    { { "Authorization", $"Bearer {accessToken}" } };
            //调用AGV接口
            List<string> strs = new List<string>();
            strs.Add("123");
            GetAgvTaskStatusDto getAgvTaskStatusDto = new GetAgvTaskStatusDto(reqCode, taskCodes);
            //查询AGV任务状态接口
            var response =
  await HttpApiHelper.PostAsync<GetAgvTaskStatusDto, ResultAgvTaskStatusDto>("TTWMS",
  $"{CMSServer}/rcms/services/rest/hikRpcService/queryTaskStatus", getAgvTaskStatusDto);
            return response;

        }


        /*
        /// <summary>
        /// 查询货架\储位与物料批次绑定关系
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ResultAgvStatusDto> QueryAgvStatusAsync(string reqCode, string mapCode)
        {
            if (AGVEnable.Equals("true"))
            {
                QueryAgvStatusDto queryAgvStatusDto = new QueryAgvStatusDto(reqCode, mapCode);
                //查询AGV任务状态接口
                var response =
      await _httpClientFactory.PostAsync<QueryAgvStatusDto, ResultAgvStatusDto>("TTWMS",
      $"{CMSServer}/rcms-dps/rest/queryAgvStatus", queryAgvStatusDto);
                return response;
            }
            else
            {
                return new ResultAgvStatusDto("0", "成功", reqCode);
            }

        }
        */

        /// <summary>
        /// 容器与仓位绑定、解绑
        /// </summary>
        /// <param name="reqCode"></param>
        /// <param name="stgBinCode"></param>
        /// <param name="ctnrTyp"></param>
        /// <param name="ctnrCode"></param>
        /// <param name="ctnrNum"></param>
        /// <param name="indBind"></param>
        /// <returns></returns>
        public async Task<ResultAgvTaskDto> BindCtnrAndBinAsync(string reqCode, string stgBinCode, string ctnrTyp, string ctnrCode, string indBind)
        {
            if (AGVEnable.Equals("true"))
            {
                BindCtnrAndBinDto bindCtnrAndBinDto = new BindCtnrAndBinDto(reqCode, stgBinCode, ctnrTyp, ctnrCode, indBind);
                //查询AGV任务状态接口
                var response =
                      await HttpApiHelper.PostAsync<BindCtnrAndBinDto, ResultAgvTaskDto>("TTWMS",
                      $"{CMSServer}/rcms/services/rest/hikRpcService/bindCtnrAndBin", bindCtnrAndBinDto);
                return response;
            }
            else
            {
                return new ResultAgvTaskDto("0", "成功", reqCode, "");
            }

        }

        /// <summary>
        /// 解绑和绑定货架和仓位
        /// </summary>
        /// <param name="reqCode"></param>
        /// <param name="stgBinCode"></param>
        /// <param name="ctnrTyp"></param>
        /// <param name="ctnrCode"></param>
        /// <param name="ctnrNum"></param>
        /// <param name="indBind"></param>
        /// <returns></returns>
        public async Task<ResultAgvTaskDto> BindPodAndBerthAsync(string reqCode, string stgBinCode, string ctnrCode, string indBind, string podDir)
        {
            if (AGVEnable.Equals("true"))
            {
                BindPodAndBerthDto bindPodAndBerthDto = new BindPodAndBerthDto(reqCode, stgBinCode, ctnrCode, indBind, podDir);
                //查询AGV任务状态接口
                var response =
      await HttpApiHelper.PostAsync<BindPodAndBerthDto, ResultAgvTaskDto>("TTWMS",
      $"{CMSServer}/rcms/services/rest/hikRpcService/bindPodAndBerth", bindPodAndBerthDto);
                return response;
            }
            else
            {
                return new ResultAgvTaskDto("0", "成功", reqCode, "");
            }

        }



    }
}
