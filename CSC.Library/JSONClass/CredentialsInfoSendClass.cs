using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicentaApp.JsonClass
{
    public class CredentialsInfoSendClass
    {
        public string credentialID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string certificates { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool certInfo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool authInfo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string lang { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string clientData { get; set; }
    }
}
