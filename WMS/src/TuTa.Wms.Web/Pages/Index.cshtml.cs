using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace TuTa.Wms.Web.Pages;

public class IndexModel : WmsPageModel
{
    public void OnGet()
    {

    }

    public async Task OnPostLoginAsync()
    {
        await HttpContext.ChallengeAsync("oidc");
    }
}
