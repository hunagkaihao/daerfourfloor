using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.ChkResultLists.ValueObjects
{
    [Owned]
    public class CountInfoOfChkRsltList : ValueObject
    {
        private CountInfoOfChkRsltList()
        {

        }

        public CountInfoOfChkRsltList(
            decimal receiveTotalCount,
            int? receivePkgOrBoxCount,
            decimal? countInOnePkgOrBox)
        {
            Check.Positive(receiveTotalCount, nameof(receiveTotalCount));
            if (receivePkgOrBoxCount != null && receivePkgOrBoxCount < 0)
                throw new Exception("receivePkgOrBoxCount的值无效");
            if (countInOnePkgOrBox != null && countInOnePkgOrBox < 0)
                throw new Exception("countInOnePkgOrBox的值无效");

            //if (receivePkgOrBoxCount != null && countInOnePkgOrBox != null)
            //{
            //    if (receiveTotalCount < receivePkgOrBoxCount * countInOnePkgOrBox)
            //        throw new ArgumentException($"收料总数量{receiveTotalCount}小于包箱数{receivePkgOrBoxCount}与最小包装内物料数{countInOnePkgOrBox}的乘积");
            //}

            ReceiveTotalCount = receiveTotalCount;
            ReceivePkgOrBoxCount = receivePkgOrBoxCount;
            CountInOnePkgOrBox = countInOnePkgOrBox;
        }

        /// <summary>
        /// 收料时的总数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal ReceiveTotalCount { get; set; }

        /// <summary>
        /// 收料时的包或箱数
        /// </summary>
        public virtual int? ReceivePkgOrBoxCount { get; set; }

        /// <summary>
        /// 最小包装中的物料数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal? CountInOnePkgOrBox { get; set; }


        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {
                ReceiveTotalCount,
                ReceivePkgOrBoxCount,
                CountInOnePkgOrBox };
        }
    }
}
