using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.ChkResultLists.ValueObjects
{
    [Owned]
    public class CheckInfoOfChkRsltList : ValueObject
    {
        private CheckInfoOfChkRsltList()
        {

        }

        public CheckInfoOfChkRsltList(
            string checkOrderCode,
            DateTime checkDate,
            string checkNo,
            EnumCheckType checkType,
            EnumCheckResult checkResult,
            decimal passCnt,
            string checkNoBeforeReCheck = null)
        {
            CheckOrderCode = Check.NotNullOrWhiteSpace(checkOrderCode, nameof(checkOrderCode));
            CheckDate = checkDate;
            CheckNo = Check.NotNullOrWhiteSpace(checkNo, nameof(checkNo));
            CheckType = checkType;
            CheckResult = checkResult;
            PassCnt = Check.Range(passCnt, nameof(passCnt), 0);
            CheckNoBeforeReCheck = checkNoBeforeReCheck; //WmsDomainHelper.NotWhiteSpaceCheck(checkNoBeforeReCheck, nameof(checkNoBeforeReCheck));
        }


        // 以下：复检后检测信息修改

        public virtual void ModifyCheckInfo(
            string checkOrderCode,
            DateTime checkDate,
            string checkNo,
            EnumCheckType checkType,
            EnumCheckResult checkResult,
            decimal passCnt,
            string checkNoBeforeReCheck = null)
        {
            CheckOrderCode = Check.NotNullOrWhiteSpace(checkOrderCode, nameof(checkOrderCode));
            CheckDate = checkDate;
            CheckNo = Check.NotNullOrWhiteSpace(checkNo, nameof(checkNo));
            CheckType = checkType;
            CheckResult = checkResult;
            PassCnt = Check.Range(passCnt, nameof(passCnt), 0);
            CheckNoBeforeReCheck = checkNoBeforeReCheck; //WmsDomainHelper.NotWhiteSpaceCheck(checkNoBeforeReCheck, nameof(checkNoBeforeReCheck));
        }

        /// <summary>
        /// 检验单号
        /// </summary>
        [StringLength(30)]
        [Required]
        public virtual string CheckOrderCode { get; private set; }

        /// <summary>
        /// 检验日期
        /// </summary>
        [Column(TypeName = "date")]
        [Required]
        public virtual DateTime CheckDate { get; private set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        [StringLength(40)]
        [Required]
        public virtual string CheckNo { get; private set; }

        /// <summary>
        /// 超期复检前的检验单号
        /// </summary>
        [StringLength(40)]
        public string CheckNoBeforeReCheck { get; private set; }

        /// <summary>
        /// 检验类型 
        /// 1(进料检验） 
        /// 2(半成品质检)  
        /// 3(无需检物料收料：第二期放在收料中间表中） 
        /// 4(超期复检)   
        /// 10(期初库存  期初ERP库存生成条码，当检验合格处理）
        /// </summary>
        public virtual EnumCheckType CheckType { get; private set; }

        /// <summary>
        /// 检验结论
        /// 1（合格入仓）  
        /// 2（不合格：第一期不合格不进入中间表）  
        /// 3（超筛代用：允许入仓，但需要车间特别注意）
        /// </summary>
        public virtual EnumCheckResult CheckResult { get; private set; }

        /// <summary>
        /// 检验合格放行数
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal PassCnt { get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { CheckOrderCode, CheckDate, CheckNo, CheckType, CheckResult, PassCnt };
        }
    }
}
