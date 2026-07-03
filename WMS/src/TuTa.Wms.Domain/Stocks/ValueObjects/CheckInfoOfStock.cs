using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TuTa.Wms.ChkResultLists;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.Stocks.ValueObjects
{
    [Owned]
    public class CheckInfoOfStock : ValueObject
    {
        private CheckInfoOfStock()
        {

        }

        public CheckInfoOfStock(
            string checkOrderCode = null,
            DateTime? checkDate = null,
            string checkNo = null,
            string checkNoBeforeReCheck = null,
            EnumCheckType? checkType = null,
            EnumCheckResult? checkResult = null,
            decimal? passCnt = null)
        {
            //if (checkOrderCode != null && string.IsNullOrEmpty(checkOrderCode)) //checkOrderCode不能为空格
            //    throw new Exception("checkOrderCode的值无效");

            //if (checkNo != null && string.IsNullOrEmpty(checkNo))
            //    throw new Exception("checkNo的值无效");

            //if (checkNoBeforeReCheck != null && string.IsNullOrEmpty(checkNoBeforeReCheck))
            //    throw new Exception("checkNoBeforeReCheck的值无效");

            if (passCnt != null && passCnt < 0)
                throw new Exception($"passCnt: {passCnt}无效，只能为null, 0及0以上");

            if (checkDate != null && (checkNo == null || checkType == null || checkResult == null || passCnt == null))
                throw new Exception($"存在检验日期，但检验编号、检验类型、检验结果或合格通过数的信息为null");


            CheckOrderCode = checkOrderCode;
            CheckDate = checkDate;
            CheckNo = checkNo;
            CheckNoBeforeReCheck = checkNoBeforeReCheck;
            CheckType = checkType;
            CheckResult = checkResult;
            PassCnt = passCnt;
        }


        // 以下：复检后检测信息修改
        internal virtual void ModifyCheckInfo(
            string checkOrderCode,
            DateTime checkDate,
            string checkNo,
            EnumCheckType checkType,
            EnumCheckResult checkResult,
            decimal passCnt)
        {
            Check.NotNullOrWhiteSpace(checkOrderCode, nameof(checkOrderCode));
            Check.NotNullOrWhiteSpace(checkNo, nameof(checkNo));

            if (passCnt < 0)
                throw new Exception($"passCnt: {passCnt}无效");

            CheckNoBeforeReCheck = CheckNo;
            CheckOrderCode = checkOrderCode;
            //CheckDate = checkDate;  //客户说：复检后的检验日期不用改
            CheckNo = checkNo;
            CheckType = checkType;
            CheckResult = checkResult;
            PassCnt = PassCnt == null ? passCnt : PassCnt; //库存的检验通过数为第一次检验的通过数，复检的通过数不计入内
        }

        public string CheckTypeInChs()
        {
            if (CheckType == null)
                return "未检验";
            else
                return CheckTypeHelper.CheckTypeToChinese(CheckType.Value);
        }

        public string CheckResultInChs()
        {
            if (CheckResult == null)
                return "未检验";
            else
                return CheckResultHelper.CheckResultToChinese(CheckResult.Value);
        }

        /// <summary>
        /// 检验单号
        /// </summary>
        [StringLength(30)]
        public virtual string CheckOrderCode { get; private set; }

        /// <summary>
        /// 检验日期
        /// </summary>
        [Column(TypeName = "datetime")]
        public virtual DateTime? CheckDate { get; private set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        [StringLength(40)]
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
        public virtual EnumCheckType? CheckType { get; private set; }

        /// <summary>
        /// 检验结论
        /// 1（合格入仓）  
        /// 2（不合格：第一期不合格不进入中间表）  
        /// 3（超筛代用：允许入仓，但需要车间特别注意）
        /// </summary>
        public virtual EnumCheckResult? CheckResult { get; private set; }

        /// <summary>
        /// 检验合格放行数
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal? PassCnt { get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { CheckOrderCode, CheckDate, CheckNo, CheckNoBeforeReCheck, CheckType, CheckResult, PassCnt };
        }
    }
}
