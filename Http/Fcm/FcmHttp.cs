using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using NetCore_PushServer.Models;

namespace NetCore_PushServer
{
    public class FcmHttp: Http
    {
        private static Uri _baseUri;
        private static GoogleCredential _googleCredential;
        private static SemaphoreLock _semaphoreLock = new SemaphoreLock();

        private HttpClient _client;
        private string _accessToken;
        private DateTime _accessTokenExpireTime = DateTime.MinValue;
        private static readonly TimeSpan _accessTokenLifeTime = TimeSpan.FromMinutes(60);

        public void Initialize(string productId, string keyPath)
        {
            _baseUri = new Uri($"https://fcm.googleapis.com/v1/projects/{productId}/messages:send");

            keyPath = JsonAppConfig.SearchParentDirectory(keyPath);
            var scope = "https://www.googleapis.com/auth/firebase.messaging";
            using var stream = new FileStream(keyPath, FileMode.Open, FileAccess.Read);
            _googleCredential = GoogleCredential.FromStream(stream).CreateScoped(scope);
        }

        public async Task<HttpResponse> RequestAsync(string token, AndroidNotification notification)
        {
            using (await _semaphoreLock.LockAsync())
            {
                var now = DateTime.UtcNow;

                if (_accessToken == null || now > _accessTokenExpireTime)
                {
                    _accessToken = await _googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();
                    _accessTokenExpireTime = now + _accessTokenLifeTime;

                    var handler = new SocketsHttpHandler
                    {
                        MaxConnectionsPerServer = int.MaxValue,
                        SslOptions = new SslClientAuthenticationOptions { EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls }
                    };

                    _client = new HttpClient(handler, true);
                    _client.BaseAddress = _baseUri;
                    _client.DefaultRequestVersion = new System.Version(2, 0);
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                    _client.DefaultRequestHeaders.Connection.Add("keep-alive");
                }
            }

            var json = FcmMessage.SetTokenAndData(token, notification).ToJson();
            using var res = await _client.PostAsync("", new StringContent(json, Encoding.UTF8, "application/json"));

            return new HttpResponse
            {
                StatusCode = res.StatusCode,
                Reason = res.ReasonPhrase
            };
        }
    }
}