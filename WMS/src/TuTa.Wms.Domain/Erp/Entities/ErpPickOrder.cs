using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpPickOrder : Entity
    {
        private ErpPickOrder()
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

        public void SetInfo(string info)
        {
            lock(_locker)
            {
                JS_SM = info;
            }
        }

        /// <summary>
        /// 领料通知单号
        /// </summary>
        [StringLength(30)]
        [Required]
        public string CHKTZD_ID { get; set; }

        /// <summary>
        /// 领料通知日期
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime CHKTZD_DATE { get; set; }

        /// <summary>
        /// 领用类型
        /// 1 生产领用  2外协领用 14生产领用2（非生产车间领用)  11试样领用   19退供应商   
        /// </summary>
        public int CHKTZD_TYPE { get; set; }

        /// <summary>
        /// 领用部门编号
        /// </summary>
        [StringLength(30)]
        public string CHKTZD_DEPT { get; set; }

        /// <summary>
        /// 领用部门名称
        /// </summary>
        [StringLength(60)]
        public string CHKTZDDEPT_NAME { get; set; }

        /// <summary>
        /// 外协领用加工单位编号
        /// </summary>
        [StringLength(30)]
        public string CHKTZD_GYS { get; set; }

        /// <summary>
        /// 外协领用加工单位名称
        /// </summary>
        [StringLength(80)]
        public string CHKTZDGYS_NAME { get; set; }

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
        [Required]
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
        /// 物料计量单位
        /// </summary>
        [StringLength(10)]
        public string PRDT_UNIT { get; set; }

        /// <summary>
        /// 领用数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal CHKTZD_NUM { get; set; }

        /// <summary>
        /// 领用检验编号，非空时指定该检验编号领用
        /// </summary>
        [StringLength(50)]
        public string PRDT_PH { get; set; }

        public bool? IFJS { get; set; }


        [Column(TypeName = "datetime")]
        public DateTime? JS_DATE { get; set; }

        /// <summary>
        /// 是否停用/删除
        /// </summary>
        public bool? IFDELETE { get; set; }

        /// <summary>
        /// 接收时WMS给ERP的说明信息
        /// </summary>
        [StringLength(150)]
        public string JS_SM { get; private set; }

        public override object[] GetKeys()
        {
            return [CHKTZD_ITM];
        }
    }
}
