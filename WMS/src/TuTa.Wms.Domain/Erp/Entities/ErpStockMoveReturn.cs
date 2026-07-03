using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpStockMoveReturn : Entity<int>
    {
        private ErpStockMoveReturn()
        {            
        }

        public ErpStockMoveReturn(
            DateTime moveDate,
            string gysCode, string gysName,
            string materialCode, string materialName, string specs, string unit,
            string checkNo, 
            string barcode,
            decimal moveCount,
            int moveType,
            string operatorName)
        {
            DBD_DATE = moveDate;
            GYS_ID = gysCode;
            GYS_NAME = gysName;
            PRDT_ID = Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            PRDT_NAME = Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            PRDT_SPEC = specs;
            PRDT_UNIT = unit;
            PRDT_PH = Check.NotNullOrWhiteSpace(checkNo, nameof(checkNo));
            DHTZD_TXM = Check.NotNullOrWhiteSpace(barcode, nameof(barcode));
            DBD_NUM = Check.Positive(moveCount, nameof(moveCount));
            DBD_TYPE = MoveTypeCheck(moveType, nameof(moveType));
            JLUSER_NAME = operatorName;
            IFJS = false;
            JS_DATE = null;
            IFDELETE = false;
            JS_SM = null;
        }

        private int MoveTypeCheck(int moveType, string parameterName)
        {
            if (moveType != 1 && moveType != 2 && moveType != 3 && moveType != 4)
                throw new Exception($"调拨类型取值无效，调拨类型可取值为：1、2、3、4");
            return moveType;
        }

        /// <summary>
        /// 调拨日期
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime DBD_DATE { get; set; }

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
        /// 调拨数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal DBD_NUM { get; set; }

        /// <summary>
        /// 调拨类型
        /// 1 暂存调正常  2正常调暂存
        /// </summary>
        public int DBD_TYPE { get; set; }

        /// <summary>
        /// 调拨人员
        /// </summary>
        [StringLength(20)]
        public string JLUSER_NAME { get; set; }

        public bool IFJS { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? JS_DATE { get; set; }

        public bool IFDELETE { get; set; }

        public string JS_SM { get; set; }
    }
}
