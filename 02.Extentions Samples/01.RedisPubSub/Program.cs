using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MessagePipe;

using MessagePipe.Redis;
using StackExchange.Redis;

namespace MessagePipeSampleCodes
{
    class Program
    {
        public async static void AsyncTest()
        {
            const string Key1 = "foo";
            const string Key2 = "bar";

            ConnectionMultiplexer connection = null;
            var c = ConfigurationOptions.Parse("localhost");
            c.ConnectTimeout = 10000;

            try
            {
                connection = StackExchange.Redis.ConnectionMultiplexer.Connect(c);
            }
            catch (RedisConnectionException ex)
            {
                throw new TimeoutException("Can not connect to redis, if you don't up redis in local, call 'docker-compose up' on project root.", ex);
            }

            // 1. 해당 프로그램에서 사용될 서비스를 등록할 틀 생성.
            ServiceCollection sc = new ServiceCollection();

            // 2. MessagePipe 서비스 등록
            sc.AddMessagePipe();
            sc.AddMessagePipeRedis(connection);
            ServiceProvider provider = sc.BuildServiceProvider();

            var p = provider.GetRequiredService<IDistributedPublisher<string, string>>();
            var s = provider.GetRequiredService<IDistributedSubscriber<string, string>>();

            var result = new List<string>();
            var d1 = await s.SubscribeAsync(Key1, (x) => result.Add("1:" + x));
            var d2 = await s.SubscribeAsync(Key2, (x) => result.Add("2:" + x));
            var d3 = await s.SubscribeAsync(Key1, (x) => result.Add("3:" + x));

            // use BeEquivalentTo, allow different order

            await p.PublishAsync(Key1, "one");
            await Task.Delay(TimeSpan.FromSeconds(1)); // wait for receive data...

            result.Clear();

            await p.PublishAsync(Key2, "one");
            await Task.Delay(TimeSpan.FromSeconds(1)); // wait for receive data...

            result.Clear();

            await d3.DisposeAsync();

            await p.PublishAsync(Key1, "two");
            await Task.Delay(TimeSpan.FromSeconds(1)); // wait for receive data...
            result.Clear();

            await d1.DisposeAsync();
            await d2.DisposeAsync();

            await p.PublishAsync(Key1, "zero");
            await p.PublishAsync(Key2, "zero");
            await Task.Delay(TimeSpan.FromSeconds(1)); // wait for receive data...

            result.Clear();
        }

        static void Main(string[] args)
        {
            AsyncTest();
            Console.ReadLine();
        }
    }
}
