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
    public class ErpMove : Entity
    {
        //暂存调正常

        private ErpMove()
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
        /// 通知单号
        /// </summary>
        [StringLength(30)]
        [Required]
        public string ZCDBD_ID { get; private set; }

        /// <summary>
        /// 通知日期
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime ZCDBD_DATE { get; private set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        [StringLength(30)]
        public string PRDT_PH { get; private set; }

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
        /// 调入数量
        /// </summary>
        //[Column(TypeName = "decimal(18,6)")]
        public int ZCDB_NUM { get; private set; }

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



        public override object[] GetKeys()
        {
            return new object[] { ZCDBD_ID };
        }
    }
}
