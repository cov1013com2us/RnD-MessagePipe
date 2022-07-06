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

class Program
{
    static void Main(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMessagePipe();
        ServiceProvider provider = services.BuildServiceProvider();

        var publisher = provider.GetRequiredService<IPublisher<MyEvent>>();
        var subscriber = provider.GetRequiredService<ISubscriber<MyEvent>>();

        SceneA sceneA = new SceneA(publisher);
        SceneB sceneB = new SceneB(subscriber);

        // 이벤트 발생
        sceneA.TestEvent();

        // 구독자 일괄 해제
        sceneB.Close();
    }
}
}
