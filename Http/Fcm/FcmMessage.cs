using System;
using System.Collections.Generic;
using NetCore_PushServer.Models;
using Newtonsoft.Json;

namespace NetCore_PushServer
{
    public class FcmMessage
    {
		[JsonProperty("message")]
		public Dictionary<string, object> Message { get; set; }

		private FcmMessage() { }

		public static FcmMessage SetTokenAndNotification(string token, AndroidNotification notification)
		{
			if (string.IsNullOrWhiteSpace(notification.Color))
			{
				notification.Color = "ff7f7f7f";
			}

			var fcmMessage = new FcmMessage();

			fcmMessage.Message = new Dictionary<string, object>();
			fcmMessage.Message["token"] = token;
			fcmMessage.Message["android"] = new AndroidConfig { Notification = notification };

			return fcmMessage;
		}

		public static FcmMessage SetTokenAndData(string token, AndroidNotification notification)
		{
			if (string.IsNullOrWhiteSpace(notification.Color))
			{
				notification.Color = "ff7f7f7f";
			}

			var fcmMessage = new FcmMessage();

			fcmMessage.Message = new Dictionary<string, object>();
			fcmMessage.Message["token"] = token;
			fcmMessage.Message["android"] = new AndroidConfig { Data = notification };

			return fcmMessage;
		}

		public static FcmMessage SetTopicAndNotification(string topic, AndroidNotification notification)
		{
			if (string.IsNullOrWhiteSpace(notification.Color))
			{
				notification.Color = "ff7f7f7f";
			}

			var fcmMessage = new FcmMessage();

			fcmMessage.Message = new Dictionary<string, object>();
			fcmMessage.Message["topic"] = topic;
			fcmMessage.Message["android"] = new AndroidConfig { Notification = notification };

			return fcmMessage;
		}

		public string ToJson(bool indented = false)
		{
			var serializerSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };

			var formatting = indented ? Formatting.Indented : Formatting.None;
			return JsonConvert.SerializeObject(this, formatting, serializerSettings);
		}
	}
}
