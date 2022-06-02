using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LicentaApp.JsonClass
{
    public class OauthTokenReceiveClass
    {
        public string access_token { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string refresh_token { get; set; }

        public string token_type { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int expires_in { get; set; }
    }
}
