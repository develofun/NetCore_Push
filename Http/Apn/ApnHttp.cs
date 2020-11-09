using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using NetCore_PushServer.Models;
using Newtonsoft.Json;

namespace NetCore_PushServer
{
    public class ApnHttp: Http
    {
		private static readonly Uri _devUri = new Uri("https://api.sandbox.push.apple.com/3/device/");
		private static readonly Uri _prodUri = new Uri("https://api.push.apple.com/3/device/");
		private static SemaphoreLock _semaphoreLock = new SemaphoreLock();
		private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

		private static ECDsa dsa;

		private string _accessToken;
		private DateTime _accessTokenExpireTime = DateTime.MinValue;
		private static readonly TimeSpan _accessTokenLifeTime = TimeSpan.FromMinutes(60);

		private HttpClient _client;

		public string KeyId { get; private set; }
		public string TeamId { get; private set; }
		public string BundleId { get; private set; }
		public bool IsSandbox { get; private set; }

		public ApnHttp(string keyId, string teamId, string bundleId, bool isSandbox = true)
		{
			KeyId = keyId;
			TeamId = teamId;
			BundleId = bundleId;
			IsSandbox = isSandbox;
		}

		public static void Initialize(string keyPath)
		{
			keyPath = JsonAppConfig.SearchParentDirectory(keyPath);
			dsa = CreatePrivateKeyAlgorithm(keyPath);
		}

		private static ECDsa CreatePrivateKeyAlgorithm(string keyPath)
		{
			try
			{
				var key = LoadPrivateKey(keyPath);

				dsa = ECDsa.Create();
				dsa.ImportPkcs8PrivateKey(key, out int _);

				return dsa;
			}
			catch (Exception ex)
			{
				_log.Error(ex);
				return null;
			}
		}

		private static byte[] LoadPrivateKey(string keyPath)
		{
			try
			{
				var sb = new StringBuilder();

				bool begun = false;
				var lines = File.ReadAllLines(keyPath);
				foreach (var l in lines)
				{
					if (l.Contains("BEGIN PRIVATE KEY"))
					{
						begun = true;
						continue;
					}

					if (l.Contains("END PRIVATE KEY"))
						break;

					if (begun && !string.IsNullOrEmpty(l))
						sb.Append(l.Trim());
				}

				var key = sb.ToString();
				var keyBytes = Convert.FromBase64String(key);

				return keyBytes;
			}
			catch (Exception ex)
			{
				_log.Error(ex);
				return null;
			}
		}

		private static string GetAccessToken(string keyId, string teamId)
		{
			var provider = JsonConvert.SerializeObject(
				new { alg = "ES256", kid = keyId }
			);
			provider = Base64UrlEncoder.Encode(provider);

			var claims = JsonConvert.SerializeObject(
				new { iss = teamId, iat = DateTimeEx.ToUnixTime(DateTime.UtcNow) }
			);
			claims = Base64UrlEncoder.Encode(claims);

			var jwt = provider + "." + claims;
			var jwtBytes = Encoding.UTF8.GetBytes(jwt);
			var signature = dsa.SignData(jwtBytes, HashAlgorithmName.SHA256);

			return jwt + "." + Base64UrlEncoder.Encode(signature);
		}

		public async Task<ApnResponse> RequestAsync(string deviceToken, Alert alert, Guid? apnsId = null)
		{
			using (await semaphoreLock.LockAsync())
			{
				var now = DateTime.UtcNow;

				if (_accessToken == null || now > _accessTokenExpireTime)
				{
					_accessToken = GetAccessToken(KeyId, TeamId);
					_accessTokenExpireTime = now + _accessTokenLifeTime;

					var handler = new SocketsHttpHandler
					{
						MaxConnectionsPerServer = int.MaxValue,
						SslOptions = new SslClientAuthenticationOptions { EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls }
					};

					_client = new HttpClient(handler, true);
					_client.BaseAddress = IsSandbox ? _devUri : _prodUri;
					_client.DefaultRequestVersion = new System.Version(2, 0);
					_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
					_client.DefaultRequestHeaders.Add("apns-push-type", "alert");
					_client.DefaultRequestHeaders.Add("apns-expiration", "0");
					_client.DefaultRequestHeaders.Add("apns-priority", "10");
					_client.DefaultRequestHeaders.Add("apns-topic", BundleId);
					_client.DefaultRequestHeaders.Connection.Add("keep-alive");
				}
			}

			var payload = Payload.SetAlert(alert);
			var json = payload.ToJson();

			using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
			{
				using (var res = await _client.PostAsync(deviceToken, content))
				{
					var response = new ApnResponse
					{
						StatusCode = res.StatusCode,
						Reason = res.ReasonPhrase
					};

					if (res.Headers.TryGetValues("apns-id", out var values))
					{
						response.ApnsId = values.FirstOrDefault();
					}

					return response;
				}
			}
		}
	}
}
