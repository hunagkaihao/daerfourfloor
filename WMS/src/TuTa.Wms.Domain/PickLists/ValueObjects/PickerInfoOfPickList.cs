using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.PickLists.ValueObjects
{
    public class PickerInfoOfPickList : ValueObject
    {
        private PickerInfoOfPickList()
        {            
        }

        public PickerInfoOfPickList(string deptCode, string deptName, string gysCode, string gysName, string pickManName = null)
        {
            //if (deptCode != null && string.IsNullOrWhiteSpace(deptCode))
            //    throw new Exception("deptCode的值无效，只能取null或者包含可见字符");

            //if (deptName != null && string.IsNullOrWhiteSpace(deptName))
            //    throw new Exception("deptName的值无效，只能取null或者包含可见字符");

            //if (gysCode != null && string.IsNullOrWhiteSpace(gysCode))
            //    throw new Exception("gysCode的值无效，只能取null或者包含可见字符");

            //if (gysName != null && string.IsNullOrWhiteSpace(gysName))
            //    throw new Exception("gysName的值无效，只能取null或者包含可见字符");

            //if ((!string.IsNullOrWhiteSpace(deptCode) ||
            //    !string.IsNullOrWhiteSpace(deptName)) &&
            //    (!string.IsNullOrWhiteSpace(gysCode) ||
            //    !string.IsNullOrWhiteSpace(gysName)))
            //    throw new Exception("领用部门和委外单位不能同时存在");

            DeptCode = deptCode;
            DeptName = deptName;
            GysCode = gysCode;
            GysName = gysName;
            PickManName = pickManName;
        }

        public void ModifyPickInfo(string deptCodeNew, string deptNameNew, string gysCodeNew, string gysNameNew)
        {
            //if (deptCodeNew != null && string.IsNullOrWhiteSpace(deptCodeNew))
            //    throw new Exception("deptCodeNew的值无效，只能取null或者包含可见字符");

            //if (deptNameNew != null && string.IsNullOrWhiteSpace(deptNameNew))
            //    throw new Exception("deptNameNew的值无效，只能取null或者包含可见字符");

            //if (gysCodeNew != null && string.IsNullOrWhiteSpace(gysCodeNew))
            //    throw new Exception("gysCodeNew的值无效，只能取null或者包含可见字符");

            //if (gysNameNew != null && string.IsNullOrWhiteSpace(gysNameNew))
            //    throw new Exception("gysNameNew的值无效，只能取null或者包含可见字符");

            //if ((!string.IsNullOrWhiteSpace(deptCodeNew) ||
            //    !string.IsNullOrWhiteSpace(deptNameNew)) &&
            //    (!string.IsNullOrWhiteSpace(gysCodeNew) ||
            //    !string.IsNullOrWhiteSpace(gysNameNew)))
            //    throw new Exception("领用部门和委外单位不能同时存在");

            DeptCode = deptCodeNew;
            DeptName = deptNameNew;
            GysCode = gysCodeNew;
            GysName = gysNameNew;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { DeptCode, DeptName, GysCode, GysName };
        }

        /// <summary>
        /// 领用部门编号
        /// </summary>
        [StringLength(30)]
        public string DeptCode { get; set; }

        /// <summary>
        /// 领用部门名称
        /// </summary>
        [StringLength(60)]
        public string DeptName { get; set; }

        /// <summary>
        /// 领用外协单位编号
        /// </summary>
        [StringLength(30)]
        public string GysCode { get; set; }

        /// <summary>
        /// 领用外协单位名称
        /// </summary>
        [StringLength(80)]
        public string GysName { get; set; }

        /// <summary>
        /// 领料人姓名，一般在无计划出库时指定，有计划时，领料人ERP自己能获取
        /// </summary>
        [StringLength(20)]
        public string PickManName { get; set; }
    }
}
