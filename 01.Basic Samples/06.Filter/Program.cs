using System;
using Microsoft.Extensions.DependencyInjection;
using MessagePipe;

namespace MessagePipeSampleCodes
{
    public class Request
    {
        public byte _type;

        public Request(byte type)
        {
            _type = type;
        }
    }

    public class Response
    {
        public int _result;
    }

    [RequestHandlerFilter(typeof(ReqResHandlerFilter))]
    class ReqResHandler : IRequestHandler<Request, Response>
    {
        public Response Invoke(Request request)
        {
            return PacketProcess(request);
        }

        public static Response PacketProcess(Request request)
        {
            Response response = new Response();

            switch (request._type)
            {
                case 1:     // POST
                    response._result = PostProcedure(request);
                    break;
                case 2:     // GET
                    response._result = GetProcedure(request);
                    break;
                case 3:     // UPDATE
                    response._result = UpdateProcedure(request);
                    break;
                default:    // ERROR
                    response._result = -1;
                    break;
            }

            return response;
        }
        public static int PostProcedure(Request request) { return 1; }
        public static int GetProcedure(Request request) { return 2; }
        public static int UpdateProcedure(Request request) { return 3; }
    }

    public class ReqResHandlerFilter : RequestHandlerFilter<Request, Response>
    {
        public override Response Invoke(Request request, Func<Request, Response> next)
        {
            // 잘못된 패킷 처리
            if (request._type < 1)
            {
                var response = new Response();
                response._result = -1;
                return response;
            }

            // 실제 핸들러 호출
            return next.Invoke(request);
        }
    }

    class Program
    {
        public class Packet
        {
            public byte _type;
        }

        public static void RequestPacketHandler(Packet recvPakcet)
        {
            var handler = GlobalMessagePipe.GetRequestHandler<Request, Response>();

            // 패킷 처리 시작
            Response response = handler.Invoke(new Request(recvPakcet._type));

            // 결과
            if (response._result < 0)
            {
                Console.WriteLine("잘못된 패킷입니다.");
                return;
            }
            else
            {
                Console.WriteLine("패킷 처리 완료!");
            }
        }

        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddMessagePipe();
            ServiceProvider provider = services.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(provider);

            Packet recvPacket = new Packet();
            recvPacket._type = 0;

            RequestPacketHandler(recvPacket);
        }
    }
}
