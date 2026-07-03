using System.Collections.Generic;

namespace TuTa.Wms.Materials.Dtos
{
    public class MaterialImportResultDto
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public int TotalCount { get; set; }

        public int SuccessCount { get; set; }

        public int SkipCount { get; set; }

        public int FailCount { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
    }
}
