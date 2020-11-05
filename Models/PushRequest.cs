using System;
namespace NetCore_PushServer.Models
{
	public class PushRequest
	{
		public long RequestNo { get; set; }
		public long Suid { get; set; }          // For Single
		public string Targets { get; set; }     // For Multi
		public string Payload { get; set; }
		public string Locale { get; set; }
		public long SentSeq { get; set; }       // For All
	}
}
