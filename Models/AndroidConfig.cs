using System;
using Newtonsoft.Json;

namespace NetCore_PushServer.Models
{
    public class AndroidConfig
    {
        // "high" or "normal"
        [JsonProperty("priority")]
        public string Priority { get; set; } = "normal";

        [JsonProperty("collapse_key")]
        public string CollapseKey { get; set; }

        [JsonProperty("ttl")]
        public string Ttl { get; set; }

        [JsonProperty("restricted_package_name")]
        public string RestrictedPackageName { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("notification")]
        public object Notification { get; set; }

    }
}
