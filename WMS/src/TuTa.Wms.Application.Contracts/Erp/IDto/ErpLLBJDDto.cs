using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TuTa.Wms.Erp.IDto
{
    /// <summary>
    /// 生成来料报检单推送请求DTO
    /// </summary>
    public class LLBJDAddRequestDto
    {
        /// <summary>
        /// 指令类型，固定为 "LLBJDAdd"
        /// </summary>
        public string Cmd { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// 制单人
        /// </summary>
        public string Maker { get; set; }

        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 报检单数据（序列化后会自动转换为 JSON 字符串）
        /// </summary>
        public List<LLBJDDataItem> Data { get; set; }
    }
    /// <summary>
    /// Data 数组中的报检单明细对象
    /// </summary>
    public class LLBJDDataItem
    {
        /// <summary>
        /// 添加类型
        /// </summary>
        public int AddType { get; set; }

        /// <summary>
        /// 来源单号
        /// </summary>
        public string CSourceCode { get; set; }

        /// <summary>
        /// 检验部门编码
        /// </summary>
        public string CInspectDepCode { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string CMemo { get; set; }

        /// <summary>
        /// 制单人
        /// </summary>
        public string CMAKER { get; set; }

        /// <summary>
        /// 报检单行明细
        /// </summary>
        public List<LLBJDDetail> Details { get; set; }
    }
    /// <summary>
    /// 报检单行明细
    /// </summary>
    public class LLBJDDetail
    {
        /// <summary>
        /// 来源明细自增ID
        /// </summary>
        public long SourceAutoId { get; set; }

        /// <summary>
        /// 存货编码
        /// </summary>
        public string CInvCode { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string CBatch { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal FQuantity { get; set; }
    }
    /// <summary>
    /// 生成来料报检单响应DTO
    /// </summary>
    public class LLBJDAddResponseDto
    {
        /// <summary>
        /// 返回信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 成功与否
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 返回添加后的单据信息及明细
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 任务码
        /// </summary>
        public int Code { get; set; }

    }
}
