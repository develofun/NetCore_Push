using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NetCore_PushServer.Database;
using NetCore_PushServer.Enums;
using NetCore_PushServer.Models;
using Newtonsoft.Json;

namespace NetCore_PushServer
{
    public class Apple: Push
    {
        protected SemaphoreLock queueLock;
        protected Queue<TokenAlert> queue = new Queue<TokenAlert>();

        private ApnHttp _apnHttp;
        private readonly int _taskCount;
        private List<Task> _tasks = new List<Task>();

        public Apple(ILog log, int taskCount, string keyId, string teamId, string bundleId, bool isSandbox = true) : base(log)
        {
            _taskCount = taskCount;
            _apnHttp = new ApnHttp(keyId, teamId, bundleId, isSandbox);
        }

        public override void Execute(CancellationToken ct)
        {
            queueLock = new SemaphoreLock(ct);

            _tasks.Add(RunSchedulerAsync(ct));

            for (int idx = 0; idx < _taskCount; idx++)
            {
                _tasks.Add(RunSenderAsync(ct));
            }

            Task.WhenAll(_tasks).Wait();
        }

        public async Task RunSchedulerAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                PushRequest request = await PushDB.RequestAllGetAsync(PushType.Google);

                if (request == null)
                {
                    await Task.Delay(5000, ct);
                    continue;
                }

                var dicLanguage = CheckAppleRequest(request);
                if (dicLanguage == null)
                {
                    await PushDB.RequestStateAsync(PushType.Google, request.RequestNo, PushState.Error);
                    continue;
                }

                long sentSeq = 0;

                while (!ct.IsCancellationRequested)
                {
                    List<PushToken> tokens = await PushDB.GetPushTokenAsync(PushType.Google, sentSeq, string.Empty, TokenLimit);
                    if (!tokens.Any())
                    {
                        await PushDB.RequestStateAsync(PushType.Google, request.RequestNo, PushState.Complete);
                        break;
                    }

                    sentSeq = tokens.Last().Seq;

                    await EnqueueTokens(ct, request, dicLanguage, tokens);
                    await PushDB.RequestSendAsync(PushType.Google, request.RequestNo, sentSeq, PushState.Sending);

                    _log.Info($"Send complete apple tokens | Last sent seq: {sentSeq}");
                }

                await Task.Delay(5000, ct);
            }
        }

        public async Task RunSenderAsync(CancellationToken ct)
        {
            long currentRequestNo = -1;
            var expiredTokens = new List<long>();

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var token = await DequeueToken();

                    if (token != null)
                    {
                        if (expiredTokens.Count > 0)
                        {
                            await PushDB.ExpirePushTokensAsync(PushType.Apple, expiredTokens);
                            expiredTokens.Clear();
                        }

                        await Task.Delay(1000, ct);
                        continue;
                    }

                    if (token.Request.RequestNo != currentRequestNo)
                    {
                        // fail + 1
                    }

                    currentRequestNo = token.Request.RequestNo;

                    // send
                    var res = await _apnHttp.RequestAsync(token.Token, token.Alert);

                    if (res.StatusCode != HttpStatusCode.OK)
                    {
                        // fail + 1
                    }

                    if (res.StatusCode == HttpStatusCode.NotFound || res.StatusCode == HttpStatusCode.BadRequest)
                    {
                        expiredTokens.Add(token.Seq);
                        if (expiredTokens.Count > 100)
                        {
                            await PushDB.ExpirePushTokensAsync(PushType.Google, expiredTokens);
                            expiredTokens.Clear();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.StackTrace);
                    await Task.Delay(1000);
                }
            }
        }

        private async Task EnqueueTokens(CancellationToken ct, PushRequest request, Dictionary<string, Alert> dicLang, List<PushToken> tokens)
        {
            request.Locale = request.Locale.Replace(" ", string.Empty);

            if (!string.IsNullOrEmpty(request.Locale))
            {
                var localeSet = StringEx.FromStringWithCommaToHashSet<string>(request.Locale);
                tokens = CheckNightPushTokens(localeSet, tokens);
                if (!tokens.Any()) return;
            }

            while (!ct.IsCancellationRequested)
            {
                using (await queueLock.LockAsync())
                {
                    if (queue.Count < 2000)
                    {
                        foreach (PushToken token in tokens)
                        {
                            if (dicLang.TryGetValue(token.Language, out var alert) == false)
                            {
                                alert = dicLang[DefaultLanguage];
                            }

                            queue.Enqueue(new TokenAlert { Request = request, Seq = token.Seq, Token = token.DeviceToken, Alert = alert });
                        }

                        break;
                    }
                }

                await Task.Delay(100, ct);
            }
        }

        private async Task<TokenAlert> DequeueToken()
        {
            using (await queueLock.LockAsync())
            {
                if (queue.Count > 0)
                {
                    return queue.Dequeue();
                }
            }

            return null;
        }

        private Dictionary<string, Alert> CheckAppleRequest(PushRequest request)
        {
            var dicLanguage = JsonConvert.DeserializeObject<Dictionary<string, Alert>>(request.Payload);

            // 푸시 발송 시 영어는 기본
            if (dicLanguage.ContainsKey("English") == false) return null;

            return dicLanguage;
        }
    }
}
