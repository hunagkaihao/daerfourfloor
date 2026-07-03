using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Users.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Users
{
    public interface IAccountService : IApplicationService
    {
        //Task<ResponseDto> CreateStockAndBindToCellAsync(StockCreateAndBindToCellDto para);
        Task<LoginOutput> LoginAsync(LoginInput input);
        //Task<AbpLoginResult> Login(UserLoginInfo login);
    }
}
