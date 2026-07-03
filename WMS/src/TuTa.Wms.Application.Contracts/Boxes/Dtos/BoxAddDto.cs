namespace TuTa.Wms.Boxes.Dtos
{
    public class BoxAddDto
    {
        public string BoxCode { get; set; }

        public string BoxName { get; set; }

        public string BoxTypeName { get; set; } = null;

        public string BoxSpecsName { get; set; } = null;

        public int? BoxLength { get; set; } = null;

        public int? BoxWidth { get; set; } = null;

        public int? BoxHeight { get; set; } = null;
    }
}
