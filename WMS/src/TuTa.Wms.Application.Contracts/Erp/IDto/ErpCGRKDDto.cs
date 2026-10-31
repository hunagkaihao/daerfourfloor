namespace TuTa.Wms.Erp.IDto
{
    /// <summary>采购入库请求，兼容旧Params.Json及新Base接口Data报文。</summary>
    public class CGRKDAddRequestDto
    {
        public string Cmd { get; set; } = "CGRKDAdd";
        public string SetBook { get; set; } = "666";
        public string SetYear { get; set; } = "2020";
        public string LoginDate { get; set; }
        public string LoginName { get; set; }
        public string LoginPwd { get; set; }
        public CGRKDParams Params { get; set; }
        /// <summary>新接口幂等任务ID。</summary>
        public string Taskid { get; set; }
        /// <summary>新接口业务ID。</summary>
        public long Id { get; set; }
        /// <summary>新接口制单人。</summary>
        public string Maker { get; set; }
        /// <summary>新接口采购入库单数组的JSON字符串，与Params互斥。</summary>
        public string Data { get; set; }
    }

    public class CGRKDParams
    {
        public string Json { get; set; }
    }

    public class CGRKDAddResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }
        public string Data { get; set; }
        /// <summary>仅服务端使用：U8已明确返回业务失败；网络异常、超时或缺失结果不能视为确认失败。</summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool FailureConfirmed { get; set; }
    }
}
