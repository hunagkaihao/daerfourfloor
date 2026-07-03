using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Skips.Aggregates
{
    public class Skip:AuditedAggregateRoot<Guid>
    {
        private Skip() { }

        internal Skip(Guid id,string skipCode,string skipName,int type):base(id)
        {
            SkipCode = skipCode;
            SkipName = skipName;
            Type = type;
            SkipStatus = SkipStatus.NoHave;
            SkipRunStatus = SkipRunStatus.Enable;
        }

        public virtual string SkipCode { get; set; }
        public virtual string SkipName { get; set; }
        public virtual Guid? CellId { get; set; }
        public virtual string CellCode { get; set; }
        public virtual int AreaId { get; set; }
        
        /// <summary>
        /// 1单排层，2多排层，3托盘料车
        /// </summary>
        public virtual int Type { get; set; }

        public virtual SkipStatus SkipStatus { get; set; }

        public virtual SkipRunStatus SkipRunStatus {  get; set; }

        public string TargetLocation { get; set; }

        public string TargetCellType { get; set; }
    }
}
