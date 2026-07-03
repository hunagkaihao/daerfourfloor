using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Dto;

namespace TuTa.Wms.Controllers.Erp
{
    [Route("erp/asn")]
    [ApiController]
    public class ErpAsnController : WmsController
    {
        private readonly IErpAsnAppService _erpAsnAppService;

        public ErpAsnController(IErpAsnAppService erpAsnAppService)
        {
            _erpAsnAppService = erpAsnAppService;
        }

        /// <summary>
        /// ERP登录（使用默认配置）
        /// </summary>
        /// <returns>登录结果，包含token</returns>
        [HttpPost("login")]
        [SwaggerOperation(summary: "ERP登录（使用默认配置）", Tags = new[] { "ERP ASN" })]
        public async Task<ErpLoginResponseDto> LoginErpAsync()
        {
            return await _erpAsnAppService.LoginErpAsync();
        }

        /// <summary>
        /// 通过ASN码获取信息
        /// </summary>
        /// <param name="asnCode">ASN码</param>
        /// <returns>ASN信息</returns>
        [HttpGet("get")]
        [SwaggerOperation(summary: "通过ASN码获取信息", Tags = new[] { "ERP ASN" })]
        public async Task<ErpAsnValidateResponseDto> GetAsnInfoAsync(string asnCode)
        {
            return await _erpAsnAppService.GetAsnInfoAsync(asnCode);
        }

        /// <summary>
        /// 保存ASN信息到数据库
        /// </summary>
        /// <param name="asnCode">ASN码</param>
        /// <returns>保存结果</returns>
        [HttpPost("save")]
        [SwaggerOperation(summary: "保存ASN信息到数据库", Tags = new[] { "ERP ASN" })]
        public async Task<ErpAsnSaveResponseDto> SaveAsnAsync(string asnCode)
        {
            return await _erpAsnAppService.SaveAsnAsync(asnCode);
        }

        /// <summary>
        /// 推送ERP收货单
        /// </summary>
        /// <param name="asnCode">ASN码</param>
        /// <returns>推送结果</returns>
        [HttpPost("push-receipt")]
        [SwaggerOperation(summary: "推送ERP收货单", Tags = new[] { "ERP ASN" })]
        public async Task<IActionResult> PushErpReceiptAsync(string asnCode)
        {
            if (string.IsNullOrEmpty(asnCode))
            {
                return BadRequest("ASN码不能为空");
            }

            var result = await _erpAsnAppService.PushErpReceiptAsync(asnCode);
            return Ok(new { success = result });
        }

        /// <summary>
        /// 生成到货单并推送到ERP
        /// </summary>
        /// <param name="input">到货单推送参数</param>
        /// <returns>推送结果</returns>
        [HttpPost("push-arr-vouch")]
        [SwaggerOperation(summary: "生成到货单", Tags = new[] { "ERP ASN" })]
        public async Task<PuArrVouchAddResponseDto> PushPuArrVouchAsync([FromBody] PuArrVouchAddRequestDto input)
        {
            return await _erpAsnAppService.PushPuArrVouchAsync(input);
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
        [HttpGet("list")]
        [SwaggerOperation(summary: "获取ASN列表", Tags = new[] { "ERP ASN" })]
        public async Task<ErpAsnListResponseDto> GetAsnListAsync(int page = 1, int pageSize = 10, string asnCode = null, string supplierName = null, string startDate = null, string endDate = null, int? status = null)
        {
            return await _erpAsnAppService.GetAsnListAsync(page, pageSize, asnCode, supplierName, startDate, endDate, status);
        }

        /// <summary>
        /// 通过物料编号获取未完成的ASN单据信息
        /// </summary>
        /// <param name="materialCode">物料编号</param>
        /// <returns>未完成的ASN明细列表</returns>
        [HttpGet("list/incomplete-by-material")]
        [SwaggerOperation(summary: "通过物料编号获取未完成的ASN单据信息", Tags = new[] { "ERP ASN" })]
        public async Task<ErpAsnValidateResponseDto> GetIncompleteAsnByMaterialCodeAsync(string materialCode)
        {
            return await _erpAsnAppService.GetIncompleteAsnByMaterialCodeAsync(materialCode);
        }
    }
}
