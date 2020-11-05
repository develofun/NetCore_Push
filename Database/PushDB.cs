namespace NetCore_PushServer.Database
{
    public partial class PushDB
    {
        public static string GetConnectionString()
        {
            return Config.Instance.ConnectionString;
        }
    }
}
