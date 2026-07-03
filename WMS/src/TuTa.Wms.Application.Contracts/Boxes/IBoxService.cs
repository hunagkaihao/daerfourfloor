using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Boxes.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Boxes
{
    public interface IBoxService : IApplicationService
    {
        //添加单个容器
        public Task<ResponseDto> AddBoxAsync(BoxAddDto para);

        //添加批量容器
        public Task<ResponseDto> AddBoxListAsync(List<BoxAddDto> paras);

        //删除指定容器
        public Task<ResponseDto> DelBoxAsync(string boxCode);

        //删除所有容器
        public Task<ResponseDto> DelAllBoxesAsync();

        //更新容器信息
        public Task<ResponseDto> UpdateBoxAsync(Guid boxId, BoxUpdateDto para);

        //查询容器
        public Task<PagedResultDto<BoxDto>> PagedBoxesGetAsync(PagedBoxesQueryDto para);
    }
}
