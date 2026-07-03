using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpStockOutReturn : Entity<int>
    {
        private ErpStockOutReturn()
        {            
        }

        public ErpStockOutReturn(
            DateTime stockOutDate,
            string gysCode, string gysName, string deptCode, string deptName,
            short stockOutType,
            string pickBatch,
            string goodsCode, string goodsName, string goodsSpecs,
            string uniqueCode,
            string materialCode, string materialName, string specs, string unit,
            string checkNo, 
            string barcode,
            decimal stockOutCount,
            string operatorName, 
            string areaCode,
            string areaName,
            string lC_MAN = null)
        {
            CHKD_DATE = stockOutDate;
            GYS_ID = gysCode;
            GYS_NAME = gysName;
            DEPT_ID = deptCode;
            DEPT_NAME = deptName;
            CHK_TYPE = stockOutType;
            CHKTZPRDT_PH = pickBatch;
            CHKTZDCP_ID = goodsCode;
            CHKTZDCP_NAME = goodsName;
            CHKTZDCP_SPEC = goodsSpecs;
            CHKTZD_ITM = uniqueCode;
            PRDT_ID = Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            PRDT_NAME = Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            PRDT_SPEC = specs;
            PRDT_UNIT = unit;
            PRDT_PH = Check.NotNullOrWhiteSpace(checkNo, nameof(checkNo));
            DHTZD_TXM = Check.NotNullOrWhiteSpace(barcode, nameof(barcode));
            CHK_NUM = Check.Positive(stockOutCount, nameof(stockOutCount));
            JLUSER_NAME = operatorName;
            IFJS = false;
            JS_DATE = null;
            IFDELETE = false;
            JS_SM = null;
            LC_MAN = lC_MAN;
            CK_KW = areaCode;
            CKKW_NAME = areaName;
        }

        /// <summary>
        /// 出库时间
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime CHKD_DATE { get; set; }

        /// <summary>
        /// 供应商编号
        /// </summary>
        [StringLength(20)]
        public string GYS_ID { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        [StringLength(120)]
        public string GYS_NAME { get; set; }

        /// <summary>
        /// 部门编号
        /// </summary>
        [StringLength(20)]
        public string DEPT_ID { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        [StringLength(120)]
        public string DEPT_NAME { get; set; }

        /// <summary>
        /// 出库类型
        /// </summary>
        public short CHK_TYPE { get; set; }

        /// <summary>
        /// 领用生产批号
        /// </summary>
        [StringLength(30)]
        public string CHKTZPRDT_PH { get; set; }

        /// <summary>
        /// 领用成品物料编号
        /// </summary>
        [StringLength(30)]
        public string CHKTZDCP_ID { get; set; }

        /// <summary>
        /// 领用成品物料名称
        /// </summary>
        [StringLength(130)]
        public string CHKTZDCP_NAME { get; set; }

        /// <summary>
        /// 领用成品物料规格
        /// </summary>
        [StringLength(130)]
        public string CHKTZDCP_SPEC { get; set; }

        /// <summary>
        /// 领用通知唯一性编号
        /// </summary>
        [StringLength(32)]
        public string CHKTZD_ITM { get; set; }

        /// <summary>
        /// 物料编号
        /// </summary>
        [StringLength(20)]
        [Required]
        public string PRDT_ID { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        [StringLength(120)]
        [Required]
        public string PRDT_NAME { get; set; }

        /// <summary>
        /// 物料规格
        /// </summary>
        [StringLength(120)]
        public string PRDT_SPEC { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [StringLength(10)]
        public string PRDT_UNIT { get; set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        [StringLength(30)]
        [Required]
        public string PRDT_PH { get; set; }

        /// <summary>
        /// 收料码
        /// </summary>
        [StringLength(50)]
        [Required]
        public string DHTZD_TXM { get; set; }

        /// <summary>
        /// 出库数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal CHK_NUM { get; set; }

        /// <summary>
        /// 出库人员
        /// </summary>
        [StringLength(20)]
        public string JLUSER_NAME { get; set; }

        /// <summary>
        /// 领料员
        /// </summary>
        [StringLength(20)]
        public string LC_MAN { get; set; }

        public bool IFJS { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? JS_DATE { get; set; }

        public bool IFDELETE { get; set; }

        public string JS_SM { get; set; }



        /// <summary>
        /// 入库编号
        /// </summary>
        [StringLength(20)]
        public string CK_KW { get; set; }

        /// <summary>
        /// 入库区域
        /// </summary>
        [StringLength(20)]
        public string CKKW_NAME { get; set; }
    }
}
