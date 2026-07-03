using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.BarcodeLists.ValueObjects
{
    [Owned]
    public class CountInfoOfBarcodeList:ValueObject
    {
        private CountInfoOfBarcodeList()
        {

        }

        public CountInfoOfBarcodeList(
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
        /// 满箱包数
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
