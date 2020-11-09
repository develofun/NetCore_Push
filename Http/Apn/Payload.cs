using System;
using System.Collections.Generic;
using NetCore_PushServer.Models;
using Newtonsoft.Json;

namespace NetCore_PushServer
{
    public class Payload
    {
        [JsonProperty("aps")]
        public Dictionary<string, object> Aps { get; private set; }

        private Payload()
        {
            Aps = new Dictionary<string, object>();
        }

        public static Payload SetAlert(Alert alert)
        {
            var message = new Payload();

            message.Aps.Add("alert", alert);
            message.Aps.Add("badge", 1);
            message.Aps.Add("mutable-content", 1);
            message.Aps.Add("sound", "default");
            //message.Aps.Add("content-available", 1);

            return message;
        }

        public string ToJson(bool indented = false)
        {
            var serializerSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };

            var formatting = indented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(this, formatting, serializerSettings);
        }
    }
}
