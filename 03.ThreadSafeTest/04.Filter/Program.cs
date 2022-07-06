/*********************************************************************************
 * Thread Safe Test
   Q1) 필터 적용시 스레드 세이프하게 작동하는가?
   A1) 그렇다.

   Q2) 각 스레드별 필터 함수를 한 번씩 차레대로 호출하는가?
   A1) 병렬 호출
 *********************************************************************************/

//#define LOCK
//#define TEST_1
//#define TEST_2

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
        public static uint TEST_COUNT = 10000;

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
        public static int gobalCount;

        [RequestHandlerFilter(typeof(ReqResHandlerFilter))]
        class ReqResHandler : IRequestHandler<Request, Response>
        {
            public Response Invoke(Request request)
            {
                int threadID = Thread.CurrentThread.ManagedThreadId;

#if TEST_1
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

                return new Response(1);
            }
        }

        class ReqResHandlerFilter : RequestHandlerFilter<Request, Response>
        {

            public override Response Invoke(Request request, Func<Request, Response> next)
            {
                int threadID = Thread.CurrentThread.ManagedThreadId;

#if TEST_1
                int count = 0;
                for (uint i = 0; i < 1000; i++)
                {
                    count++;
                }
                if (count == 1000)
                {
                    Console.WriteLine($"Filter Thread ({threadID}) : {count}");
                }
                else
                {
                    Console.ReadLine();
                }
#endif

#if TEST_2
                RequestResponseTestSet.gobalCount++;
#endif

                return next(request);
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
            Console.WriteLine(RequestResponseTestSet.gobalCount);
#endif
        }
    }
}