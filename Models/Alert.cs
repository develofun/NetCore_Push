using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetCore_PushServer.Models
{
    public class Alert
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subtitle")]
        public string SubTitle { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("launch-image")]
        public string LaunchImage { get; set; }

        [JsonProperty("title-loc-key")]
        public string TitleLocKey { get; set; }

        [JsonProperty("title-loc-args")]
        public List<string> TitleLogArgs { get; set; }

        [JsonProperty("subtitle-loc-key")]
        public string SubTitleLocKey { get; set; }

        [JsonProperty("subtitle-loc-args")]
        public List<string> SubTitleLocArgs { get; set; }

        [JsonProperty("loc-key")]
        public string LocKey { get; set; }

        [JsonProperty("loc-args")]
        public List<string> LocArgs { get; set; }
    }
}
