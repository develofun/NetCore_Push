using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace NetCore_PushServer
{
    public class Program
    {
        private static CancellationToken ct = new CancellationToken();
        private const int taskCount = 10;

        public static void Main(string[] args)
        {
            Config.Load("push.config");

            new Google(new Log(), taskCount).Execute(ct);
            new Apple(new Log(), taskCount, Config.Instance.Apple.KeyId, Config.Instance.Apple.TeamId, Config.Instance.Apple.BundleId, Config.Instance.Apple.IsSandbox).Execute(ct);

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureServices(services =>
                        {
                            services.AddSingleton<ILog, Log>();
                        })
                        .Configure(app =>
                        {
                        });
                });
    }
}
