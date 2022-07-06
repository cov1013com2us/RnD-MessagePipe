/*********************************************************************************
 * Thread Safe Test
   Q1) Invoke 함수 호출 시 등록한 순서대로 호출되는가?
   A1) 그렇다.
 *********************************************************************************/

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
        public static uint WORKER_THREAD_NUM = 4;
        public static uint TEST_COUNT = 4;

        private static Task[] _tasks;
        private static object _lockObject = new object();

        private static void WorkerThreadProcedure()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Request request = null;

            var handler = GlobalMessagePipe.GetRequestAllHandler<Request, Response>();

            for (int i = 0; i < (TEST_COUNT / WORKER_THREAD_NUM); i++)
            {
                request = new Request(threadId);
#if LOCK
                lock (_lockObject)
                {
                    Response[] response = handler.InvokeAll(request);
                }
#else
                Response[] response = handler.InvokeAll(request);
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
        class ReqResHandler1 : IRequestHandler<Request, Response>
        {
            public Response Invoke(Request request)
            {
                int threadID = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine($"CurThread({threadID}) Handler 1");

                return new Response(1);
            }
        }
        class ReqResHandler2 : IRequestHandler<Request, Response>
        {
            public Response Invoke(Request request)
            {
                int threadID = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine($"CurThread({threadID}) Handler 2");

                return new Response(1);
            }
        }
        class ReqResHandler3 : IRequestHandler<Request, Response>
        {
            public Response Invoke(Request request)
            {
                int threadID = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine($"CurThread({threadID}) Handler 3");

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
        }
    }
}