using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpStockAftChk : Entity
    {
        private ErpStockAftChk()
        {
            
        }

        private static readonly object _locker = new object();

        public void SetIsReceived()
        {
            lock(_locker)
            {
                if (IFJS == true) return;

                IFJS = true;
                JS_DATE = DateTime.Now;
            }
        }

        /// <summary>
        /// 入库条形码
        /// </summary>
        [StringLength(30)]
        [Required]
        public string DHTZD_TXM { get; private set; }

        /// <summary>
        /// 检验单号
        /// </summary>
        [StringLength(30)]
        public string BYQC_ID { get; private set; }

        /// <summary>
        /// 检验日期
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime? BYQC_DATE { get; private set; }

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
        /// 收料仓编号
        /// </summary>
        [StringLength(10)]
        public string CK_ID { get; private set; }

        /// <summary>
        /// 收料仓名称
        /// </summary>
        [StringLength(30)]
        public string CK_NAME { get; private set; }

        /// <summary>
        /// 收料时的总数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal DHTZD_NUM { get; private set; }

        /// <summary>
        /// 检验合格放行数
        /// 1合格入库： 收料数量=合格数量  2 不合格 有收料数，放行数量为0   3 筛选代用：收料数量大于合格放行数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal QCPASS_NUM { get; private set; }

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
        /// 检验编号
        /// </summary>
        [StringLength(40)]
        public string QCPRDT_PH { get; private set; }

        /// <summary>
        /// 检验结论
        /// 1（合格入仓）  2（不合格）  3（筛选代用：允许入仓，但需要车间特别注意）
        /// </summary>
        public int QCJL { get; private set; }

        /// <summary>
        /// 检验类型 
        /// 1进料检验  4超期复检 
        /// </summary>
        public int QC_TYPE { get; private set; }

        /// <summary>
        /// 入库类型  
        /// 1(正常采购）  4(委托加工） 7(超期复检）  18 车间退货入仓
        /// </summary>
        public int? RK_TYPE { get; private set; }

        /// <summary>
        /// 超期复检前的检验单号
        /// </summary>
        [StringLength(40)]
        public string OLDQCPRDT_PH { get; private set; }

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
        public bool? IFJS { get; private set; }

        /// <summary>
        /// WMS接收的时间
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime? JS_DATE { get; private set; }

        /// <summary>
        /// 删除或停用标志
        /// </summary>
        public bool? IFDELETE { get; private set; }

        /// <summary>
        /// 接收时WMS给ERP的说明信息
        /// </summary>
        [StringLength(150)]
        public string JS_SM { get; private set; }

        public override object[] GetKeys()
        {
            return new object[] { DHTZD_TXM, QC_TYPE };
        }
    }
}
