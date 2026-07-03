using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TuTa.Wms.Skips.Aggregates;

namespace TuTa.Wms.Skips
{
    public class SkipManager:WmsDomainService
    {
        private readonly ISkipRepository _skipRepository;

        public SkipManager (ISkipRepository skipRepository)
        {
            _skipRepository = skipRepository;
        }

        public async Task<Skip> CreateSkipAsync(string skipCode,string skipName,int type)
        {
            var skipExist = await _skipRepository.FindBySkipCodeAsync(skipCode);
            if (skipExist != null)
                throw new Exception($"编号为{skipCode}的料车已存在");

            Skip skip = new Skip(GuidGenerator.Create(), skipCode, skipName, type);
            return skip;
        }
    }
}
