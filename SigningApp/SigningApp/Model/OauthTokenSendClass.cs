using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LicentaApp.JsonClass
{
    public class OauthTokenSendClass
    {
        public string grant_type { get; set; }

        public string code { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string refresh_token { get; set; }

        public string client_id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string client_secret { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string client_assertion { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string client_assertion_type { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string redirect_uri { get; set; }
    }
}
