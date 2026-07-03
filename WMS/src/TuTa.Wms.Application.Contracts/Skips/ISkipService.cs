using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Skips.Dtos;

using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Skips
{
    public interface ISkipService:IApplicationService
    {
        public Task<ResponseDto> AddSkipAsync(SkipAddDto para);

        public Task<ResponseDto> SkipBindCellAsync(string skipCode, string cellCode, string podDir, string isBind);

        public Task<ResponseDto> SendSkipAsync(string skipCode, string cellCode);
        public Task<ResponseDto> CallSkipAsync(string skipCode, int areaId);

        public Task<PagedResultDto<SkipDto>> GetPagedSkips(PagedSkipDto para);

        public Task<PagedResultDto<SkipOutDto>> GetPagedSkipsOut();

        public Task<ResponseDto> SetNoHaveStatus(string skipCode);

        public Task<ResponseDto> ClearWallNoHaveBox();
        public Task<ResponseDto> SetReceipt(string skipCode);

    }
}
