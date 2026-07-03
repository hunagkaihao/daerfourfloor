using Microsoft.Extensions.Logging;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using TuTa.Wms.Application.Contracts.Shared;
using Wms.LogTool;
using TuTa.Wms.Materials.Aggregates;
using TuTa.Wms.Materials.Dtos;
using Microsoft.AspNetCore.Authorization;
using TuTa.Wms.Permissions;

namespace TuTa.Wms.Materials
{
    //[Authorize]
    public class MaterialService : WmsAppService, IMaterialService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly MaterialManager _materialManager;
        private readonly ILogger<MaterialService> _logger;

        public MaterialService(
            IMaterialRepository materialRepository,
            MaterialManager materialManager, 
            ILogger<MaterialService> logger)
        {
            _materialRepository = materialRepository;
            _materialManager = materialManager;
            _logger = logger;
        }

        //[Authorize(WmsPermissions.AddPermission)]
        public async Task<ResponseDto> CreateMaterialAsync(MaterialCreateDto para)
        {
            try
            {
                var typeCode = ResolveTypeCode(para.MaterialCode, para.TypeCode);
                var typeName = ResolveTypeName(para.TypeName);
                var goods = await _materialManager.CreateMaterialAsync(para.MaterialCode, para.MaterialName, para.Specs, para.Unit, typeCode, typeName,
                    para.IsHB, para.SafetyStock, para.FullBoxCount, para.ExpiryDate, para.IsQCPJ, para.IsPPAP,null,null,null,false,null).ConfigureAwait(false);

                await _materialRepository.InsertAsync(goods).ConfigureAwait(false);
                return new ResponseDto() { success = true, message = "添加物料定义成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> DeleteMaterialAsync(string materialCodeToDel)
        {
            try
            {
                var goodsExist = await _materialRepository.FindByMaterialCodeAsync(materialCodeToDel).ConfigureAwait(false);
                if (goodsExist == null)
                {
                    return new ResponseDto() { success = true, message = $"物料码为{materialCodeToDel}的物料不存在，默认删除成功" };
                }
                await _materialRepository.DeleteAsync(goodsExist).ConfigureAwait(false);
                return new ResponseDto() { success = true, message = "删除物料定义成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> UpdateMaterialAsync(Guid materialIdToUpdate, MaterialUpdateDto para)
        {
            try
            {
                Material materialExist = await _materialRepository.FindAsync(materialIdToUpdate).ConfigureAwait(false);
                if (materialExist == null)
                {
                    throw new Exception($"Id为{materialIdToUpdate}的物料定义不存在");
                }
                var typeCode = ResolveTypeCode(para.MaterialCodeNew, para.TypeCodeNew);
                var typeName = ResolveTypeName(para.TypeNameNew);
                await _materialManager.ModifyMaterialAsync(
                    materialExist, 
                    para.MaterialCodeNew, 
                    para.MaterialNameNew, 
                    para.SpecsNew, 
                    para.UnitNew, 
                    typeCode, 
                    typeName,
                    para.IsHBNew, 
                    para.SafetyStockNew, 
                    para.FullBoxCount,
                    para.ExpiryDateNew, 
                    para.IsQCPJNew, 
                    para.IsPPAPNew,null,null,null,false,null).ConfigureAwait(false);

                await _materialRepository.UpdateAsync(materialExist).ConfigureAwait(false);
                return new ResponseDto() { success = true, message = "更新物料定义成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<PagedResultDto<MaterialDto>> GetPagedMaterialsAsync(PagedMaterialsQueryDto para)
        {
            try
            {
                var pagedGoods = await _materialRepository.GetPagedMaterialsAsync(
                    para.MaterialCode,
                    para.MaterialName,
                    para.Specs,
                    para.Unit,
                    para.TypeCode,
                    para.TypeName,
                    para.IsHB,
                    para.SafetyStock,
                    para.ExpiryDate,
                    para.IsQCPJ,
                    para.IsPPAP,
                    para.SkipCount,
                    para.MaxResultCount).ConfigureAwait(false);

                PagedResultDto<MaterialDto> result = new PagedResultDto<MaterialDto>()
                {
                    TotalCount = pagedGoods.TotalCount,
                    Items = ObjectMapper.Map<List<Material>, List<MaterialDto>>(pagedGoods.Items)
                };
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<MaterialDto>> GetMaterialsByMaterialCodeTipAsync(string materialCodeTip)
        {
            try
            {
                var materials = await _materialRepository.GetMaterialsByCodeTipAsync(materialCodeTip, false).ConfigureAwait(false);

                return ObjectMapper.Map<List<Material>, List<MaterialDto>>(materials);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<MaterialImportResultDto> ImportMaterialBasicDataAsync(byte[] fileBytes, string fileName)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new UserFriendlyException("请上传Excel文件");
            }

            var result = new MaterialImportResultDto();
            var rows = ParseMaterialBasicExcel(fileBytes, fileName, result.Errors);
            result.TotalCount = rows.Count;

            if (rows.Count == 0)
            {
                result.Success = false;
                result.Message = result.Errors.Count > 0
                    ? "Excel解析失败，请检查文件格式"
                    : "Excel中没有可导入的数据";
                return result;
            }

            foreach (var row in rows)
            {
                try
                {
                    var materialExist = await _materialRepository.FindByMaterialCodeAsync(row.MaterialCode).ConfigureAwait(false);
                    if (materialExist != null)
                    {
                        result.SkipCount++;
                        result.Errors.Add($"第{row.RowNumber}行：物料码{row.MaterialCode}已存在，已跳过");
                        continue;
                    }

                    var typeCode = ResolveTypeCode(row.MaterialCode, null);
                    var typeName = ResolveTypeName(null);
                    var material = await _materialManager.CreateMaterialAsync(
                        row.MaterialCode,
                        row.MaterialName,
                        row.Specs,
                        row.Unit,
                        typeCode,
                        typeName,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        false,
                        null).ConfigureAwait(false);

                    await _materialRepository.InsertAsync(material).ConfigureAwait(false);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailCount++;
                    result.Errors.Add($"第{row.RowNumber}行：{ex.Message}");
                    _logger.Error(ex.Message);
                }
            }

            result.Success = result.FailCount == 0;
            result.Message = $"导入完成，共{result.TotalCount}条，成功{result.SuccessCount}条，跳过{result.SkipCount}条，失败{result.FailCount}条";
            return result;
        }

        private static List<MaterialBasicImportRow> ParseMaterialBasicExcel(byte[] fileBytes, string fileName, List<string> errors)
        {
            var rows = new List<MaterialBasicImportRow>();

            using var stream = new MemoryStream(fileBytes);
            IWorkbook workbook;
            if (!string.IsNullOrWhiteSpace(fileName) && fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            {
                workbook = new HSSFWorkbook(stream);
            }
            else
            {
                workbook = new XSSFWorkbook(stream);
            }

            var sheet = workbook.GetSheetAt(0);
            if (sheet == null)
            {
                errors.Add("Excel工作表为空");
                return rows;
            }

            var headerRow = sheet.GetRow(sheet.FirstRowNum);
            if (headerRow == null)
            {
                errors.Add("Excel表头为空");
                return rows;
            }

            var columnIndexMap = BuildColumnIndexMap(headerRow);
            var startRowIndex = columnIndexMap.ContainsKey("MaterialCode") ? sheet.FirstRowNum + 1 : sheet.FirstRowNum;

            for (var rowIndex = startRowIndex; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null)
                {
                    continue;
                }

                var materialCode = GetCellValue(row, columnIndexMap, "MaterialCode", 0);
                var materialName = GetCellValue(row, columnIndexMap, "MaterialName", 1);
                var specs = GetCellValue(row, columnIndexMap, "Specs", 2);
                var unit = GetCellValue(row, columnIndexMap, "Unit", 3);

                if (string.IsNullOrWhiteSpace(materialCode)
                    && string.IsNullOrWhiteSpace(materialName)
                    && string.IsNullOrWhiteSpace(specs)
                    && string.IsNullOrWhiteSpace(unit))
                {
                    continue;
                }

                var rowNumber = rowIndex + 1;
                if (string.IsNullOrWhiteSpace(materialCode))
                {
                    errors.Add($"第{rowNumber}行：MaterialCode不能为空");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(materialName))
                {
                    errors.Add($"第{rowNumber}行：MaterialName不能为空");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(unit))
                {
                    errors.Add($"第{rowNumber}行：Unit不能为空");
                    continue;
                }

                rows.Add(new MaterialBasicImportRow
                {
                    RowNumber = rowNumber,
                    MaterialCode = materialCode.Trim(),
                    MaterialName = materialName.Trim(),
                    Specs = specs?.Trim(),
                    Unit = unit.Trim()
                });
            }

            return rows;
        }

        private static Dictionary<string, int> BuildColumnIndexMap(IRow headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = headerRow.FirstCellNum; i < headerRow.LastCellNum; i++)
            {
                var header = GetCellStringValue(headerRow.GetCell(i));
                if (string.IsNullOrWhiteSpace(header))
                {
                    continue;
                }

                map[header.Trim()] = i;
            }

            return map;
        }

        private static string GetCellValue(IRow row, Dictionary<string, int> columnIndexMap, string columnName, int defaultIndex)
        {
            if (columnIndexMap.TryGetValue(columnName, out var index))
            {
                return GetCellStringValue(row.GetCell(index));
            }

            return GetCellStringValue(row.GetCell(defaultIndex));
        }

        private static readonly DataFormatter CellFormatter = new DataFormatter();

        private static string GetCellStringValue(ICell cell)
        {
            if (cell == null)
            {
                return null;
            }

            return CellFormatter.FormatCellValue(cell)?.Trim();
        }

        private sealed class MaterialBasicImportRow
        {
            public int RowNumber { get; set; }

            public string MaterialCode { get; set; }

            public string MaterialName { get; set; }

            public string Specs { get; set; }

            public string Unit { get; set; }
        }

        private static string ResolveTypeCode(string materialCode, string typeCode)
        {
            if (!string.IsNullOrWhiteSpace(typeCode))
            {
                return typeCode.Trim();
            }

            return !string.IsNullOrWhiteSpace(materialCode) ? materialCode.Trim().Substring(0, 1) : "0";
        }

        private static string ResolveTypeName(string typeName)
        {
            return string.IsNullOrWhiteSpace(typeName) ? "-" : typeName.Trim();
        }
    }
}
