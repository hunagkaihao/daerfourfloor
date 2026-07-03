namespace TuTa.Wms.Boxes.Dtos
{
    public class BoxUpdateDto
    {
        public string BoxCodeNew { get; set; }

        public string BoxNameNew { get; set; }

        public string BoxTypeNameNew { get; set; }

        public string BoxSpecsNameNew { get; set; }

        public int? BoxLengthNew { get; set; }    

        public int? BoxWidthNew { get; set; }

        public int? BoxHeightNew { get; set; }
    }
}
