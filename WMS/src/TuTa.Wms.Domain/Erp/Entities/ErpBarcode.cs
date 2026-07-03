using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpBarcode : Entity
    {

        private ErpBarcode()
        {

        }

        private static readonly object _locker = new object();

        public void SetIsReceived()
        {
            lock (_locker)
            {
                if (IFJS == true) return;

                IFJS = true;
                JS_DATE = DateTime.Now;
            }
        }

        /// <summary>
        /// 收料条形码
        /// </summary>
        [StringLength(30)]
        [Required]
        public string DHTZD_TXM { get; private set; }

        /// <summary>
        /// 收料单号
        /// </summary>
        //[StringLength(40)]
        //public string DHTZD_ID { get; private set; }

        /// <summary>
        /// 采购单号
        /// </summary>
        [StringLength(40)]
        public string BYORD_ID { get; private set; }

        /// <summary>
        /// 供应商编号
        /// </summary>
        [StringLength(20)]
        public string GYS_ID { get; private set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        [StringLength(120)]
        public string GYS_NAME { get; private set; }

        /// <summary>
        /// 收料日期
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime? DHTZD_DATE { get; private set; }

        /// <summary>
        /// 供应商生产批号(目前只有压电片有)
        /// </summary>
        [StringLength(40)]
        public string GYSQC_PH { get; private set; }

        /// <summary>
        /// 物料编号
        /// </summary>
        [StringLength(20)]
        public string PRDT_ID { get; private set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        [StringLength(120)]
        public string PRDT_NAME { get; private set; }

        /// <summary>
        /// 规格特性
        /// </summary>
        [StringLength(120)]
        public string PRDT_SPEC { get; private set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [StringLength(10)]
        public string PRDT_UNIT { get; private set; }

        /// <summary>
        /// 收料仓编号,01 综合库（正常材料仓）  26：(采购暂存库)  04 (待处理)
        /// </summary>
        [StringLength(10)]
        public string CK_ID { get; private set; }

        /// <summary>
        /// 收料仓名称
        /// </summary>
        [StringLength(30)]
        public string CK_NAME { get; private set; }

        /// <summary>
        /// 是否需要检验，1需要2不需要
        /// </summary>
        public int IFQC_TAG { get; private set; }

        /// <summary>
        /// 收料时的总数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal DHTZD_NUM { get; private set; }

        /// <summary>
        /// 收料时的包或箱数
        /// </summary>
        public int? DHTZD_XS { get; private set; }

        /// <summary>
        /// 最小包装中的物料数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? DHTZD_DJSHL { get; private set; }

        /// <summary>
        /// 入库类型  1(正常采购） 4(委托加工） 7(超期复检） 18 车间退货入仓
        /// </summary>
        public int RK_TYPE { get; private set; }

        /// <summary>
        /// 生产批号
        /// </summary>
        [StringLength(180)]
        public string SCAP_ID { get; private set; }

        /// <summary>
        /// 备料单号
        /// </summary>
        [StringLength(30)]
        public string OPBLD_ID { get; private set; }

        /// <summary>
        /// 备货单号
        /// </summary>
        [StringLength(30)]
        public string OPBHD_ID { get; private set; }

        /// <summary>
        /// WMS是否已经接收
        /// </summary>
        public virtual bool? IFJS { get; private set; }

        /// <summary>
        /// WMS接收的时间
        /// </summary>
        [Column(TypeName = "datetime")]
        public virtual DateTime? JS_DATE { get; private set; }

        /// <summary>
        /// 删除或停用标志
        /// </summary>
        public virtual bool? IFDELETE { get; private set; }

        /// <summary>
        /// 接收时WMS给ERP的说明信息
        /// </summary>
        [StringLength(150)]
        public string JS_SM { get; private set; }

        /// <summary>
        /// 模号
        /// </summary>
        [StringLength(60)]
        public string PRDT_MH { get; private set; }



        public override object[] GetKeys()
        {
            return new object[] { DHTZD_TXM };
        }
    }
}
