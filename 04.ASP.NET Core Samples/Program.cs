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
            // ���� ���
            var host = CreateHostBuilder(args).Build();

            // �������� ����� ���ι��̴� ���
            GlobalMessagePipe.SetProvider(host.Services);

            // �޼��� �������� ����� �̺�Ʈ ����
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
