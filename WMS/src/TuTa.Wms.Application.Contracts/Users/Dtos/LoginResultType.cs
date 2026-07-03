using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.Users.Dtos
{
    public enum LoginResultType : byte
    {
        Success = 1,

        InvalidUserNameOrPassword = 2,

        NotAllowed = 3,

        LockedOut = 4,

        RequiresTwoFactor = 5
    }
}
