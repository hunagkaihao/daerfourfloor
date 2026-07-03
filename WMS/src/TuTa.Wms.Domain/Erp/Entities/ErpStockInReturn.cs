using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TuTa.Wms.Stocks;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpStockInReturn : Entity<int>
    {
        private ErpStockInReturn()
        {            
        }

        public ErpStockInReturn(
            DateTime stockInDate,
            string gysCode, string gysName,
            StockInType stockInType,
            string materialCode, string materialName, string specs, string unit,
            string targetHouseCode, string targetHouseName,
            string checkNo,
            string barcode,
            string checkOrderNo,
            decimal stockInCount,
            string operatorName,
            string areaCode,
            string areaName)
        {
            RKD_DATE = stockInDate;
            GYS_ID = gysCode;
            GYS_NAME = gysName;
            RK_TYPE = (int)stockInType;
            PRDT_ID = Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            PRDT_NAME = Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            PRDT_SPEC = specs;
            PRDT_UNIT = unit;
            CK_ID = Check.NotNullOrWhiteSpace(targetHouseCode, nameof(targetHouseCode));
            CK_NAME = Check.NotNullOrWhiteSpace(targetHouseName, nameof(targetHouseName));
            PRDT_PH = checkNo;
            DHTZD_TXM = Check.NotNullOrWhiteSpace(barcode, nameof(barcode));
            BYQC_ID = checkOrderNo;
            RK_NUM = Check.Positive(stockInCount, nameof(stockInCount));
            JLUSER_NAME = operatorName;
            IFJS = false;
            JS_DATE = null;
            IFDELETE = false;
            JS_SM = null;
            CK_KW = areaCode;
            CKKW_NAME = areaName;
        }

        /// <summary>
        /// 入库时间
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime RKD_DATE { get; set; }

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
        /// 入库类型
        /// </summary>
        [Required]
        public int RK_TYPE { get; set; }

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
        /// 收料仓库编号
        /// </summary>
        [StringLength(10)]
        [Required]
        public string CK_ID { get; set; }

        /// <summary>
        /// 收料仓库名称
        /// </summary>
        [StringLength(30)]
        [Required]
        public string CK_NAME { get; set; }

        /// <summary>
        /// 生产批号
        /// </summary>
        [StringLength(50)]
        public string PRDT_PH { get; set; }

        /// <summary>
        /// 箱码
        /// </summary>
        [StringLength(50)]
        [Required]
        public string DHTZD_TXM { get; set; }

        /// <summary>
        /// 检验单号
        /// </summary>
        [StringLength(30)]
        public string BYQC_ID { get; set; }

        /// <summary>
        /// 入库数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal RK_NUM { get; set; }

        /// <summary>
        /// 入库人员
        /// </summary>
        [StringLength(20)]
        public string JLUSER_NAME { get; set; }

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
