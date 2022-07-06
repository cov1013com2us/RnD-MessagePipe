using System;
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
        private IPublisher<string, MyEvent> _publisher;
        public SceneA(IPublisher<string, MyEvent> publisher)
        {
            _publisher = publisher;
        }
        public void TestEvent()
        {
            MyEvent message;
            message._eventCode = 1;
            Console.WriteLine($"송신 이벤트 코드 : {message._eventCode}");

            _publisher.Publish("Key-1", message);
            Console.WriteLine($"Key-1 송신 완료");

            _publisher.Publish("Key-2", message);
            Console.WriteLine($"Key-2 송신 완료");

            _publisher.Publish("Key-3", message);
            Console.WriteLine($"Key-3 송신 완료");
        }
    }

    public class SceneB
    {
        private ISubscriber<string, MyEvent> _subscriber;
        public SceneB(ISubscriber<string, MyEvent> subscriber)
        {
            _subscriber = subscriber;
            _subscriber.Subscribe("Key-1", message => TestEventHandler(message));
            _subscriber.Subscribe("Key-2", message => TestEventHandler(message));
            _subscriber.Subscribe("Key-3", message => TestEventHandler(message));
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

            var publisher = provider.GetRequiredService<IPublisher<string, MyEvent>>();
            var subscriber = provider.GetRequiredService<ISubscriber<string, MyEvent>>();

            SceneA sceneA = new SceneA(publisher);
            SceneB sceneB = new SceneB(subscriber);

            // 이벤트 발생
            sceneA.TestEvent();
        }
    }
}
