using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.PickLists.Dtos
{
    public class NoPlanPickTypeDto : EntityDto
    {
        public int PickTypeNo { get; set; }

        public string PickTypeName { get; set; }
    }
}
