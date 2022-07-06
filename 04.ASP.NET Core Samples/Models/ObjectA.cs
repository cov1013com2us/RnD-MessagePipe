using System;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

namespace MessagePipeSamples
{
    public class ObjectA
    {
        public static void SendMessage(MyEvent message)
        {
            var publisher = GlobalMessagePipe.GetPublisher<string, MyEvent>();

            Console.WriteLine($"송신 시작");
            publisher.Publish("SendMessage", message);
            Console.WriteLine($"송신 완료");
        }
    }
}
