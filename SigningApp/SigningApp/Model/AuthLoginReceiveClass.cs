using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LicentaApp.JsonClass
{
    public class AuthLoginReceiveClass
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }

        public int expires_in { get; set; } = 3600;
    }
}
