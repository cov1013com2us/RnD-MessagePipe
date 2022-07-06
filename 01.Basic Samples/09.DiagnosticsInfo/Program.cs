using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MessagePipe;

namespace MessagePipeSampleCodes
{
    public struct MyEvent
    {
        public int _eventCode;
    }

    public class SceneA
    {
        private IPublisher<MyEvent> _publisher;
        public SceneA(IPublisher<MyEvent> publisher)
        {
            _publisher = publisher;
        }
        public void TestEvent()
        {
            MyEvent message;
            message._eventCode = 1;
            Console.WriteLine($"송신 이벤트 코드 : {message._eventCode}");

            _publisher.Publish(message);

            Console.WriteLine($"송신 완료");
        }
    }

    public class SceneB
    {
        private ISubscriber<MyEvent> _subscriber;
        private IDisposable _disposable;
        public SceneB(ISubscriber<MyEvent> subscriber)
        {
            _subscriber = subscriber;

            var bag = DisposableBag.CreateBuilder();

            _subscriber.Subscribe(message => TestEventHandler(message)).AddTo(bag);
            _subscriber.Subscribe(message => TestEventHandler(message)).AddTo(bag);
            _subscriber.Subscribe(message => TestEventHandler(message)).AddTo(bag);
            _subscriber.Subscribe(message => TestEventHandler(message)).AddTo(bag);
            _subscriber.Subscribe(message => TestEventHandler(message)).AddTo(bag);

            _disposable = bag.Build();
        }

        public void TestEventHandler(MyEvent message)
        {
            int eventCode = message._eventCode;
            Console.WriteLine($"수신 이벤트 코드 : {eventCode}");
        }

        public void Close()
        {
            _disposable.Dispose();
        }
    }

    public class MonitorTimer : IDisposable
    {
        CancellationTokenSource cts = new CancellationTokenSource();

        public MonitorTimer(MessagePipeDiagnosticsInfo diagnosticsInfo)
        {
            RunTimer(diagnosticsInfo);
        }

        async void RunTimer(MessagePipeDiagnosticsInfo diagnosticsInfo)
        {
            while (!cts.IsCancellationRequested)
            {
                // show SubscribeCount
                Console.WriteLine("SubscribeCount:" + diagnosticsInfo.SubscribeCount);
                await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
            }
        }

        public void Dispose()
        {
            cts.Cancel();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddMessagePipe();
            ServiceProvider provider = services.BuildServiceProvider();

            var publisher = provider.GetRequiredService<IPublisher<MyEvent>>();
            var subscriber = provider.GetRequiredService<ISubscriber<MyEvent>>();

            var info = provider.GetRequiredService<MessagePipeDiagnosticsInfo>();
            MonitorTimer monitor = new MonitorTimer(info);

            SceneA sceneA = new SceneA(publisher);
            SceneB sceneB = new SceneB(subscriber);

            ConsoleKeyInfo keyInfo;
            while (true)
            {
                keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    // 구독자 일괄 해제
                    sceneB.Close();
                    monitor.Dispose();

                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Spacebar)
                {
                    // 이벤트 발생
                    sceneA.TestEvent();
                }
            }
        }
    }
}
