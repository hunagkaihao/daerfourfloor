using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Materials;
using TuTa.Wms.Materials.Dtos;
using System.Collections.Generic;

namespace TuTa.Wms.Controllers.Materials;

[Route("wms/material")]
[ApiController]
public class MaterialController : WmsController, IMaterialService
{
    private readonly IMaterialService _materialServece;
    public MaterialController(IMaterialService materialServece)
    {
        _materialServece = materialServece;
    }

    [HttpPost("materialAdd")]
    public async Task<ResponseDto> CreateMaterialAsync(MaterialCreateDto para)
    {
        return await _materialServece.CreateMaterialAsync(para).ConfigureAwait(false);
    }

    [HttpPost("materialDel")]
    public async Task<ResponseDto> DeleteMaterialAsync(string materialCodeToDel)
    {
        return await _materialServece.DeleteMaterialAsync(materialCodeToDel).ConfigureAwait(false);
    }

    [HttpPost("materialUpdate")]
    public async Task<ResponseDto> UpdateMaterialAsync(Guid materialIdToUpdate, MaterialUpdateDto para)
    {
        return await _materialServece.UpdateMaterialAsync(materialIdToUpdate, para).ConfigureAwait(false);
    }

    [HttpPost("pagedMaterialsGet")]
    public async Task<PagedResultDto<MaterialDto>> GetPagedMaterialsAsync(PagedMaterialsQueryDto para)
    {
        return await _materialServece.GetPagedMaterialsAsync(para).ConfigureAwait(false);
    }

    [HttpGet("materialsWithCodeTipGet")]
    public async Task<List<MaterialDto>> GetMaterialsByMaterialCodeTipAsync(string materialCodeTip)
    {
        return await _materialServece.GetMaterialsByMaterialCodeTipAsync(materialCodeTip).ConfigureAwait(false);
    }

    [NonAction]
    public Task<MaterialImportResultDto> ImportMaterialBasicDataAsync(byte[] fileBytes, string fileName)
    {
        return _materialServece.ImportMaterialBasicDataAsync(fileBytes, fileName);
    }

    [HttpPost("importMaterialBasicData")]
    [Consumes("multipart/form-data")]
    public async Task<MaterialImportResultDto> ImportMaterialBasicDataFromFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new Volo.Abp.UserFriendlyException("请选择Excel文件");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream).ConfigureAwait(false);
        return await _materialServece.ImportMaterialBasicDataAsync(memoryStream.ToArray(), file.FileName).ConfigureAwait(false);
    }
}