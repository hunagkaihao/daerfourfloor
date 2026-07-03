using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.Users.Dtos
{
    public class LoginOutput
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string UserName { get; set; }

        public string Token { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
    }
}
