using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpStockCheck:Entity<int>
    {
        private ErpStockCheck()
        {
        }

        public ErpStockCheck(
            string barcode,
            string boxCode,
            decimal count)
        {
            DHTZD_TXM = barcode;
            LXBH= boxCode;
            JNCJ_NUM = count;
            JNCJ_DATE = DateTime.Now;
            JS_STATE = false;
            JS_DATE = null;
            IFDELETE = false;
        }

        /// <summary>
        /// 条形码号
        /// </summary>
        [StringLength(50)]
        public string DHTZD_TXM { get; set; }

        /// <summary>
        /// 料箱编号
        /// </summary>
        [StringLength(50)]
        public string LXBH { get; set; }

        /// <summary>
        /// 抽检数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal JNCJ_NUM { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? JNCJ_DATE { get; set; }

        public bool JS_STATE { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? JS_DATE { get; set; }

        public bool IFDELETE { get; set; }
    }
}
