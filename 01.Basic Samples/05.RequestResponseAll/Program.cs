﻿using System;
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

class ReqResHandlerTwice : IRequestHandler<Request, Response>
{
    public Response Invoke(Request request)
    {
        return PacketProcess2(request);
    }

    public static Response PacketProcess2(Request request)
    {
        Response response = new Response();

        switch (request._type)
        {
            case 1:     // POST
                response._result = PostProcedure2(request);
                break;
            case 2:     // GET
                response._result = GetProcedure2(request);
                break;
            case 3:     // UPDATE
                response._result = UpdateProcedure2(request);
                break;
            default:    // ERROR
                response._result = -1;
                break;
        }

        return response;
    }
    public static int PostProcedure2(Request request) { return 1; }
    public static int GetProcedure2(Request request) { return 2; }
    public static int UpdateProcedure2(Request request) { return 3; }
}

class Program
{
    public struct Packet
    {
        public byte _type;
    }

    public static void RequestPacketHandler(Packet recvPakcet)
    {
        var handler = GlobalMessagePipe.GetRequestAllHandler<Request, Response>();

        // 패킷 처리 시작
        Response[] response = handler.InvokeAll(new Request(recvPakcet._type));

        // 결과
        for (int i = 0; i < response.Length; i++)
        {
            if (response[i]._result < 0)
            {
                Console.WriteLine("잘못된 패킷입니다.");
                return;
            }
        }
        Console.WriteLine("패킷 처리 완료!");
    }

    static void Main(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMessagePipe();
        ServiceProvider provider = services.BuildServiceProvider();
        GlobalMessagePipe.SetProvider(provider);

        Packet recvPacket = new Packet();
        recvPacket._type = 1;

        RequestPacketHandler(recvPacket);
    }
}
}
