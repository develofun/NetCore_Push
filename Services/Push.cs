using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NetCore_PushServer.Models;
using Newtonsoft.Json;

namespace NetCore_PushServer
{
    public class Push : IHostedService
    {
        protected readonly ILog _log;
        protected const int TokenLimit = 1000;
        protected const string DefaultLanguage = "English";

        public Push(ILog log)
        {
            _log = log;
        }

        public Task StartAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public virtual Task ExecuteAsync(CancellationToken ct) { throw new Exception("Do not call ExcuteAsync()"); }

        public Task StopAsync(CancellationToken ct)
        {
            _log.Info("Push server stopped...");
            return Task.CompletedTask;
        }

        public List<PushToken> CheckNightPushTokens(HashSet<string> localeSet, List<PushToken> tokens)
        {
            if (tokens == null) return null;

            var now = DateTime.UtcNow;
            var nightStart = new DateTime(now.Year, now.Month, now.Day, 11, 30, 0);     // 20:30 KST
            var nightEnd = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);        // 08:00 KST

            var list = new List<PushToken>();

            foreach (var t in tokens)
            {
                if (localeSet.Count == 0 || localeSet.Contains(t.Locale))
                {
                    if (t.AtNight == false && t.Locale == "KR" && now > nightStart && now < nightEnd)
                        continue;

                    list.Add(t);
                }
            }

            return list;
        }

        protected Dictionary<string, Alert> CheckAppleRequest(PushRequest request)
        {
            var dicLanguage = JsonConvert.DeserializeObject<Dictionary<string, Alert>>(request.Payload);

            // 푸시 발송 시 영어는 기본
            if (dicLanguage.ContainsKey("English") == false) return null;

            return dicLanguage;
        }
    }
}
