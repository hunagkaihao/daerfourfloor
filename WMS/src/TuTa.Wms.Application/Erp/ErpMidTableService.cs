using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using TuTa.Wms.Erp.Dtos;
using TuTa.Wms.Erp.Entities;
using Wms.LogTool;
using System.Collections.Generic;
using TuTa.Wms.PickLists.Dtos;
using Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    public class ErpMidTableService : WmsAppService, IErpMidTableService
    {
        private readonly IErpPickManRepository _erpPickManRepository;
        private readonly ILogger<ErpMidTableService> _logger;

        public ErpMidTableService(
            IErpPickManRepository erpPickManRepository,
            ILogger<ErpMidTableService> logger)
        {
            _erpPickManRepository = erpPickManRepository;
            _logger = logger;
        }

        public async Task<List<ErpPickManDto>> GetPickerNamesAsync(string nameTip)
        {
            try
            {
                var pickers = await _erpPickManRepository.GetPickManNamesAsync(nameTip).ConfigureAwait(false);
                if (pickers == null || pickers.Count == 0)
                    return new List<ErpPickManDto>();

                List<ErpPickManDto> erpPickManDtos = new List<ErpPickManDto>();
                foreach (var picker in pickers)
                {
                    ErpPickManDto dto = new ErpPickManDto() { PickerName = picker.MAN_NAME };
                    erpPickManDtos.Add(dto);
                }

                return erpPickManDtos;
            }
            catch(Exception ex)
            {
                _logger.LogException(ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        /// <summary>
        /// 通过收料条形码，获取待组盘的检后物料
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        //public async Task<ErpGoodsAftChkDto> GetAftChkGoodsToGroupBoxByBarcodeAsync(string barcode)
        //{
        //    try
        //    {
        //        var goods = await _erpGoodsAftChkRepository.FindByBarcodeAsync(barcode).ConfigureAwait(false);
        //        if (goods == null)
        //            throw new Exception($"收料码为{barcode}的检后物料不存在");

        //        goods.SetIsReceived();  //读取物料信息后，该物料就被认为已经使用，不论设置是否成功，都执行更新
        //        await _erpGoodsAftChkRepository.UpdateAsync(goods).ConfigureAwait(false);

        //        return ObjectMapper.Map<ErpStockAftChk, ErpGoodsAftChkDto>(goods);
        //    }
        //    catch(Exception ex) 
        //    {
        //        _logger.Error(ex.Message);
        //        throw new UserFriendlyException(ex.Message);
        //    }
        //}
    }
}
