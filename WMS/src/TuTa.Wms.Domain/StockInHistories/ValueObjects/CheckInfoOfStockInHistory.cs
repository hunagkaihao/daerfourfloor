using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.StockInHistories.ValueObjects
{
    [Owned]
    public class CheckInfoOfStockInHistory : ValueObject
    {
        private CheckInfoOfStockInHistory()
        {
        }

        public CheckInfoOfStockInHistory(
            string checkOrderCode,
            string checkNo,
            DateTime? checkDate,
            string checkResult)
        {           
            CheckOrderCode = checkOrderCode;
            CheckNo = checkNo;
            CheckDate = checkDate;
            CheckResult = checkResult;
        }

        /// <summary>
        /// 检验单号
        /// </summary>
        [StringLength(30)]
        public virtual string CheckOrderCode { get; private set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        [StringLength(40)]
        public virtual string CheckNo { get; private set; }

        /// <summary>
        /// 检验日期
        /// </summary>
        [Column(TypeName = "date")]
        public virtual DateTime? CheckDate { get; private set; }

        /// <summary>
        /// 检验结论
        /// 1（合格入仓）  
        /// 2（不合格：第一期不合格不进入中间表）  
        /// 3（超筛代用：允许入仓，但需要车间特别注意）
        /// </summary>
        public virtual string CheckResult { get; private set; }


        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { CheckOrderCode, CheckNo, CheckResult };
        }
    }
}
