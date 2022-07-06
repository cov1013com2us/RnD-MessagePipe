using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MessagePipe;

namespace MessagePipeSampleCodes
{
class Program
{
    public async static void AsyncTest()
    {
        var sc = new ServiceCollection();
        sc.AddMessagePipe();
        sc.AddMessagePipeTcpInterprocess("127.0.0.1", 1784, x =>
        {
            x.HostAsServer = true;
        });

        var provider = sc.BuildServiceProvider();

        using (provider as IDisposable)
        {
            var p1 = provider.GetRequiredService<IDistributedPublisher<int, int>>();
            var s1 = provider.GetRequiredService<IDistributedSubscriber<int, int>>();

            var result = new List<int>();
            await s1.SubscribeAsync(1, x =>
            {
                result.Add(x);
            });

            var result2 = new List<int>();
            await s1.SubscribeAsync(4, x =>
            {
                result2.Add(x);
            });

            await Task.Delay(TimeSpan.FromSeconds(1)); // wait for receive data...
            await p1.PublishAsync(1, 9999);
            await Task.Delay(TimeSpan.FromSeconds(1)); // wait for receive data...
            await p1.PublishAsync(4, 888);
            await Task.Delay(TimeSpan.FromSeconds(1)); // wait for receive data...
            await p1.PublishAsync(1, 4999);
            await Task.Delay(TimeSpan.FromSeconds(1)); // wait for receive data...
        }
    }

    static void Main(string[] args)
    {
        AsyncTest();
        Console.ReadLine();
    }
}
}
