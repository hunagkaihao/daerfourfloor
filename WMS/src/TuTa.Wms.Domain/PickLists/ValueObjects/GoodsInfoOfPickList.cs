using System;
using System.ComponentModel.DataAnnotations;

namespace TuTa.Wms.PickLists.ValueObjects
{
    public class GoodsInfoOfPickList
    {
        private GoodsInfoOfPickList()
        {            
        }

        public GoodsInfoOfPickList(string goodsCode, string goodsName, string goodsSpecs)
        {
            //if (goodsCode != null && string.IsNullOrWhiteSpace(goodsCode))
            //    throw new Exception("goodsCode的值无效，只能取null或者包含可见字符");

            //if (goodsName != null && string.IsNullOrWhiteSpace(goodsName))
            //    throw new Exception("goodsName的值无效，只能取null或者包含可见字符");

            //if (goodsSpecs != null && string.IsNullOrWhiteSpace(goodsSpecs))
            //    throw new Exception("goodsSpecs的值无效，只能取null或者包含可见字符");

            GoodsCode = goodsCode;
            GoodsName = goodsName;
            GoodsSpecs = goodsSpecs;
        }

        public void ModifyGoods(string goodsCodeNew, string goodsNameNew, string goodsSpecsNew)
        {
            if (goodsCodeNew != null && string.IsNullOrWhiteSpace(goodsCodeNew))
                throw new Exception("goodsCodeNew的值无效，只能取null或者包含可见字符");

            if (goodsNameNew != null && string.IsNullOrWhiteSpace(goodsNameNew))
                throw new Exception("goodsNameNew的值无效，只能取null或者包含可见字符");

            if (goodsSpecsNew != null && string.IsNullOrWhiteSpace(goodsSpecsNew))
                throw new Exception("goodsSpecsNew的值无效，只能取null或者包含可见字符");

            GoodsCode = goodsCodeNew;
            GoodsName = goodsNameNew;
            GoodsSpecs = goodsSpecsNew;
        }


        /// <summary>
        /// 成品编号
        /// </summary>
        [StringLength(30)]
        public string GoodsCode { get; set; }

        /// <summary>
        /// 成品名称
        /// </summary>
        [StringLength(130)]
        public string GoodsName { get; set;}

        /// <summary>
        /// 成品规格
        /// </summary>
        [StringLength(130)]
        public string GoodsSpecs { get; set;}
    }
}
