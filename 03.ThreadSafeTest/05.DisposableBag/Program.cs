/*********************************************************************************
 * Thread Safe Test
    Q1) 다른 스레드에서 Subscribe를 Dispose 하면 감지할까?
    A1) 감지를 못한다. 때문에 어느 한 스레드에서 Publish 작업이 남아있다면 Dispose하면 안됨.
 *********************************************************************************/

// 원하는 테스트 활성화.
//#define LOCK

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MessagePipe;

namespace MessagePipeSampleCodes
{
    class MultiThreadTestSet
    {
        public static uint MAX_THREAD = 4;
        public static uint MAX_COUNT = 10000;

        private static object lockObject = new object();

        public static void WorkerThreadProcedure()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            IPublisher<int> publisher = GlobalMessagePipe.GetPublisher<int>();

            for (int i = 0; i < (MAX_COUNT / MAX_THREAD); i++)
            {
#if LOCK
                lock (lockObject)
                {
                    publisher.Publish(currentThread.ManagedThreadId);
                }
#else
                publisher.Publish(i + 1);
                Program.globalDisposable.Dispose();
#endif
            }

            return;
        }
    }

    class Program
    {
        public static IDisposable globalDisposable;

        static void SubscribeHandler(int threadId)
        {
            Console.WriteLine(threadId);
        }

        static void Initialize()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddMessagePipe();
            ServiceProvider provider = services.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(provider);

            ISubscriber<int> subscriber = GlobalMessagePipe.GetSubscriber<int>();

            var bag = DisposableBag.CreateBuilder();

            subscriber.Subscribe(x => 
            {
                uint max = MultiThreadTestSet.MAX_COUNT / MultiThreadTestSet.MAX_THREAD;
                if (x == max)
                {
                    Console.WriteLine(x);
                }
            }).AddTo(bag);

            Program.globalDisposable = bag.Build();
        }

        static void Run()
        {
            Task[] tasks = new Task[MultiThreadTestSet.MAX_THREAD];
            for (int i = 0; i < MultiThreadTestSet.MAX_THREAD; i++)
            {
                tasks[i] = new Task(MultiThreadTestSet.WorkerThreadProcedure);
                tasks[i].Start();
            }
            Task.WaitAll(tasks);
        }

        static void Main(string[] args)
        {
            Initialize();
            Run();
        }
    }
}