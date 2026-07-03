using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.Boxes.ValueObjects
{
    public class CellInfo : ValueObject
    {
        private CellInfo()
        {            
        }

        public CellInfo(Guid? cellId, string cellCode, string cellName)
        {
            if (cellCode != null && string.IsNullOrWhiteSpace(cellCode))
                throw new Exception($"cellCode的值无效");

            if (cellName != null && string.IsNullOrWhiteSpace(cellName))
                throw new Exception($"cellName的值无效");

            CellId = cellId;
            CellCode = cellCode;
            CellName = cellName;
        }


        public Guid? CellId { get; private set; }

        [StringLength(20)]
        public string CellCode { get; private set; }

        [StringLength(50)]  
        public string CellName { get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return [CellId, CellCode, CellName];
        }
    }
}
