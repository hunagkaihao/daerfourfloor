using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.Users.Dtos
{
    public class AbpLoginResult
    {
        public AbpLoginResult(LoginResultType result)
        {
            Result = result;
        }

        public LoginResultType Result { get; }

        public string Description => Result.ToString();
    }
}
