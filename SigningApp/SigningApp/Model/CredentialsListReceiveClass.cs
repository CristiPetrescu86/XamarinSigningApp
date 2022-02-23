using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicentaApp.JsonClass
{
    public class CredentialsListReceiveClass
    {
        public List<string> credentialIDs { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string nextPageToken { get; set; }

    }
}
