using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.ChkResultLists.Entities
{
    public class ChkResultBox : Entity<int>
    {
        private ChkResultBox()
        {            
        }

        internal ChkResultBox(Guid chkResultListId, Guid boxId, string boxCode, string boxName, decimal countInBox)
        {
            ChkResultListId = chkResultListId;
            BoxId = boxId;
            BoxCode = Check.NotNullOrWhiteSpace(boxCode, nameof(boxCode));
            BoxName = Check.NotNullOrWhiteSpace(boxName, nameof(boxName));
            CountInBox = Check.Positive(countInBox, nameof(countInBox));
            Status = BoxAftChkStatus.Created;
            HouseId = null;
            HouseCode = null;
            HouseName = null;
            AreaId = null;
            AreaCode = null;
            AreaName = null;
            CellId = null;
            CellCode = null;
            CellName = null;
        }

        internal void BindToCell(
            Guid houseId, string houseCode, string houseName, 
            int? areaId, string areaCode, string areaName, 
            Guid cellId, string cellCode, string cellName)
        {
            HouseId = houseId;
            HouseCode = Check.NotNullOrWhiteSpace(houseCode, nameof(houseCode));
            HouseName = Check.NotNullOrWhiteSpace(houseName, nameof(houseName));
            AreaId = WmsDomainHelper.NotNegativeOrZeroCheck(areaId, nameof(areaId));
            AreaCode = WmsDomainHelper.NotWhiteSpaceCheck(areaCode, nameof(areaCode));
            AreaName = WmsDomainHelper.NotWhiteSpaceCheck(areaName, nameof(areaName));
            CellId = cellId;
            CellCode = Check.NotNullOrWhiteSpace(cellCode, nameof(cellCode));
            CellName = Check.NotNullOrWhiteSpace(cellName, nameof(cellName));
            Status = BoxAftChkStatus.BindedToCell;
        }

        internal void AddBindCount(decimal countToAdd)
        {
            CountInBox += countToAdd;
        }

        public Guid ChkResultListId { get; private set; }

        public Guid BoxId { get; private set; }

        [StringLength(20)]
        public string BoxCode { get; private set; }

        [StringLength(50)]
        public string BoxName { get; private set; }

        public Guid? HouseId { get; private set; }

        [StringLength(20)]
        public string HouseCode { get; private set; }

        [StringLength(50)]
        public string HouseName { get; private set; }

        public int? AreaId { get; private set; }

        [StringLength(20)]
        public string AreaCode { get; private set; }

        [StringLength(50)]
        public string AreaName { get; private set; }

        public Guid? CellId { get; private set; }

        [StringLength(20)]
        public string CellCode { get; set; }

        [StringLength(50)]
        public string CellName { get; set; }

        
        [Column(TypeName = "decimal(18,6)")]
        public decimal CountInBox { get; private set; }

        public BoxAftChkStatus Status { get; private set; }
    }
}
