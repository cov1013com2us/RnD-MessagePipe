using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePipe;

namespace MessagePipeSamples
{
    public class Program
    {
        public static void MessagePipeSetting()
        {
            var subscriber = GlobalMessagePipe.GetSubscriber<string, MyEvent>();
            subscriber.Subscribe("SendMessage", message =>
            {
                ObjectB.RecvProcedure(message);
            });
        }

        public static void Main(string[] args)
        {
            // 서비스 등록
            var host = CreateHostBuilder(args).Build();

            // 전역으로 사용할 프로바이더 등록
            GlobalMessagePipe.SetProvider(host.Services);

            // 메세지 파이프로 등록할 이벤트 세팅
            MessagePipeSetting();

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
