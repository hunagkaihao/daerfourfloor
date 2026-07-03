using System;
using System.Threading.Tasks;
using TuTa.Wms.RecheckLists.Aggregates;
using TuTa.Wms.RecheckLists.ValueObjects;

namespace TuTa.Wms.RecheckLists
{
    public class RecheckListManager : WmsDomainService
    {
        private readonly IRecheckListRepository _recheckRepository;

        public RecheckListManager(IRecheckListRepository recheckRepository)
        {
            _recheckRepository = recheckRepository;
        }

        public async Task<RecheckList> CreateReCheckListAsync(string reCheckListCode, DateTime reCheckListDate)
        {
            var reCheckListExist = await _recheckRepository.FindByReCheckListCodeAsync(reCheckListCode).ConfigureAwait(false);
            if (reCheckListExist != null)
                throw new Exception($"单号为{reCheckListCode}的复检单已经存在");

            RecheckList recheckList = new RecheckList(reCheckListCode, reCheckListDate);
            return recheckList;
        }

        public async Task AddRecheckItemAsync(
            RecheckList recheckListToAdd,
            string checkNo,
            string barcode,
            MaterialInfoOfRechkList material,
            decimal checkCount,
            int? reCheckTimes = null,
            DateTime? expiryLimitDate = null)
        {
            var recheckLists = await _recheckRepository.GetAllRecheckListsAsync().ConfigureAwait(false);
            if (recheckLists != null && recheckLists.Count > 0) 
            {
                foreach(var recheckList in recheckLists)
                {
                    var recheckItems = recheckList.RecheckItems;
                    if (recheckItems == null || recheckItems.Count == 0)
                        continue;
                    foreach(var recheckItem in recheckItems)
                    {
                        if (recheckItem.Barcode == barcode)
                            throw new Exception($"收料码为{barcode}的复检项在复检单{recheckList.RecheckListCode}中已经存在");
                    }
                }
            }

            recheckListToAdd.AddRecheckItem(checkNo, barcode, material, checkCount, reCheckTimes, expiryLimitDate);
        }
    }
}
