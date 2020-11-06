using System.Net;

namespace NetCore_PushServer.Models
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Reason { get; set; }
    }
}
