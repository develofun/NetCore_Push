using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetCore_PushServer.Models
{
    public class AndroidNotification
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }            // URL

        [JsonProperty("color")]
        public string Color { get; set; }           // Notification icon Color, #rrggbb format

        [JsonProperty("sound")]
        public string Sound { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("click_action")]
        public string ClickAction { get; set; }

        [JsonProperty("body_loc_key")]
        public string BodyLocKey { get; set; }

        [JsonProperty("body_loc_args")]
        public List<string> BodyLocArgs { get; set; }

        [JsonProperty("title_loc_key")]
        public string TitleLocKey { get; set; }

        [JsonProperty("title_loc_args")]
        public List<string> TitleLocArgs { get; set; }

        public string ToJson()
        {
            var serializerSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };
            return JsonConvert.SerializeObject(this, serializerSettings);
        }
    }
}
