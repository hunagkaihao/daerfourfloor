namespace TuTa.Wms.Erp.IDto
{
    public class CGRKDAddRequestDto
    {
        public string Cmd { get; set; } = "CGRKDAdd";
        public string SetBook { get; set; } = "666";
        public string SetYear { get; set; } = "2020";
        public string LoginDate { get; set; }
        public string LoginName { get; set; }
        public string LoginPwd { get; set; }
        public CGRKDParams Params { get; set; }
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
    }
}
