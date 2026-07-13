using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Dto;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Erp.IDto;
using Volo.Abp;
using Volo.Abp.Application.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TuTa.Wms.Erp
{
    public class ErpSettings
    {
        public string LoginUrl { get; set; }
        public string GetAsnUrl { get; set; }
        public string GetAsnListUrl { get; set; }
        public string PushReceiptUrl { get; set; }
        public string PuArrVouchAddUrl { get; set; }
        public string U8ApiUrl { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
    }

    /// <summary>
    /// ERP ASN应用服务
    /// </summary>
    public class ErpAsnAppService : ApplicationService, IErpAsnAppService
    {
        private readonly ILogger<ErpAsnAppService> _logger;
        private readonly IErpAsnRepository _erpAsnRepository;
        private readonly ErpSettings _erpSettings;

        public ErpAsnAppService(IErpAsnRepository erpAsnRepository, ILogger<ErpAsnAppService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _erpAsnRepository = erpAsnRepository;
            _erpSettings = configuration.GetSection("Erp").Get<ErpSettings>();
        }

        /// <summary>
        /// 通过ASN码获取信息
        /// </summary>
        public async Task<ErpAsnValidateResponseDto> GetAsnInfoAsync(string asnCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(asnCode))
                {
                    return new ErpAsnValidateResponseDto
                    {
                        Success = false,
                        Message = "ASN码不能为空"
                    };
                }

                _logger.LogInformation($"开始获取ASN信息，ASN码：{asnCode}");

                _logger.LogInformation($"开始登录ERP获取token，登录地址：{_erpSettings.LoginUrl}");
                string token;
                using (var loginClient = new HttpClient())
                {
                    var loginData = new
                    {
                        appKey = _erpSettings.AppKey,
                        appSecret = _erpSettings.AppSecret
                    };

                    var loginContent = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(loginData),
                        Encoding.UTF8,
                        "application/json");

                    _logger.LogInformation($"请求ERP登录接口，URL：{_erpSettings.LoginUrl}，请求参数：{System.Text.Json.JsonSerializer.Serialize(loginData)}");
                    var loginResponse = await loginClient.PostAsync(_erpSettings.LoginUrl, loginContent);
                    loginResponse.EnsureSuccessStatusCode();

                    var loginResult = await loginResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation($"ERP登录响应: {loginResult}");
                    
                    var loginObj = System.Text.Json.JsonSerializer.Deserialize<ErpLoginResponse>(loginResult);

                    if (loginObj == null)
                    {
                        _logger.LogError($"登录ERP失败: 响应解析为空");
                        return new ErpAsnValidateResponseDto
                        {
                            Success = false,
                            Message = "登录ERP失败：响应解析为空"
                        };
                    }

                    if (!loginObj.success)
                    {
                        _logger.LogError($"登录ERP失败: {loginObj.data?.token ?? "未知错误"}");
                        return new ErpAsnValidateResponseDto
                        {
                            Success = false,
                            Message = $"登录ERP失败：{loginObj.data?.token ?? "未知错误"}"
                        };
                    }

                    if (string.IsNullOrEmpty(loginObj.data?.token))
                    {
                        _logger.LogError($"登录ERP失败: token为空");
                        return new ErpAsnValidateResponseDto
                        {
                            Success = false,
                            Message = "登录ERP失败：未获取到token"
                        };
                    }

                    token = loginObj.data.token;
                    _logger.LogInformation($"登录ERP成功，获取到token");
                }

                _logger.LogInformation($"使用token请求ASN信息，ASN码：{asnCode}，请求地址：{_erpSettings.GetAsnUrl}");
                using var getInfoClient = new HttpClient();
                getInfoClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                getInfoClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestData = new { ccode = asnCode };
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNameCaseInsensitive = true
                };

                var pushDataJson = System.Text.Json.JsonSerializer.Serialize(requestData, options);
                _logger.LogInformation($"请求ERP获取ASN信息接口，URL：{_erpSettings.GetAsnUrl}，请求参数：{pushDataJson}");

                var getContent = new StringContent(pushDataJson, Encoding.UTF8, "application/json");
                var httpRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(_erpSettings.GetAsnUrl),
                    Content = getContent
                };

                var response = await getInfoClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(responseBody);
                var dataArray = jObject["data"] as JArray;

                if (dataArray != null && dataArray.Count > 0)
                {
                    var items = dataArray
                        .Select(item => MapToErpAsnDto(item))
                        .ToList();

                    return new ErpAsnValidateResponseDto
                    {
                        Success = true,
                        Message = "获取ASN信息成功",
                        Data = items
                    };
                }
                else
                {
                    return new ErpAsnValidateResponseDto
                    {
                        Success = false,
                        Message = "未找到该ASN码对应的信息"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"获取ASN信息异常: {ex.Message}");
                return new ErpAsnValidateResponseDto
                {
                    Success = false,
                    Message = $"获取ASN信息异常：{ex.Message}"
                };
            }
        }

        /// <summary>
        /// ERP登录响应
        /// </summary>
        private class ErpLoginResponse
        {
            public bool success { get; set; }
            public ErpLoginData data { get; set; }
        }

        /// <summary>
        /// ERP登录数据
        /// </summary>
        private class ErpLoginData
        {
            public string token { get; set; }
        }

        /// <summary>
        /// 保存ASN信息到数据库
        /// </summary>
        public async Task<ErpAsnSaveResponseDto> SaveAsnAsync(string asnCode)
        {
            var operationId = Guid.NewGuid().ToString();
            _logger.LogInformation($"[操作ID: {operationId}] 开始保存ASN信息，ASN码：{asnCode}");
            
            try
            {
                if (string.IsNullOrWhiteSpace(asnCode))
                {
                    _logger.LogWarning($"[操作ID: {operationId}] ASN码不能为空");
                    return new ErpAsnSaveResponseDto
                    {
                        Success = false,
                        Message = "ASN码不能为空"
                    };
                }

                _logger.LogInformation($"[操作ID: {operationId}] 从ERP获取ASN信息：{asnCode}");
                var asnInfoList = await GetAsnInfoListFromErp(asnCode, operationId);
                
                if (asnInfoList == null || asnInfoList.Count == 0)
                {
                    _logger.LogWarning($"[操作ID: {operationId}] 未找到该ASN码对应的信息：{asnCode}");
                    return new ErpAsnSaveResponseDto
                    {
                        Success = false,
                        Message = "未找到该ASN码对应的信息"
                    };
                }

                _logger.LogInformation($"[操作ID: {operationId}] 检查ASN是否已存在：{asnCode}");
                var existingAsns = await _erpAsnRepository.GetListByAsnCodeAsync(asnCode);
                var savedIds = new List<Guid>();
                var newCount = 0;

                foreach (var asnInfo in asnInfoList)
                {
                    var existingAsn = FindExistingAsn(existingAsns, asnInfo);
                    if (existingAsn != null)
                    {
                        savedIds.Add(existingAsn.Id);
                        continue;
                    }

                    _logger.LogInformation($"[操作ID: {operationId}] 开始创建ASN明细，ASN码：{asnCode}，物料：{asnInfo.Cinvcode}");
                    var newAsn = CreateErpAsnFromDto(asnInfo);
                    await _erpAsnRepository.InsertAsync(newAsn);
                    existingAsns.Add(newAsn);
                    savedIds.Add(newAsn.Id);
                    newCount++;
                    _logger.LogInformation($"[操作ID: {operationId}] 成功保存ASN明细，ASN码：{asnCode}，ID：{newAsn.Id}");
                }

                var message = newCount == 0
                    ? "ASN信息已存在"
                    : newCount == asnInfoList.Count
                        ? "保存ASN信息成功"
                        : $"保存ASN信息成功，新增{newCount}条明细";

                return new ErpAsnSaveResponseDto
                {
                    Success = true,
                    Message = message,
                    Data = savedIds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[操作ID: {operationId}] 保存ASN信息异常：{asnCode}");
                return new ErpAsnSaveResponseDto
                {
                    Success = false,
                    Message = $"保存ASN信息异常：{ex.Message}"
                };
            }
        }

        private static ErpAsn FindExistingAsn(List<ErpAsn> existingAsns, ErpAsnDto asnInfo)
        {
            return existingAsns.FirstOrDefault(x =>
                (asnInfo.Iposid > 0 && x.PoDetailId == asnInfo.Iposid) ||
                (TryGetErpLineId(asnInfo, out var lineId) && x.ErpOrderDetailId == lineId));
        }

        /// <summary>
        /// 获取 ERP 明细行唯一标识。优先 autoid（行ID），id 常为表头ID不能用于明细匹配。
        /// </summary>
        private static bool TryGetErpLineId(ErpAsnDto asnInfo, out long lineId)
        {
            if (long.TryParse(asnInfo.Autoid, out lineId) && lineId > 0)
            {
                return true;
            }

            if (asnInfo.Id > 0)
            {
                lineId = asnInfo.Id;
                return true;
            }

            lineId = 0;
            return false;
        }

        private ErpAsn CreateErpAsnFromDto(ErpAsnDto asnInfo)
        {
            TryGetErpLineId(asnInfo, out var erpLineId);
            var newAsn = ErpAsn.Create(
                GuidGenerator.Create(),
                asnInfo.Ccode,
                asnInfo.Cordercode,
                asnInfo.Cvencode,
                asnInfo.Cvenabbname,
                asnInfo.Cwhcode,
                asnInfo.Cwhname,
                asnInfo.Cinvcode,
                asnInfo.Cinvname,
                asnInfo.Cinvstd,
                asnInfo.Cinfvm_unit,
                asnInfo.Ipoquantity,
                asnInfo.Cbatch,
                string.IsNullOrEmpty(asnInfo.Darridate) ? (DateTime?)null : DateTime.Parse(asnInfo.Darridate),
                asnInfo.Iasnflag,
                asnInfo.Cbustype,
                asnInfo.Cptcode,
                asnInfo.Cptname,
                string.IsNullOrEmpty(asnInfo.Dshipdate) ? (DateTime?)null : DateTime.Parse(asnInfo.Dshipdate),
                asnInfo.Cdepcode,
                asnInfo.Cdepname,
                asnInfo.Cpersoncode,
                asnInfo.Cpersonname,
                asnInfo.CexchName,
                asnInfo.Cmemo,
                asnInfo.Cmaker,
                string.IsNullOrEmpty(asnInfo.Ddate) ? (DateTime?)null : DateTime.Parse(asnInfo.Ddate),
                asnInfo.Headcmemo,
                string.IsNullOrEmpty(asnInfo.Darridateb) ? (DateTime?)null : DateTime.Parse(asnInfo.Darridateb),
                asnInfo.Cmaketime,
                asnInfo.Itaxrateb,
                asnInfo.Iexchrate,
                asnInfo.Iposid > 0 ? (long?)asnInfo.Iposid : null,
                erpLineId > 0 ? (long?)erpLineId : null,
                asnInfo.Bgsp == 1,
                asnInfo.Ccloser,
                asnInfo.Cfree2,
                asnInfo.Cfree3,
                asnInfo.Cfree5,
                asnInfo.Cinvaddcode,
                asnInfo.Wdhsl);

            newAsn.UpdateQuantity(
                asnInfo.Farrqty,
                asnInfo.Foutquantity,
                asnInfo.Iquantity,
                asnInfo.Frealquantity);

            return newAsn;
        }

        /// <summary>
        /// 从ERP获取ASN明细列表
        /// </summary>
        private async Task<List<ErpAsnDto>> GetAsnInfoListFromErp(string asnCode, string operationId)
        {
            _logger.LogInformation($"[操作ID: {operationId}] 开始从ERP获取ASN信息，ASN码：{asnCode}");
            
            string token;
            using (var loginClient = new HttpClient())
            {
                var loginData = new
                {
                    appKey = _erpSettings.AppKey,
                    appSecret = _erpSettings.AppSecret
                };

                var loginContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(loginData),
                    Encoding.UTF8,
                    "application/json");

                var loginResponse = await loginClient.PostAsync(_erpSettings.LoginUrl, loginContent);
                loginResponse.EnsureSuccessStatusCode();

                var loginResult = await loginResponse.Content.ReadAsStringAsync();
                var loginObj = System.Text.Json.JsonSerializer.Deserialize<ErpLoginResponse>(loginResult);

                if (loginObj == null || !loginObj.success || string.IsNullOrEmpty(loginObj.data?.token))
                {
                    _logger.LogError($"[操作ID: {operationId}] 登录ERP失败: {loginResult}");
                    return null;
                }

                token = loginObj.data.token;
            }

            using var getInfoClient = new HttpClient();
            getInfoClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            getInfoClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestData = new { ccode = asnCode };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true
            };

            var pushDataJson = System.Text.Json.JsonSerializer.Serialize(requestData, options);
            _logger.LogInformation($"[操作ID: {operationId}] 同步ASN明细, ASN：{pushDataJson}");

            var getContent = new StringContent(pushDataJson, Encoding.UTF8, "application/json");
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_erpSettings.GetAsnUrl),
                Content = getContent
            };

            var response = await getInfoClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(responseBody);
            var dataArray = jObject["data"] as JArray;

            if (dataArray == null || dataArray.Count == 0)
            {
                _logger.LogWarning($"[操作ID: {operationId}] 未找到ASN信息：{asnCode}");
                return null;
            }

            return dataArray
                .Select(item => MapToErpAsnDto(item))
                .ToList();
        }

        private static ErpAsnDto MapToErpAsnDto(JToken item)
        {
            return new ErpAsnDto
            {
                Ccode = item["ccode"]?.ToString(),
                Cordercode = item["cordercode"]?.ToString(),
                Cvenabbname = item["cvenabbname"]?.ToString(),
                Cvencode = item["cvencode"]?.ToString(),
                Cwhcode = item["cwhcode"]?.ToString(),
                Cwhname = item["cwhname"]?.ToString(),
                Darridate = item["darridate"]?.ToString(),
                Iasnflag = item["iasnflag"]?.ToString(),
                Cinvcode = item["cinvcode"]?.ToString(),
                Cinvname = item["cinvname"]?.ToString(),
                Cinvstd = item["cinvstd"]?.ToString(),
                Cinfvm_unit = item["cinvm_unit"]?.ToString(),
                Ipoquantity = decimal.TryParse(item["ipoquantity"]?.ToString(), out var ipoqty) ? ipoqty : 0,
                Cbatch = item["cbatch"]?.ToString(),
                Farrqty = decimal.TryParse(item["farrqty"]?.ToString(), out var farrqty) ? farrqty : 0,
                Wdhsl = decimal.TryParse(item["wdhsl"]?.ToString(), out var wdhsl) ? wdhsl : 0,
                Foutquantity = decimal.TryParse(item["foutquantity"]?.ToString(), out var foutqty) ? foutqty : 0,
                Iquantity = decimal.TryParse(item["iquantity"]?.ToString(), out var iqty) ? iqty : 0,
                Frealquantity = decimal.TryParse(item["frealquantity"]?.ToString(), out var frealqty) ? frealqty : 0,
                Cmemo = item["cmemo"]?.ToString(),
                Cmaker = item["cmaker"]?.ToString(),
                Ddate = item["ddate"]?.ToString(),
                Cbustype = item["cbustype"]?.ToString(),
                Cptcode = item["cptcode"]?.ToString(),
                Cptname = item["cptname"]?.ToString(),
                Dshipdate = item["dshipdate"]?.ToString(),
                Cdepcode = item["cdepcode"]?.ToString(),
                Cdepname = item["cdepname"]?.ToString(),
                Cpersoncode = item["cpersoncode"]?.ToString(),
                Cpersonname = item["cpersonname"]?.ToString(),
                CexchName = item["cexch_name"]?.ToString(),
                Id = long.TryParse(item["id"]?.ToString(), out var id) ? id : 0,
                Autoid = item["autoid"]?.ToString(),
                Headcmemo = item["headcmemo"]?.ToString(),
                Darridateb = item["darridateb"]?.ToString(),
                Cmaketime = item["cmaketime"]?.ToString(),
                Itaxrateb = decimal.TryParse(item["itaxrateb"]?.ToString(), out var taxRate) ? taxRate : 0,
                Iexchrate = decimal.TryParse(item["iexchrate"]?.ToString(), out var exchRate) ? exchRate : 0,
                Iposid = long.TryParse(item["iposid"]?.ToString(), out var posId) ? posId : 0,
                Bgsp = int.TryParse(item["bgsp"]?.ToString(), out var bgsp) ? bgsp : 0,
                Ccloser = item["ccloser"]?.ToString(),
                Cfree2 = item["cfree2"]?.ToString(),
                Cfree3 = item["cfree3"]?.ToString(),
                Cfree5 = item["cfree5"]?.ToString(),
                Cinvaddcode = item["cinvaddcode"]?.ToString()
            };
        }

        /// <summary>
        /// ERP登录（使用默认配置）
        /// </summary>
        /// <returns>登录结果，包含token</returns>
        public async Task<ErpLoginResponseDto> LoginErpAsync()
        {
            var operationId = Guid.NewGuid().ToString();
            _logger.LogInformation($"[操作ID: {operationId}] 开始ERP登录");
            
            try
            {
                string token;
                using (var loginClient = new HttpClient())
                {
                    var loginData = new
                    {
                        AppKey = _erpSettings.AppKey,
                        AppSecret = _erpSettings.AppSecret
                    };

                    var loginContent = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(loginData),
                        Encoding.UTF8,
                        "application/json");

                    var loginResponse = await loginClient.PostAsync(_erpSettings.LoginUrl, loginContent);
                    loginResponse.EnsureSuccessStatusCode();

                    var loginResult = await loginResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation($"[操作ID: {operationId}] ERP登录响应：{loginResult}");
                    
                    var loginObj = System.Text.Json.JsonSerializer.Deserialize<ErpLoginResponse>(loginResult);

                    if (loginObj == null || !loginObj.success || string.IsNullOrEmpty(loginObj.data?.token))
                    {
                        _logger.LogError($"[操作ID: {operationId}] 登录ERP失败: {loginResult}");
                        return new ErpLoginResponseDto
                        {
                            Success = false,
                            Message = "登录ERP失败"
                        };
                    }

                    token = loginObj.data.token;
                }

                _logger.LogInformation($"[操作ID: {operationId}] ERP登录成功");
                return new ErpLoginResponseDto
                {
                    Success = true,
                    Message = "登录成功",
                    Token = token
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"[操作ID: {operationId}] ERP登录异常: {ex.Message}");
                return new ErpLoginResponseDto
                {
                    Success = false,
                    Message = $"登录异常：{ex.Message}"
                };
            }
        }

        /// <summary>
        /// 推送ERP收货单
        /// </summary>
        /// <param name="asnCode">ASN码</param>
        /// <returns>推送结果</returns>
        public async Task<bool> PushErpReceiptAsync(string asnCode)
        {
            var operationId = Guid.NewGuid().ToString();
            _logger.LogInformation($"[操作ID: {operationId}] 开始推送ERP收货单，ASN码：{asnCode}");
            
            try
            {
                if (string.IsNullOrWhiteSpace(asnCode))
                {
                    _logger.LogWarning($"[操作ID: {operationId}] ASN码不能为空");
                    return false;
                }

                _logger.LogInformation($"[操作ID: {operationId}] 开始登录ERP获取token");
                string token;
                using (var loginClient = new HttpClient())
                {
                    var loginData = new
                    {
                        appKey = _erpSettings.AppKey,
                        appSecret = _erpSettings.AppSecret
                    };

                    var loginContent = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(loginData),
                        Encoding.UTF8,
                        "application/json");

                    var loginResponse = await loginClient.PostAsync(_erpSettings.LoginUrl, loginContent);
                    loginResponse.EnsureSuccessStatusCode();

                    var loginResult = await loginResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation($"[操作ID: {operationId}] ERP登录响应: {loginResult}");
                    
                    var loginObj = System.Text.Json.JsonSerializer.Deserialize<ErpLoginResponse>(loginResult);

                    if (loginObj == null)
                    {
                        _logger.LogError($"[操作ID: {operationId}] 登录ERP失败: 响应解析为空");
                        return false;
                    }

                    if (!loginObj.success)
                    {
                        _logger.LogError($"[操作ID: {operationId}] 登录ERP失败: {loginObj.data?.token ?? "未知错误"}");
                        return false;
                    }

                    if (string.IsNullOrEmpty(loginObj.data?.token))
                    {
                        _logger.LogError($"[操作ID: {operationId}] 登录ERP失败: token为空");
                        return false;
                    }

                    token = loginObj.data.token;
                    _logger.LogInformation($"[操作ID: {operationId}] 登录ERP成功，获取到token");
                }

                _logger.LogInformation($"[操作ID: {operationId}] 使用token推送ERP收货单，ASN码：{asnCode}，请求地址：{_erpSettings.PushReceiptUrl}");
                using (var pushClient = new HttpClient())
                {
                    pushClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    pushClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var requestData = new { ccode = asnCode };

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        PropertyNameCaseInsensitive = true
                    };

                    var pushDataJson = System.Text.Json.JsonSerializer.Serialize(requestData, options);
                    _logger.LogInformation($"[操作ID: {operationId}] 请求ERP推送收货单接口，URL：{_erpSettings.PushReceiptUrl}，请求参数：{pushDataJson}");

                    var pushContent = new StringContent(
                        pushDataJson,
                        Encoding.UTF8,
                        "application/json");

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(_erpSettings.PushReceiptUrl),
                        Content = pushContent
                    };

                    _logger.LogInformation($"[操作ID: {operationId}] 发送推送ERP收货单请求：{_erpSettings.PushReceiptUrl}");
                    HttpResponseMessage response = await pushClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"[操作ID: {operationId}] 推送ERP收货单响应内容: {responseBody}");

                    var jObject = JObject.Parse(responseBody);
                    var success = jObject["success"]?.ToObject<bool>() ?? false;

                    if (success)
                    {
                        _logger.LogInformation($"[操作ID: {operationId}] 成功推送ERP收货单：{asnCode}");
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"[操作ID: {operationId}] 推送ERP收货单失败：{asnCode}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[操作ID: {operationId}] 推送ERP收货单异常：{asnCode}");
                return false;
            }
        }

        /// <summary>
        /// 生成到货单并推送到ERP
        /// </summary>
        public async Task<PuArrVouchAddResponseDto> PushPuArrVouchAsync(PuArrVouchAddRequestDto input)
        {
            var operationId = Guid.NewGuid().ToString();
            _logger.LogInformation($"[操作ID: {operationId}] 开始生成到货单推送");

            try
            {
                if (input == null)
                {
                    return new PuArrVouchAddResponseDto
                    {
                        Success = false,
                        Message = "请求参数不能为空"
                    };
                }

                var pushUid = input.Uid > 0 ? input.Uid.Value : GeneratePuArrVouchUid();
                _logger.LogInformation($"[操作ID: {operationId}] 使用推送UID：{pushUid}");

                if (string.IsNullOrWhiteSpace(input.CAsnCode))
                {
                    return new PuArrVouchAddResponseDto
                    {
                        Success = false,
                        Message = "ASN单号不能为空"
                    };
                }

                if (string.IsNullOrWhiteSpace(input.CVenCode))
                {
                    return new PuArrVouchAddResponseDto
                    {
                        Success = false,
                        Message = "供应商编码不能为空"
                    };
                }

                if (string.IsNullOrWhiteSpace(input.Cpocode))
                {
                    return new PuArrVouchAddResponseDto
                    {
                        Success = false,
                        Message = "订单号不能为空"
                    };
                }

                if (input.Data == null || input.Data.Count == 0)
                {
                    return new PuArrVouchAddResponseDto
                    {
                        Success = false,
                        Message = "明细列表不能为空"
                    };
                }

                if (string.IsNullOrWhiteSpace(_erpSettings.PuArrVouchAddUrl))
                {
                    return new PuArrVouchAddResponseDto
                    {
                        Success = false,
                        Message = "ERP到货单推送地址未配置"
                    };
                }

                for (var i = 0; i < input.Data.Count; i++)
                {
                    var detail = input.Data[i];
                    if (string.IsNullOrWhiteSpace(detail.CInvCode))
                    {
                        return new PuArrVouchAddResponseDto
                        {
                            Success = false,
                            Message = $"第{i + 1}行物料料号不能为空"
                        };
                    }

                    if (string.IsNullOrWhiteSpace(detail.CBatch))
                    {
                        return new PuArrVouchAddResponseDto
                        {
                            Success = false,
                            Message = $"第{i + 1}行批号不能为空"
                        };
                    }

                    if (string.IsNullOrWhiteSpace(detail.Cordercode))
                    {
                        return new PuArrVouchAddResponseDto
                        {
                            Success = false,
                            Message = $"第{i + 1}行订单号不能为空"
                        };
                    }
                }

                var token = await GetErpTokenAsync(operationId);
                if (string.IsNullOrEmpty(token))
                {
                    return new PuArrVouchAddResponseDto
                    {
                        Success = false,
                        Message = "登录ERP失败"
                    };
                }

                var pushPayload = BuildPuArrVouchPayload(input, pushUid);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var pushDataJson = System.Text.Json.JsonSerializer.Serialize(pushPayload, options);
                _logger.LogInformation($"[操作ID: {operationId}] 请求ERP生成到货单接口，URL：{_erpSettings.PuArrVouchAddUrl}，请求参数：{pushDataJson}");

                using var pushClient = new HttpClient();
                pushClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                pushClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var pushContent = new StringContent(pushDataJson, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_erpSettings.PuArrVouchAddUrl),
                    Content = pushContent
                };

                var response = await pushClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"[操作ID: {operationId}] 生成到货单响应：{responseBody}");

                var jObject = JObject.Parse(responseBody);
                var success = jObject["success"]?.ToObject<bool>() ?? false;
                var message = jObject["message"]?.ToString() ?? jObject["msg"]?.ToString();

                return new PuArrVouchAddResponseDto
                {
                    Success = success,
                    Message = success ? "生成到货单成功" : (message ?? "生成到货单失败"),
                    ErpData = jObject["data"]?.ToObject<object>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[操作ID: {operationId}] 生成到货单推送异常");
                return new PuArrVouchAddResponseDto
                {
                    Success = false,
                    Message = $"生成到货单异常：{ex.Message}"
                };
            }
        }

        /// <summary>
        /// 推送生成来料报检单请求
        /// </summary>
        public async Task<LLBJDAddResponseDto> PushLLBJDAddAsync(LLBJDAddRequestDto input)
        {
            var operationId = Guid.NewGuid().ToString();
            _logger.LogInformation($"[操作ID: {operationId}] 开始生成来料报检单推送");

            // 1. 基础参数校验
            if (input == null || string.IsNullOrWhiteSpace(input.Cmd))
            {
                return new LLBJDAddResponseDto { Success = false, Message = "请求参数无效" };
            }

            try
            {
                _logger.LogInformation($"开始推送来料报检单，指令：{input.Cmd}，制单人：{input.Maker}");

                var token = await GetErpTokenAsync(operationId);
                if (string.IsNullOrEmpty(token))
                {
                    return new LLBJDAddResponseDto
                    {
                        Success = false,
                        Message = "登录ERP失败"
                    };
                }

                // 2. 处理 Data 字段（将其序列化为 JSON 字符串，以契合目标接口要求）
                var jsonOptions = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // 根据实际接口大小写要求调整
                };

                // 将 Data 列表序列化为字符串
                string dataJsonString = JsonSerializer.Serialize(input.Data, jsonOptions);

                // 构建最终发送的匿名对象（确保 Data 是字符串类型）
                var payload = new
                {
                    Cmd = input.Cmd,
                    TaskId = input.TaskId ?? string.Empty,
                    Maker = input.Maker,
                    Id = input.Id,
                    Data = dataJsonString
                };

                string jsonContent = JsonSerializer.Serialize(payload, jsonOptions);
                _logger.LogDebug($"推送报文内容：{jsonContent}");

                // 3. 发送 HTTP POST 请求
                using var pushClient = new HttpClient();
                pushClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                pushClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await pushClient.PostAsync(_erpSettings.U8ApiUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"推送来料报检单响应状态码：{(int)response.StatusCode}，响应内容：{responseBody}");

                // 4. 解析响应结果
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<LLBJDAddResponseDto>(responseBody, jsonOptions);
                    return result ?? new LLBJDAddResponseDto { Success = false, Message = "响应结果解析为空" };
                }
                else
                {
                    return new LLBJDAddResponseDto
                    {
                        Success = false,
                        Message = $"第三方接口调用失败，状态码：{(int)response.StatusCode}，原因：{responseBody}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"推送来料报检单发生异常，指令：{input.Cmd}");
                return new LLBJDAddResponseDto { Success = false, Message = $"系统异常：{ex.Message}" };
            }
        }
        /// <summary>
        /// 当同一ASN单号下所有明细均已入库完成时，自动推送到货单
        /// </summary>
        public async Task<PuArrVouchAddResponseDto> TryPushPuArrVouchIfAllLinesCompletedAsync(string asnCode)
        {
            var operationId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(asnCode))
            {
                return null;
            }

            var allLines = await _erpAsnRepository.GetListByAsnCodeAsync(asnCode).ConfigureAwait(false);
            var activeLines = allLines.Where(x => x.Status != AsnStatus.Cancelled).ToList();
            if (activeLines.Count == 0)
            {
                return null;
            }

            if (activeLines.Any(x => x.IsPushedToErp))
            {
                _logger.LogInformation($"[操作ID: {operationId}] ASN {asnCode} 已推送过到货单，跳过");
                return new PuArrVouchAddResponseDto
                {
                    Success = true,
                    Message = "已推送过到货单"
                };
            }

            if (!activeLines.All(x => x.Status == AsnStatus.Completed))
            {
                _logger.LogInformation($"[操作ID: {operationId}] ASN {asnCode} 尚有明细未完成入库，暂不推送");
                return null;
            }

            var firstLine = activeLines.First();
            if (string.IsNullOrWhiteSpace(firstLine.SupplierCode))
            {
                _logger.LogWarning($"[操作ID: {operationId}] ASN {asnCode} 供应商编码为空，无法推送到货单");
                return new PuArrVouchAddResponseDto
                {
                    Success = false,
                    Message = "供应商编码为空，无法推送到货单"
                };
            }

            var headerOrderCode = !string.IsNullOrWhiteSpace(firstLine.OrderCode)
                ? firstLine.OrderCode
                : activeLines.Select(x => x.OrderCode).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            var request = new PuArrVouchAddRequestDto
            {
                CAsnCode = asnCode,
                CVenCode = firstLine.SupplierCode,
                Cpocode = headerOrderCode,
                Data = activeLines.Select(line => new PuArrVouchDetailRequestDto
                {
                    CInvCode = line.MaterialCode,
                    IQuantity = line.InWarehouseQuantity,
                    INum = 1,
                    FRealQuantity = line.AlreadyStockInQuantity ?? 0,
                    FRealNumy = 1,
                    CBatch = string.IsNullOrWhiteSpace(line.BatchCode) ? "0" : line.BatchCode,
                    Cordercode = line.OrderCode,
                    IPoDetailId = line.PoDetailId ?? line.ErpOrderDetailId
                }).ToList()
            };

            _logger.LogInformation($"[操作ID: {operationId}] ASN {asnCode} 全部入库完成，开始自动推送到货单");
            var result = await PushPuArrVouchAsync(request).ConfigureAwait(false);
            if (result.Success)
            {
                foreach (var line in activeLines)
                {
                    line.MarkAsPushedToErp();
                    await _erpAsnRepository.UpdateAsync(line).ConfigureAwait(false);
                }

                _logger.LogInformation($"[操作ID: {operationId}] ASN {asnCode} 自动推送到货单成功");
            }
            else
            {
                _logger.LogWarning($"[操作ID: {operationId}] ASN {asnCode} 自动推送到货单失败：{result.Message}");
            }

            return result;
        }

        private static long GeneratePuArrVouchUid()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private static object BuildPuArrVouchPayload(PuArrVouchAddRequestDto input, long uid)
        {
            return new
            {
                uid = uid,
                cAsnCode = input.CAsnCode,
                cVenCode = input.CVenCode,
                cDepCode = "11",
                cMaker = "张三",
                cBusType = "普通采购",
                cexch_name = "人民币",
                cpocode = input.Cpocode,
                cPtCode = 11,
                itaxrate = 13,
                iexchrate = 1,
                cMemo = "",
                Data = input.Data.Select((item, index) => new
                {
                    ivouchrowno = index + 1,
                    cWhCode = "200",
                    cInvCode = item.CInvCode,
                    iQuantity = item.IQuantity,
                    iinvexchrate = 1500,
                    cunitid = "202",
                    iNum = item.INum,
                    fRealQuantity = item.FRealQuantity,
                    fRealNumy = item.FRealNumy,
                    bGsp = 0,
                    cBatch = item.CBatch,
                    iPOsID = item.IPoDetailId ?? 1000104266,
                    cordercode = item.Cordercode
                }).ToList()
            };
        }

        private async Task<string> GetErpTokenAsync(string operationId)
        {
            using var loginClient = new HttpClient();
            var loginData = new
            {
                appKey = _erpSettings.AppKey,
                appSecret = _erpSettings.AppSecret
            };

            var loginContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(loginData),
                Encoding.UTF8,
                "application/json");

            var loginResponse = await loginClient.PostAsync(_erpSettings.LoginUrl, loginContent);
            loginResponse.EnsureSuccessStatusCode();

            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            _logger.LogInformation($"[操作ID: {operationId}] ERP登录响应: {loginResult}");

            var loginObj = System.Text.Json.JsonSerializer.Deserialize<ErpLoginResponse>(loginResult);
            if (loginObj == null || !loginObj.success || string.IsNullOrEmpty(loginObj.data?.token))
            {
                _logger.LogError($"[操作ID: {operationId}] 登录ERP失败: {loginResult}");
                return null;
            }

            return loginObj.data.token;
        }

        /// <summary>
        /// 获取ASN列表
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="asnCode">ASN码</param>
        /// <param name="supplierName">供应商名称</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="status">状态（1=已创建，2=收货中，3=已完成，4=已取消）</param>
        /// <returns>ASN列表</returns>
        public async Task<ErpAsnListResponseDto> GetAsnListAsync(int page, int pageSize, string asnCode = null, string supplierName = null, string startDate = null, string endDate = null, int? status = null)
        {
            try
            {
                _logger.LogInformation($"开始获取ASN列表，页码：{page}，每页数量：{pageSize}，状态：{status?.ToString() ?? "全部"}");

                var (asnList, total) = await _erpAsnRepository.GetAsnListAsync(page, pageSize, asnCode, supplierName, startDate, endDate, status);

                var items = asnList.Select(MapToErpAsnDto).ToList();

                var response = new ErpAsnListResponseDto
                {
                    Items = items,
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation($"获取ASN列表成功，总数：{total}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取ASN列表异常");
                return new ErpAsnListResponseDto
                {
                    Items = new List<ErpAsnDto>(),
                    Total = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }
        }

        /// <summary>
        /// 通过物料编号获取未完成的ASN单据信息
        /// </summary>
        /// <param name="materialCode">物料编号</param>
        /// <returns>未完成的ASN明细列表</returns>
        public async Task<ErpAsnValidateResponseDto> GetIncompleteAsnByMaterialCodeAsync(string materialCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(materialCode))
                {
                    return new ErpAsnValidateResponseDto
                    {
                        Success = false,
                        Message = "物料编号不能为空"
                    };
                }

                var normalizedMaterialCode = materialCode.Trim();
                _logger.LogInformation($"开始获取未完成ASN信息，物料编号：{normalizedMaterialCode}");

                var asnList = await _erpAsnRepository.GetIncompleteListByMaterialCodeAsync(normalizedMaterialCode);
                var items = asnList.Select(MapToErpAsnDto).ToList();

                return new ErpAsnValidateResponseDto
                {
                    Success = true,
                    Message = items.Count > 0 ? "获取未完成ASN信息成功" : "未找到该物料对应的未完成ASN信息",
                    Data = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取未完成ASN信息异常，物料编号：{materialCode}");
                return new ErpAsnValidateResponseDto
                {
                    Success = false,
                    Message = $"获取未完成ASN信息异常：{ex.Message}"
                };
            }
        }

        public async Task<ErpAsnValidateResponseDto> GetLocalAsnByCodeAsync(string asnCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(asnCode))
                    return new ErpAsnValidateResponseDto { Success = false, Message = "ASN码不能为空" };

                _logger.LogInformation($"从本地ErpAsns表查询ASN数据，ASN码：{asnCode}");
                var asnList = await _erpAsnRepository.GetListByAsnCodeAsync(asnCode.Trim());
                var items = asnList.Select(MapToErpAsnDto).ToList();

                return new ErpAsnValidateResponseDto
                {
                    Success = true,
                    Message = items.Count > 0 ? "查询成功" : "未找到该ASN数据",
                    Data = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询本地ASN数据异常，ASN码：{asnCode}");
                return new ErpAsnValidateResponseDto { Success = false, Message = $"查询异常：{ex.Message}" };
            }
        }

        private static ErpAsnDto MapToErpAsnDto(ErpAsn asn)
        {
            return new ErpAsnDto
            {
                Ccode = asn.AsnCode,
                Cordercode = asn.OrderCode,
                Cvenabbname = asn.SupplierName,
                Cvencode = asn.SupplierCode,
                Cwhcode = asn.WarehouseCode,
                Cwhname = asn.WarehouseName,
                Darridate = asn.ArrivalDate?.ToString("yyyy-MM-dd"),
                Iasnflag = asn.AsnFlag,
                Cinvcode = asn.MaterialCode,
                Cinvname = asn.MaterialName,
                Cinvstd = asn.Specs,
                Cinfvm_unit = asn.Unit,
                Ipoquantity = asn.PlanQuantity,
                Cbatch = asn.BatchCode,
                Farrqty = asn.ArrivedQuantity,
                Wdhsl = asn.NotArrivedQuantity,
                Foutquantity = asn.OutQuantity,
                Iquantity = asn.InWarehouseQuantity,
                Frealquantity = asn.RealQuantity,
                Cmemo = asn.Remarks,
                Cmaker = asn.Maker,
                Ddate = asn.BillDate?.ToString("yyyy-MM-dd"),
                Cbustype = asn.BusinessType,
                Cptcode = asn.ProcessTypeCode,
                Cptname = asn.ProcessTypeName,
                Dshipdate = asn.ShipDate?.ToString("yyyy-MM-dd"),
                Cdepcode = asn.DepartmentCode,
                Cdepname = asn.DepartmentName,
                Cpersoncode = asn.PersonCode,
                Cpersonname = asn.PersonName,
                CexchName = asn.ExchangeName,
                Id = asn.ErpOrderDetailId ?? 0,
                Autoid = asn.Id.ToString(),
                Headcmemo = asn.Headcmemo,
                Darridateb = asn.ArrivalDateB?.ToString("yyyy-MM-dd"),
                Cmaketime = asn.MakeTime,
                Itaxrateb = asn.TaxRate,
                Iexchrate = asn.ExchangeRate,
                Iposid = asn.PoDetailId ?? 0,
                Bgsp = asn.IsGsp ? 1 : 0,
                Ccloser = asn.Closer,
                Cfree2 = asn.Free2,
                Cfree3 = asn.Free3,
                Cfree5 = asn.Free5,
                Cinvaddcode = asn.MaterialAddCode,
                Status = (int)asn.Status,
                StatusName = GetAsnStatusName(asn.Status),
                AlreadyStockInQuantity = asn.AlreadyStockInQuantity,
                PendingStockInQuantity = asn.GetPendingStockInQuantity()
            };
        }

        private static string GetAsnStatusName(AsnStatus status)
        {
            return status switch
            {
                AsnStatus.Created => "已创建",
                AsnStatus.Received => "收货中",
                AsnStatus.Completed => "已完成",
                AsnStatus.Cancelled => "已取消",
                _ => status.ToString()
            };
        }
    }
}
