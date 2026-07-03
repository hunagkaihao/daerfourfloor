using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpDeptTypeDetail : Entity
    {
        /// <summary>
        /// 该类不能创建，只能从中间表读取
        /// </summary>
        private ErpDeptTypeDetail()
        {
        }

        /// <summary>
        /// ID,无用
        /// </summary>
        [Required]
        public int ID { get; set; }

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
        /// 产品信息
        /// </summary>
        public string PRDT_ID { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string PRDT_NAME { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string PRDT_SPEC { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public string PRDT_UNIT { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { CLCHKLB_ID };
        }
    }
}
