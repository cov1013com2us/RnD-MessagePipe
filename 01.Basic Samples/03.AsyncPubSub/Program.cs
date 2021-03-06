using System;
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
        private IAsyncPublisher<string, MyEvent> _publisher;
        public SceneA(IAsyncPublisher<string, MyEvent> publisher)
        {
            _publisher = publisher;
        }
        public async void TestEvent()
        {
            MyEvent message;
            message._eventCode = 1;
            Console.WriteLine($"송신 이벤트 코드 : {message._eventCode}");

            await _publisher.PublishAsync("Key", message);

            Console.WriteLine($"송신 완료");
        }
    }

    public class SceneB
    {
        private IAsyncSubscriber<string, MyEvent> _subscriber;
        public SceneB(IAsyncSubscriber<string, MyEvent> subscriber)
        {
            _subscriber = subscriber;
            _subscriber.Subscribe("Key", async (message, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), ct); // DB Access...
                TestEventHandler(message);
            });
        }
        public void TestEventHandler(MyEvent message)
        {
            int eventCode = message._eventCode;
            Console.WriteLine($"수신 이벤트 코드 : {eventCode}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddMessagePipe();
            ServiceProvider provider = services.BuildServiceProvider();

            var publisher = provider.GetRequiredService<IAsyncPublisher<string, MyEvent>>();
            var subscriber = provider.GetRequiredService<IAsyncSubscriber<string, MyEvent>>();

            SceneA sceneA = new SceneA(publisher);
            SceneB sceneB = new SceneB(subscriber);

            // 이벤트 발생
            sceneA.TestEvent();
            Console.ReadLine();
        }
    }
}
