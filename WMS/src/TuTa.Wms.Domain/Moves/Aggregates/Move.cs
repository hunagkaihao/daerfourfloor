using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Moves.Aggregates
{
    public class Move:AuditedAggregateRoot<Guid>
    {
        private Move()
        {

        }

        public Move(string moveCode,DateTime time,string checkNo,string code,string name,string specs,string unit,decimal count)
        {
            MoveCode = moveCode;
            MoveTime = time;
            CheckNo = checkNo;
            MaterialCode = code;
            MaterialName = name;
            MaterialSpecs = specs;
            MaterialUnit = unit;
            CountToMove = count;
            MoveCount = 0;
        }

        public void ModifyMove(DateTime time, string checkNo, string code, string name, string specs, string unit, decimal count)
        {
            MoveTime = time;
            CheckNo = checkNo;
            MaterialCode = code;
            MaterialName = name;
            MaterialSpecs = specs;
            MaterialUnit = unit;
            CountToMove = count;
        }

        /// <summary>
        /// 通知单号
        /// </summary>
        [StringLength(30)]
        [Required]
        public string MoveCode { get; private set; }

        /// <summary>
        /// 通知日期
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime MoveTime { get; private set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        [StringLength(30)]
        public string CheckNo { get; private set; }

        /// <summary>
        /// 物料编号
        /// </summary>
        [StringLength(20)]
        public string MaterialCode { get; private set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        [StringLength(120)]
        public string MaterialName { get; private set; }

        /// <summary>
        /// 规格特性
        /// </summary>
        [StringLength(120)]
        public string MaterialSpecs { get; private set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [StringLength(10)]
        public string MaterialUnit { get; private set; }

        /// <summary>
        /// 调入数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal CountToMove { get; private set; }

        /// <summary>
        /// 已调拨数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal MoveCount { get; set; }
    }
}
