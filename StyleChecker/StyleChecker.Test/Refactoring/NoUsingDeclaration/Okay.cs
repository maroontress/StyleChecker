#nullable enable
namespace StyleChecker.Test.Refactoring.NoUsingDeclaration;

using System;
using System.IO;
using System.Net.Sockets;

public sealed class Okay
{
    private StreamReader? streamReader;

    private TextWriter? SharedWriter { get; set; }

    public void AssignedToFieldOrProperty()
    {
        // Replacing 'var' with 'using var' makes no sense.
        var reader = new StreamReader("input.txt");
        streamReader = reader;
        var writer = new StreamWriter("output.txt");
        SharedWriter = writer;
    }

    public static BufferedStream UsedAsAParameter()
    {
        // Replacing 'var' with 'using var' makes no sense.
        var clientSocket = new Socket(
            AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var b = clientSocket.Connected;
        var netStream = new NetworkStream(clientSocket, true);
        var bufStream = new BufferedStream(netStream, 1024);
        return bufStream;
    }

    // A factory method that returns a new IDisposable object.
    public static Socket? Returned(Uri uri, int port)
    {
        // Replacing 'var' with 'using var' makes no sense.
        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        try
        {
            socket.Connect(uri.Host, port);
            return socket;
        }
        catch (Exception)
        {
            socket.Dispose();
        }
        return null;
    }

    public static void Reassigned()
    {
        // Replacing 'var' with 'using var' causes an error CS1656.
        var i = new StreamReader("file.txt");
        Console.WriteLine(i.ReadLine());
        i = new StreamReader("another.txt");
        Console.WriteLine(i.ReadLine());
    }

    public static Action Captured()
    {
        var i = new StreamWriter("file.txt");
        return () =>
        {
            i.WriteLine("hello");
        };
    }

    public static void Main()
    {
        using var i = new StreamReader("file.txt");
        // StringReader does not need to be disposed.
        var j = new StringReader("hello");
    }

    public static void ExplicitTypeDeclarations()
    {
        using StreamReader i = new("file.txt");
        using IDisposable j = new StringReader("world");
    }

    public static void Nullable()
    {
        using StreamReader? k = new StreamReader("file.txt");
    }

    public static void NotNewOperator()
    {
        var out1 = Console.Out;
        TextWriter out2 = Console.Out;
        var file1 = NewStreamReader("file.txt");
        StreamReader? file2 = NewStreamReader("file.txt");
    }

    private static StreamReader? NewStreamReader(string path)
    {
        try
        {
            return new StreamReader(path);
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static void Loophole()
    {
        var (r1, r2) = (new StreamReader("1"), new StreamReader("2"));
        TextReader[] array = [new StreamReader("1")];
        var anotherArray = new[] { new StreamReader("1") };
    }
}
