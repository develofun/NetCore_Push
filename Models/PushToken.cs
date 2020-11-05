namespace NetCore_PushServer.Models
{
    public class PushToken
    {
		public long Seq { get; set; }
		public string DeviceToken { get; set; }
		public string Locale { get; set; }
		public string Language { get; set; }
		public bool AtNight { get; set; }
		public bool Expired { get; set; }
	}

	public class TokenNotification
	{
        public PushRequest Request { get; set; }
        public long Seq { get; set; }
		public string Token { get; set; }
		public AndroidNotification Notification { get; set; }
	}
}
