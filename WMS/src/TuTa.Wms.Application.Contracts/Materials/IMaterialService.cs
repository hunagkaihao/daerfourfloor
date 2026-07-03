using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Materials.Dtos;
using System.Collections.Generic;

namespace TuTa.Wms.Materials
{
    public interface IMaterialService : IApplicationService
    {
        public Task<ResponseDto> CreateMaterialAsync(MaterialCreateDto para);

        public Task<ResponseDto> DeleteMaterialAsync(string materialCodeToDel);

        public Task<ResponseDto> UpdateMaterialAsync(Guid materialIdToUpdate, MaterialUpdateDto para);

        public Task<PagedResultDto<MaterialDto>> GetPagedMaterialsAsync(PagedMaterialsQueryDto para);

        public Task<List<MaterialDto>> GetMaterialsByMaterialCodeTipAsync(string materialCodeTip);

        public Task<MaterialImportResultDto> ImportMaterialBasicDataAsync(byte[] fileBytes, string fileName);
    }
}
