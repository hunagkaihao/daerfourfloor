using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpRecheckNotifier : Entity
    {
        private ErpRecheckNotifier()
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
        /// 超期复检通知单号
        /// </summary>
        [StringLength(30)]
        [Required]
        public string CKFQTZD_ID { get; private set; }

        /// <summary>
        /// 复检通知日期
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime CKFQTZD_DATE { get; private set; }

        /// <summary>
        /// 收料条形码
        /// </summary>
        [StringLength(30)]
        [Required]
        public string DHTZD_TXM { get; private set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        [StringLength(30)]
        [Required]
        public string PRDT_PH { get; private set; }

        /// <summary>
        /// 物料编号
        /// </summary>
        [StringLength(20)]
        [Required]
        public string PRDT_ID { get; private set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        [StringLength(120)]
        [Required]
        public string PRDT_NAME { get; private set; }

        /// <summary>
        /// 物料规格
        /// </summary>
        [StringLength(120)]
        public string PRDT_SPEC { get; private set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [StringLength(10)]
        public string PRDT_UNIT { get; private set; }

        /// <summary>
        /// 保质期天数
        /// </summary>
        public int? PRDT_STOREDAYS { get; private set; }

        /// <summary>
        /// 保质期限
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime? PRDT_DATE { get; private set; }

        /// <summary>
        /// 复检次数
        /// </summary>
        public int? FQXH { get; private set; }

        /// <summary>
        /// 复检数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal FQNUM { get; private set; }

        /// <summary>
        /// 复检抽出数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal FQCQ_NUM { get; private set; }

        /// <summary>
        /// WMS是否已接收
        /// </summary>
        public bool? IFJS { get; private set; }

        /// <summary>
        /// 接收日期
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime? JS_DATE { get; private set; }

        /// <summary>
        /// 是否停用/删除
        /// </summary>
        public bool? IFDELETE { get; private set; }

        /// <summary>
        /// 接收时WMS给ERP的说明信息
        /// </summary>
        [StringLength(150)]
        public string JS_SM { get; private set; }

        public override object[] GetKeys()
        {
            return [CKFQTZD_ID, DHTZD_TXM];
        }
    }
}
