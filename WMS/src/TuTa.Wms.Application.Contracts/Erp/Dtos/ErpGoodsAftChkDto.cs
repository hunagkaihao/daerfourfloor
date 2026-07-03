using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Erp.Dtos
{
    public class ErpGoodsAftChkDto : EntityDto
    {
        public virtual string DHTZD_TXM { get; set; }

        /// <summary>
        /// 入库类型  1(正常采购） 2（生产入库：指半成品） 4(委托加工） 7(超期复检）
        /// </summary>
        public virtual int RK_TYPE { get; set; }

        /// <summary>
        /// 是否已经使用
        /// </summary>
        public virtual bool IFUSED { get; set; }

        /// <summary>
        /// 使用时间
        /// </summary>
        public virtual string USED_DATE { get; set; }

        /// <summary>
        /// 删除或停用标志
        /// </summary>
        public virtual bool IFDELETE { get; set; }

        /// <summary>
        /// 物料码
        /// </summary>
        public virtual string PRDT_ID { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public virtual string PRDT_NAME { get; set; }

        /// <summary>
        /// 规格特性
        /// </summary>
        public virtual string PRDT_SPEC { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public virtual string PRDT_UNIT { get; set; }

        /// <summary>
        /// 收料时的总数量
        /// </summary>
        public virtual int DHTZD_NUM { get; set; }

        /// <summary>
        /// 收料时的包或箱数
        /// </summary>
        public virtual int DHTZD_XS { get; set; }

        /// <summary>
        /// 收料时的零散数量
        /// </summary>
        public virtual int DHTZD_LSNUM { get; set; }

        /// <summary>
        /// 最小包装中的物料数量
        /// </summary>
        public virtual int DHTZD_DJSHL { get; set; }

        /// <summary>
        /// 使用了的箱或包数
        /// </summary>
        public virtual int DHTZD_XSUSED { get; set; }

        /// <summary>
        /// 使用了的零散数量
        /// </summary>
        public virtual int DHTZD_LSNUMUSED { get; set; }

        /// <summary>
        /// 检验单号
        /// </summary>
        public virtual string BYQC_ID { get; set; }

        /// <summary>
        /// 检验日期
        /// </summary>
        public virtual string BYQC_DATE { get; set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        public virtual string QCPRDT_PH { get; set; }

        /// <summary>
        /// 超期复检前的检验单号
        /// </summary>
        public virtual string OLDQCPRDT_PH { get; set; }

        /// <summary>
        /// 检验类型 
        /// 1(进料检验） 
        /// 2(半成品质检)  
        /// 3(无需检物料收料：第二期放在收料中间表中） 
        /// 4(超期复检)   
        /// 10(期初库存  期初ERP库存生成条码，当检验合格处理）
        /// </summary>
        public virtual int QC_TYPE { get; set; }

        /// <summary>
        /// 检验结论
        /// 1（合格入仓）  
        /// 2（不合格：第一期不合格不进入中间表）  
        /// 3（超筛代用：允许入仓，但需要车间特别注意）
        /// </summary>
        public virtual int QCJL { get; set; }

        /// <summary>
        /// 检验合格放行数
        /// </summary>
        public virtual int QCPASS_NUM { get; set; }

        /// <summary>
        /// 供应商编号
        /// </summary>
        public virtual string GYS_ID { get; set; }


        /// <summary>
        /// 供应商名称
        /// </summary>
        public virtual string GYS_NAME { get; set; }

        /// <summary>
        /// 收料仓编号
        /// </summary>
        public virtual string CK_ID { get; set; }

        /// <summary>
        /// 收料仓名称
        /// </summary>
        public virtual string CK_NAME { get; set; }

        /// <summary>
        /// 生产批号
        /// </summary>
        public virtual string SCAP_ID { get; set; }

        /// <summary>
        /// 备料单号
        /// </summary>
        public virtual string OPBLD_ID { get; set; }

        /// <summary>
        /// 备货单号
        /// </summary>
        public virtual string OPBHD_ID { get; set; }
    }
}
