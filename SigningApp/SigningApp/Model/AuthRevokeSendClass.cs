using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicentaApp.JsonClass
{
    public class AuthRevokeSendClass
    {
        public string token { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string token_type_hint { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string clientData { get; set; }
    }
}
