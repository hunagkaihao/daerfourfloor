using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpDeptType:Entity
    {
        /// <summary>
        /// 该类不能创建，只能从中间表读取
        /// </summary>
        private ErpDeptType()
        {            
        }

        private static readonly object _locker = new object();

        public void SetIsReceived()
        {
            lock (_locker)
            {
                if (IFFP == true)
                    return;

                IFFP = true;
            }
        }

        public void SetIsNotReceived()
        {
            lock (_locker)
            {
                if (IFFP == false)
                    return;

                IFFP = false;
            }
        }

        /// <summary>
        /// 车间编号
        /// </summary>
        [StringLength(30)]
        [Required]
        public string DEPT_ID { get; set; }

        /// <summary>
        /// 车间名称
        /// </summary>
        [StringLength(50)]
        public string DEPT_NAME { get; set; }

        /// <summary>
        /// 类别编号
        /// </summary>
        [StringLength(60)]

        public string CLCHKLB_ID { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        [StringLength(60)]
        public string CLCHKLB_NAME { get; set; }

        /// <summary>
        /// 设置类型 0产品信息分类 1综合类
        /// </summary>
        public string CLCHKLB_TYPE { get; set; }

        /// <summary>
        /// 是否已分配
        /// </summary>
        public bool? IFFP { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { CLCHKLB_ID };
        }
    }
}
