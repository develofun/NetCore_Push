namespace NetCore_PushServer
{
	public class Config : Singleton<Config>
	{
		public string ServerUrl { get; set; }
		public string ConnectionString { get; set; }

		public AppleConfig Apple { get; set; }
		public GoogleConfig Google { get; set; }

		public static void Load(string filePath)
		{
			Instance = JsonAppConfig.Load<Config>(filePath);
		}
	}

	public class AppleConfig
	{
		public bool IsSandbox { get; set; }
		public int TaskCount { get; set; }

		public string TeamId { get; set; }
		public string KeyId { get; set; }
		public string BundleId { get; set; }
		public string AuthPrivateKeyPath { get; set; }
	}

	public class GoogleConfig
	{
		public int TaskCount { get; set; }

		public string ProjectId { get; set; }
		public string ServiceAccountKeyPath { get; set; }
	}
}
