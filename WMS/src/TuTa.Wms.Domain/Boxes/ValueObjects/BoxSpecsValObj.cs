using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.Boxes.ValueObjects
{
    public class BoxSpecsValObj : ValueObject
    {
        private BoxSpecsValObj()
        {            
        }

        public BoxSpecsValObj(string specsName, int? length, int? width, int? height)
        {
            if (specsName != null && string.IsNullOrWhiteSpace(specsName))
                throw new Exception("specsName值无效");
            SpecsName = specsName;

            if (length != null && length <= 0)
                throw new Exception("length值无效");
            Length = length;

            if (width != null && width <= 0)
                throw new Exception("width值无效");
            Width = width;

            if (height != null && height <= 0)
                throw new Exception("height值无效");
            Height = height;
        }

        /// <summary>
        /// 规格名称
        /// </summary>
        [StringLength(30)]
        public string SpecsName { get; set; }

        public int? Length { get; private set; }

        public int? Width { get; private set; }

        public int? Height { get; private set; }



        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { SpecsName, Length, Width, Height };
        }
    }
}
