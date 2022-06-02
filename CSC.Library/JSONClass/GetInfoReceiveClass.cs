using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicentaApp.JsonClass
{
    public class GetInfoReceiveClass
    {
        public string specs { get; set; }
        public string name { get; set; }
        public string logo { get; set; }
        public string region { get; set; }
        public string lang { get; set; }

        public string description { get; set; }

        public List<string> authType { get; set; }

        public string oauth2 { get; set; }

        public List<string> methods { get; set; }

}
}
