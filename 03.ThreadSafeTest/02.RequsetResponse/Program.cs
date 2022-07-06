/*********************************************************************************
 * Thread Safe Test
   Q1) Invoke 함수 자체가 스레드 세이프한가.
   A1) 그렇다.
   
   Q2) Invoke가 한 번에 하나씩 호출되는가? 아니면 쓰레드마다 호출(병렬)되는가.
   A2) 병렬 호출
   
   Q3) Invoke를 호출한 스레드와 Invoke 내부 처리가 같은 스레드에서 작동하는가?
   A3) 그렇다.
 *********************************************************************************/

//#define LOCK

#define TEST_1
//#define TEST_2
//#define TEST_3

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MessagePipe;

namespace MessagePipeSampleCodes
{
    class MultiThreadTestSet
    {
        public static uint WORKER_THREAD_NUM = 4;

#if TEST_3
        public static uint TEST_COUNT = 4;
#else
        public static uint TEST_COUNT = 10000;
#endif

        private static Task[] _tasks;
        private static object _lockObject = new object();

        private static void WorkerThreadProcedure()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Request request = null;

            var handler = GlobalMessagePipe.GetRequestHandler<Request, Response>();

            for (int i = 0; i < (TEST_COUNT / WORKER_THREAD_NUM); i++)
            {
                request = new Request(threadId);
#if LOCK
                lock (_lockObject)
                {
                    Response response = handler.Invoke(request);
                }
#else
                    Response response = handler.Invoke(request);
#endif
            }

            return;
        }

        public static void Initilize()
        {
            _tasks = new Task[MultiThreadTestSet.WORKER_THREAD_NUM];
            for (int i = 0; i < MultiThreadTestSet.WORKER_THREAD_NUM; i++)
            {
                _tasks[i] = new Task(MultiThreadTestSet.WorkerThreadProcedure);
            }
        }

        public static void Start(int index)
        {
            _tasks[index].Start();
        }

        public static void StartAll()
        {
            for (int i = 0; i < MultiThreadTestSet.WORKER_THREAD_NUM; i++)
            {
                _tasks[i].Start();
            }
        }

        public static void WaitAll()
        {
            Task.WaitAll(_tasks);
        }
    }

    public class Request
    {
        public int _type;

        public Request(int type)
        {
            _type = type;
        }
    }

    public class Response
    {
        public int _result;

        public Response(int result)
        {
            _result = result;
        }
    }

    public class RequestResponseTestSet
    {
        public static int _count;

        class ReqResHandler : IRequestHandler<Request, Response>
        {
            public Response Invoke(Request request)
            {
                int threadID = Thread.CurrentThread.ManagedThreadId;
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
                //========================================================
                // Q2) : 각 스레드마다 순서대로 한 번에 한번만 실행되는가?
                // A2) : 병렬 호출
                //========================================================
#if TEST_2
                ++_count;
#endif

                //========================================================
                // Q3) : Invoke를 호출한 스레드와 처리하는 스레드가 같은가?
                // A3) : 그렇다.
                //========================================================
#if TEST_3
                Console.WriteLine($"CurThread : {currentThread.ManagedThreadId} WorkerThread : {request._type}");
#endif
                return new Response(1);
            }
        }
    }

    class Program
    {
        static void Initialize()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddMessagePipe();
            ServiceProvider provider = services.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(provider);

            MultiThreadTestSet.Initilize();
            MultiThreadTestSet.StartAll();
            MultiThreadTestSet.WaitAll();
        }

        static void Main(string[] args)
        {
            Initialize();

#if TEST_2
            Console.WriteLine(RequestResponseTestSet._count);
#endif
        }
    }
}