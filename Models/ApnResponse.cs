using System.Net;

namespace NetCore_PushServer.Models
{
    public class ApnResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Reason { get; set; }
        public string ApnsId { get; set; }
    }
}
