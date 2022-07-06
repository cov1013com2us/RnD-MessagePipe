using System;
using MessagePipe;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MessagePipeSamples
{
    public class ObjectB
    {
        public static void RecvProcedure(MyEvent message)
        {
            Console.WriteLine($"메세지 수신 완료 type({message.type})");
        }
    }
}
