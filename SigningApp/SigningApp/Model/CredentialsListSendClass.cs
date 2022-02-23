using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicentaApp.JsonClass
{
    public class CredentialsListSendClass
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int maxResults { get; set; } = 10;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string pageToken { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
        public string clientData { get; set; }
    }
}
