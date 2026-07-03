using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpMaterial : Entity
    {
        /// <summary>
        /// 该类不能创建，只能从中间表读取
        /// </summary>
        private ErpMaterial()
        {            
        }

        private static readonly object _locker = new object();

        public void SetIsReceived()
        {
            lock (_locker)
            {
                if (IFJSTWO == true)
                    return;

                IFJSTWO = true;
                JS_DATE = DateTime.Now;
            }
        }

        /// <summary>
        /// 物料编号，唯一
        /// </summary>
        [StringLength(20)]
        [Required]
        public string PRDT_ID { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        [StringLength(120)]
        public string PRDT_NAME { get; set; }

        /// <summary>
        /// 规格特性
        /// </summary>
        [StringLength(120)]
        public string PRDT_SPEC { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [StringLength(10)]
        public string PRDT_UNIT { get; set; }

        /// <summary>
        /// 类别编号 
        /// </summary>
        [StringLength(20)]
        public string PRDT_TYPE { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        [StringLength(60)]
        public string PTYPE_NAME { get; set; }

        /// <summary>
        /// 是否环保
        /// </summary>
        [StringLength(60)]
        public string PRDT_HB { get; set; }

        /// <summary>
        /// 安全库存
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? PRDT_LOW { get; set; }

        /// <summary>
        /// 保质期
        /// </summary>
        public int? PRDT_STOREDAYS { get; set; }

        /// <summary>
        /// 是否汽车配件
        /// </summary>
        public bool? IFQCPJ { get; set; }

        /// <summary>
        /// 是否符合PPAP
        /// </summary>
        public bool? IFPPAP { get; set; }

        /// <summary>
        /// WMS接收标识
        /// </summary>
        public bool? IFJSTWO { get; private set; }

        /// <summary>
        /// WMS接收时间
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


        /// <summary>
        /// 标准装箱数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal? PRDTZX_BZSHL { get; private set; }

        /// <summary>
        /// 标准装箱重量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal? PRDTZX_ZL { get; private set; }

        /// <summary>
        /// 拼箱类别
        /// </summary>
        [StringLength(50)]
        public virtual string PRDTPX_LB { get; private set; }

        public virtual bool PRDTPX_TAG { get; private set; }

        /// <summary>
        /// 成品列表
        /// </summary>
        [StringLength (450)]
        public virtual string CP_SM { get; private set; }

        public override object[] GetKeys()
        {
            return new object[] { PRDT_ID };
        }
    }
}
