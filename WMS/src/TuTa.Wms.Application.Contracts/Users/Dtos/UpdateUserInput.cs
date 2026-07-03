using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Identity;

namespace TuTa.Wms.Users.Dtos
{
    public class UpdateUserInput
    {
        public Guid UserId { get; set; }

        public IdentityUserUpdateDto UserInfo { get; set; }
    }
}
