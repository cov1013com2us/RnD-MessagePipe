/*********************************************************************************
 * Thread Safe Test
   Q1) Publish로 호출한 Subscribe Handler 자체가 스레드 세이프한가.
   A1) 그렇다.

   Q2) Subscribe Handler를 여러 개 등록했을 때 순차적(스레드마다)으로 호출되는가.
   A2) 그렇다.
   
   Q3) Publish가 한 번에 하나씩 호출되는가? 아니면 쓰레드마다 호출(병렬)되는가.
   A3) 각 스레드마다 호출된다.
   
   Q4) Publish를 호출한 스레드에서 Subscribe Handle가 호출되는가?
   A4) 그렇다.
 *********************************************************************************/

// 원하는 테스트 활성화.
//#define LOCK
#define TEST_1
//#define TEST_2
//#define TEST_3
//#define TEST_4

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

#if TEST_3
        public static uint MAX_COUNT = 10000;
#endif

#if TEST_1
        public static uint MAX_COUNT = 10000;
#else
        public static uint MAX_COUNT = 4;
#endif

        private static object lockObject = new object();

        public static void WorkerThreadProcedure()
        {
            Thread currentThread = Thread.CurrentThread;

            int count = 0;
            IPublisher<int> publisher = GlobalMessagePipe.GetPublisher<int>();

            while (count < (MAX_COUNT / MAX_THREAD))
            {
#if LOCK
                lock (lockObject)
                {
                    publisher.Publish(currentThread.ManagedThreadId);
                }
#else
                publisher.Publish(currentThread.ManagedThreadId);
#endif
                count++;
            }

            //Console.WriteLine($"thread({currentThread.ManagedThreadId}) : {count}");

            return;
        }
    }

    class Program
    {
        public static int globalCount = 0;

        static void SubscribeHandler(int threadID)
        {

#if TEST_1
            //========================================================
            // Q1) 해당 함수 자체가 스레드 세이프한지 확인(여러 스레드에서 Publish 함수를 여러번 호출 했을 때 아래 코드가 정상 작동할까?)
            // A1) 이상 없음.
            //     물론 공유 자원 접근 시에는 각 스레드별 동기화가 필요하다.
            //========================================================
            uint count = 0;
            for (uint i = 0; i < 1000; i++)
            {
                count++;
            }
            if (count == 1000)
            {
                Console.WriteLine($"Worker Thread ({threadID}) : {count}");
            }
            else
            {
                Console.ReadLine();
            }
#endif

#if TEST_3
            //========================================================
            // Q3) Publish 함수 호출 방식 확인
            // A3) 스레드마다 Publish를 호출할 경우 각자 병렬로 호출된다.
            //     때문에 공유 자원 접근시에는 Subscribe Handler 자체에서
            //     동기화를 하거나, Publish 자체를 동기화해야한다.
            // !)  해당 테스트는 globalCount를 동기화 없이 증가할 경우를 테스트한 것.
            //========================================================
            globalCount++;
#endif

#if TEST_4
            Console.WriteLine($"CurThread : {Thread.CurrentThread.ManagedThreadId} WorkerThread : {threadID}");
#endif
        }

        static void Initialize()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddMessagePipe();
            ServiceProvider provider = services.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(provider);

            ISubscriber<int> subscriber = GlobalMessagePipe.GetSubscriber<int>();

#if TEST_2
            //=====================================================================
            // Q2) Subscribe를 여러 개 등록했을 때 스레드마다 순차적으로 실행되는가?
            // A2) 스레드 별 Subscribe Handler 호출 순서가 보장된다.
            //=====================================================================
            subscriber.Subscribe(x => 
            {
                Thread thread = Thread.CurrentThread;
                Console.WriteLine($"Thread({thread.ManagedThreadId}) : 1");
            });
            subscriber.Subscribe(x =>
            {
                Thread thread = Thread.CurrentThread;
                Console.WriteLine($"Thread({thread.ManagedThreadId}) : 2");
            });
            subscriber.Subscribe(x =>
            {
                Thread thread = Thread.CurrentThread;
                Console.WriteLine($"Thread({thread.ManagedThreadId}) : 3");
            });
            subscriber.Subscribe(x =>
            {
                Thread thread = Thread.CurrentThread;
                Console.WriteLine($"Thread({thread.ManagedThreadId}) : 4");
            });
            subscriber.Subscribe(x =>
            {
                Thread thread = Thread.CurrentThread;
                Console.WriteLine($"Thread({thread.ManagedThreadId}) : 5");
            });
#else
            subscriber.Subscribe(x => SubscribeHandler(x));
#endif
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

#if TEST_3
            Console.WriteLine(globalCount);
#endif
        }

        static void Main(string[] args)
        {
            Initialize();
            Run();
        }
    }
}