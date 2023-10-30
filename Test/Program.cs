using System.Net;
using System.Net.Sockets;
using System.Text;

using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
socket.Connect(IPEndPoint.Parse("94.130.56.121:443"));
socket.Send("GET / HTTP/1.1\r\n\r\nHost: bonne.run"u8);
var buffer = new byte[1024];
var length = socket.Receive(buffer);
Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, length));


//var socketClient = new SocketClient();
//socketClient.Run();

return;

internal sealed class SocketClient
{
    private readonly FuncWithArg<int> _initialize;
    private readonly FuncWithArg<Socket> _connect;
    private readonly FuncWithArg<Socket> _request;
    private readonly FuncWithArg<Socket> _cleanUp;

    public SocketClient()
    {
        _initialize = new(Initialize);
        _connect = new(Connect);
        _request = new(Request);
        _cleanUp = new(CleanUp);
    }

    public void Run()
    {
        _initialize.Arg = 0;
        var funcWithArg = (IFuncWithArg?)_initialize;
        while (funcWithArg is not null)
        {
            funcWithArg = funcWithArg.Invoke();
        }
    }

    private IFuncWithArg? Initialize(int i)
    {
        if (i is 1) { return null; }

        _connect.Arg = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        return _connect;
    }

    private IFuncWithArg? Connect(Socket socket)
    {
        socket.Connect(IPEndPoint.Parse("94.130.56.121:443"));
        _request.Arg = socket;
        return _request;
    }

    private IFuncWithArg? Request(Socket socket)
    {
        socket.Send("GET / HTTP/1.1\r\n\r\nHost: bonne.run"u8);
        var buffer = new byte[1024];
        var length = socket.Receive(buffer);
        Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, length));
        _cleanUp.Arg = socket;
        return _cleanUp;
    }

    private IFuncWithArg? CleanUp(Socket socket)
    {
        socket.Close();
        socket.Dispose();
        _initialize.Arg = 1;
        return _initialize;
    }
}

internal interface IFuncWithArg
{
    IFuncWithArg? Invoke();
}

internal sealed class FuncWithArg<T> : IFuncWithArg
{
    private readonly Func<T, IFuncWithArg?> _func;

    public FuncWithArg(Func<T, IFuncWithArg?> func)
    {
        _func = func;
    }

    public T Arg { get; set; }

    public IFuncWithArg? Invoke() => _func(Arg);
}

// var start = new FuncWithArg<int>(1, Start);
// Calls(start);

// var start = new RecursiveFuncCallBase<int>(1, RecursiveStart);
// 
// RecursiveCalls(start);
// 
// return;
// 
// static void Calls(IFuncWithArg? funcWithArgs)
// {
//     while (funcWithArgs is not null)
//     {
//         funcWithArgs = funcWithArgs.Invoke();
//     }
// }
// 
// static IFuncWithArg? Start(int i)
// {
//     Console.WriteLine("Do first thing...");
//     return new FuncWithArg<int>(i, FirstThing);
// }
// 
// static IFuncWithArg? FirstThing(int i)
// {
//     Console.WriteLine($"Doint first thing for the {i}th time.");
//     Console.WriteLine("Did first thing, now stopping...");
//     return new FuncWithArg<int>(i, Stop);
// }
// 
// static IFuncWithArg? Stop(int i)
// {
//     Console.WriteLine("Starting over again if less than 5 times.");
//     if (i < 5)
//     {
//         return new FuncWithArg<int>(i + 1, Start);
//     }
// 
//     return null;
// }
// 
// static void RecursiveCalls(RecursiveFuncCall funcCall)
// {
//     foreach (var item in funcCall.Call())
//     {
//         RecursiveCalls(item);
//     }
// }
// 
// static IEnumerable<RecursiveFuncCall> RecursiveStart(int i)
// {
//     Console.WriteLine("Do first thing...");
//     yield return new RecursiveFuncCallBase<int>(i, RecursiveFirstThing);
//     Console.WriteLine("Did first thing, now stopping...");
//     yield return new RecursiveFuncCallBase<int>(i, RecursiveStop);
// }
// 
// static IEnumerable<RecursiveFuncCall> RecursiveFirstThing(int i)
// {
//     Console.WriteLine($"Doint first thing for the {i}th time.");
//     yield break;
// }
// static IEnumerable<RecursiveFuncCall> RecursiveStop(int i)
// {
//     Console.WriteLine("Starting over again if less than 5 times.");
//     if (i < 5)
//     {
//         yield return new RecursiveFuncCallBase<int>(i + 1, RecursiveStart);
//     }
// }
// 
// interface IFuncWithArg
// {
//     IFuncWithArg? Invoke();
// }
// 
// record FuncWithArg<T>(T Arg, Func<T, IFuncWithArg?> Func) : IFuncWithArg
// {
//     public IFuncWithArg? Invoke() => Func(Arg);
// }
// 
// 
// interface RecursiveFuncCall
// {
//     IEnumerable<RecursiveFuncCall> Call();
// }
// 
// record RecursiveFuncCallBase<T>(T Arg, Func<T, IEnumerable<RecursiveFuncCall>> Func) : RecursiveFuncCall
// {
//     public IEnumerable<RecursiveFuncCall> Call() => Func(Arg);
// }
// 