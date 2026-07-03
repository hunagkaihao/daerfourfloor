using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Cells.Entities
{
    public class CellBox : Entity
    {
        private CellBox()
        {
        }

        public CellBox(
            Guid cellId,
            Guid boxId,
            string boxCode,
            string boxName,
            string boxTypeName,
            string boxSpecsName,
            int? length,
            int? width,
            int? height)
        {
            CellId = cellId;
            BoxId = boxId;
            BoxCode = Check.NotNullOrWhiteSpace(boxCode, nameof(boxCode));
            BoxName = Check.NotNullOrWhiteSpace(boxName, nameof(boxName));
            if (boxTypeName != null && string.IsNullOrWhiteSpace(boxTypeName))
                throw new Exception("boxTypeName的值无效");
            BoxTypeName = boxTypeName;
            if (boxSpecsName != null && string.IsNullOrWhiteSpace(boxSpecsName))
                throw new Exception("boxSpecsName值无效");
            SpecsName = boxSpecsName;

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

        public Guid CellId { get; private set; }

        public Guid BoxId { get; private set; }

        [StringLength(20)]
        public virtual string BoxCode { get; private set; }

        [StringLength(50)]
        public virtual string BoxName { get; private set; }

        [StringLength(50)]
        public virtual string BoxTypeName { get; private set; }

        [StringLength(30)]
        public string SpecsName { get; private set; }

        public int? Length { get; private set; }

        public int? Width { get; private set; }

        public int? Height { get; private set; }

        public override object[] GetKeys()
        {
            return new object[] { CellId, BoxId };
        }
    }
}
