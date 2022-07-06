using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

namespace MessagePipeSamples
{
    class ReqResHandler : IRequestHandler<Request, Response>
    {
        public Response Invoke(Request request)
        {
            return PacketProcess(request);
        }

        public static Response PacketProcess(Request request)
        {
            Response response = new Response(0);

            switch (request.type)
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

    [Route("api")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpPost]
        [Route("pubsub")]
        public void PubSub(MyEvent message)
        {
            ObjectA.SendMessage(message);

            //... 완료 처리
        }

        [HttpPost]
        [Route("reqres")]
        public void ReqRes(Request request)
        {
            var handler = GlobalMessagePipe.GetRequestHandler<Request, Response>();
            var response = handler.Invoke(request);

            //... 완료 처리
        }
    }
}
