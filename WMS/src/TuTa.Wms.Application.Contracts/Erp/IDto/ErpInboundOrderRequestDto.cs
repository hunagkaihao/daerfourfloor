using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Erp.Dto
{
    /// <summary>
    /// ERP入库单接口请求DTO
    /// </summary>
    public class ErpInboundOrderRequestDto
    {
        /// <summary>
        /// 入库单号
        /// </summary>
        [Required(ErrorMessage = "入库单号不能为空")]
        public string FStkInLogNo { get; set; }

        /// <summary>
        /// 仓库代号
        /// </summary>
        [Required(ErrorMessage = "仓库代号不能为空")]
        public string FStkCode { get; set; }

        /// <summary>
        /// 计划入库日期
        /// </summary>
        [Required(ErrorMessage = "计划入库日期不能为空")]
        public string FPlanInDate { get; set; }

        /// <summary>
        /// 入库单明细
        /// </summary>
        [Required(ErrorMessage = "入库单明细不能为空")]
        public List<ErpInboundItemRequestDto> FStkInMxs { get; set; } = new List<ErpInboundItemRequestDto>();
    }

    /// <summary>
    /// ERP入库单项接口请求DTO
    /// </summary>
    public class ErpInboundItemRequestDto
    {
        /// <summary>
        /// 材料代号
        /// </summary>
        [Required(ErrorMessage = "材料代号不能为空")]
        public string fGoodsCode { get; set; }

        /// <summary>
        /// 材料名称
        /// </summary>
        [Required(ErrorMessage = "材料名称不能为空")]
        public string fGoodsName { get; set; }

        /// <summary>
        /// 计划入库数量
        /// </summary>
        [Required(ErrorMessage = "计划入库数量不能为空")]
        public string fPlanInQty { get; set; }

        /// <summary>
        /// 实际入库数量
        /// </summary>
        [Required(ErrorMessage = "实际入库数量不能为空")]
        public string fActInQty { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        [Required(ErrorMessage = "单位不能为空")]
        public string fUnitCode { get; set; }

        /// <summary>
        /// 制令号
        /// </summary>
        public string fMoNo { get; set; }

        /// <summary>
        /// 等级代号
        /// </summary>
        public string fLvlCode { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string fLotNo { get; set; }
    }

    /// <summary>
    /// ERP入库单接口响应DTO
    /// </summary>
    public class ErpInboundOrderResponseDto
    {
        /// <summary>
        /// 成功与否
        /// </summary>
        public bool Succeed { get; set; }

        /// <summary>
        /// 失败的详细信息
        /// </summary>
        public string Message { get; set; }

       
    }
}
