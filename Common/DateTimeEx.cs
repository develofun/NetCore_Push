using System;
namespace NetCore_PushServer
{
    public class DateTimeEx
    {
		private static readonly DateTime dt1970 = new DateTime(1970, 1, 1);

		public static int ToUnixTime(DateTime dt)
		{
			return (int)(dt - dt1970).TotalSeconds;
		}

		public static DateTime FromUnixTime(int unixTime)
		{
			return dt1970 + TimeSpan.FromSeconds(unixTime);
		}
	}
}
