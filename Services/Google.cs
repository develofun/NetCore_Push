using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetCore_PushServer.Database;
using NetCore_PushServer.Enums;
using NetCore_PushServer.Models;

namespace NetCore_PushServer
{
    public class Google : Push
    {
        protected SemaphoreLock queueLock;
        protected Queue<TokenNotification> queue = new Queue<TokenNotification>();

        public Google(ILog log): base(log) {}

        public override Task ExecuteAsync(CancellationToken ct)
        {
            queueLock = new SemaphoreLock(ct);
            return base.ExecuteAsync(ct);
        }

        public async Task RunSchedulerAsync(CancellationToken ct)
        {
            while(!ct.IsCancellationRequested)
            {
                PushRequest request = await PushDB.RequestAllGetAsync(PushType.Google);

                if (request == null)
                {
                    await Task.Delay(5000, ct);
                    continue;
                }

                var dicLanguage = CheckRequest(request);
                if (dicLanguage == null)
                {
                    await PushDB.RequestStateAsync(PushType.Google, request.RequestNo, PushState.Error);
                    continue;
                }

                long sentSeq = 0;

                while(!ct.IsCancellationRequested)
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

                    _log.Info($"Send complete google tokens | Request No {request.RequestNo} | Last sent seq: {sentSeq}");
                }

                await Task.Delay(5000, ct);
            }
        }

        public async Task RunSenderAsync(CancellationToken ct)
        {
            long currentRequestNo = -1;
            var expiredTokens = new List<long>();

            while(!ct.IsCancellationRequested)
            {
                var token = await DequeueToken();

                if (token != null)
                {
                    if (expiredTokens.Count > 0)
                    {
                        await PushDB.ExpirePushTokensAsync(PushType.Google, expiredTokens);
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

                // result check

                await Task.Delay(1000);
            }
        }

        private async Task EnqueueTokens(CancellationToken ct, PushRequest request, Dictionary<string, AndroidNotification> dicLang, List<PushToken> tokens)
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
                            if (dicLang.TryGetValue(token.Language, out var notification) == false)
                            {
                                notification = dicLang[DefaultLanguage];
                            }

                            queue.Enqueue(new TokenNotification { Request = request, Seq = token.Seq, Token = token.DeviceToken, Notification = notification });
                        }

                        break;
                    }
                }

                await Task.Delay(100, ct);
            }
        }

        private async Task<TokenNotification> DequeueToken()
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
    }
}
