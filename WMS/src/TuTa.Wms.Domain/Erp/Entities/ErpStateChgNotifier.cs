using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpStateChgNotifier : Entity
    {
        private ErpStateChgNotifier()
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
            lock (_locker)
            {
                JS_SM = info;
            }
        }

        /// <summary>
        /// 变更通知单号
        /// </summary>
        [StringLength(30)]
        [Required]
        public string CKZTCHANG_ID { get; private set; }

        /// <summary>
        /// 变更日期
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime CKZTCHANG_DATE { get; private set; }

        /// <summary>
        /// 库位区域 01：正常区域  04：待处理区
        /// </summary>
        [StringLength(50)]
        [Required]
        public string CK_ID { get; private set; }

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
        /// 变更前状态  0 正常 1冻结
        /// </summary>
        public int OLDKCZT_STATE { get; private set; }

        /// <summary>
        /// 变更后状态  0正常 1冻结
        /// </summary>
        public int NEWKCZT_STATE { get; private set; }

        /// <summary>
        /// 申请人
        /// </summary>
        [StringLength(20)]
        public string JLUSER_NAME { get; private set; }

        /// <summary>
        /// 申请部门
        /// </summary>
        [StringLength(20)]
        public string CKZTCHANG_DEPT { get; private set; }

        public bool? IFJS { get; private set; }

        [Column(TypeName = "datetime")]
        public DateTime? JS_DATE { get; private set; }

        public bool? IFDELETE { get; private set; }

        /// <summary>
        /// 接收时WMS给ERP的说明信息
        /// </summary>
        [StringLength(80)]
        public string JS_SM { get; private set; }


        public override object[] GetKeys()
        {
            return [CKZTCHANG_ID];
        }
    }
}
