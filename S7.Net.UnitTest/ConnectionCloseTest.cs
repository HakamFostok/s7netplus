using System.Net.Sockets;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace S7.Net.UnitTest;

/// <summary>
/// Test stream which only gives 1 byte per read.
/// </summary>
class TestStreamConnectionClose : Stream
{
    private readonly CancellationTokenSource _cancellationTokenSource;

    public TestStreamConnectionClose(CancellationTokenSource cancellationTokenSource)
    {
        _cancellationTokenSource = cancellationTokenSource;
    }
    public override bool CanRead => false;

    public override bool CanSeek => throw new NotImplementedException();

    public override bool CanWrite => true;

    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _cancellationTokenSource.Cancel();
    }
}

/// <summary>
/// These tests are intended to test <see cref="StreamExtensions"/> functions and other stream-related special cases.
/// </summary>
[TestClass]
public class ConnectionCloseTest
{
    const short TestServerPort = 31122;
    const string TestServerIp = "127.0.0.1";

    [TestMethod]
    public async Task Test_CancellationDuringTransmission()
    {
        Plc? plc = new(CpuType.S7300, TestServerIp, TestServerPort, 0, 2);

        // Set up a shared cancellation source so we can let the stream
        // initiate cancel after some data has been written to it.
        CancellationTokenSource? cancellationSource = new();
        CancellationToken cancellationToken = cancellationSource.Token;

        TestStreamConnectionClose? stream = new(cancellationSource);
        byte[]? requestData = new byte[100]; // empty data, it does not matter what is in there

        // Set up access to private method and field
        MethodInfo? dynMethod = plc.GetType().GetMethod("NoLockRequestTpduAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (dynMethod is null)
        {
            throw new NullReferenceException("Could not find method 'NoLockRequestTpduAsync' on Plc object.");
        }
        FieldInfo? tcpClientField = plc.GetType().GetField("tcpClient", BindingFlags.NonPublic | BindingFlags.Instance);
        if (tcpClientField is null)
        {
            throw new NullReferenceException("Could not find field 'tcpClient' on Plc object.");
        }

        // Set a value to tcpClient field so we can later ensure that it has been closed.
        tcpClientField.SetValue(plc, new TcpClient());
        object? tcpClientValue = tcpClientField.GetValue(plc);
        Assert.IsNotNull(tcpClientValue);

        try
        {
            Task<COTP.TPDU>? result = (Task<COTP.TPDU>)dynMethod.Invoke(plc, new object[] { stream, requestData, cancellationToken });
            await result;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Task was cancelled as expected.");

            // Ensure that the plc connection was closed since the task was cancelled
            // after data has been sent through the network. We expect that the tcpClient
            // object was set to NULL
            object? tcpClientValueAfter = tcpClientField.GetValue(plc);
            Assert.IsNull(tcpClientValueAfter);
            return;
        }
        catch (Exception e)
        {
            Assert.Fail($"Wrong exception type received. Expected {typeof(OperationCanceledException)}, received {e.GetType()}.");
        }

        // Ensure test fails if cancellation did not occur.
        Assert.Fail("Task was not cancelled as expected.");
    }

    [TestMethod]
    public async Task Test_CancellationBeforeTransmission()
    {
        Plc? plc = new(CpuType.S7300, TestServerIp, TestServerPort, 0, 2);

        // Set up a cancellation source
        CancellationTokenSource? cancellationSource = new();
        CancellationToken cancellationToken = cancellationSource.Token;

        TestStreamConnectionClose? stream = new(cancellationSource);
        byte[]? requestData = new byte[100]; // empty data, it does not matter what is in there

        // Set up access to private method and field
        MethodInfo? dynMethod = plc.GetType().GetMethod("NoLockRequestTpduAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (dynMethod is null)
        {
            throw new NullReferenceException("Could not find method 'NoLockRequestTpduAsync' on Plc object.");
        }
        FieldInfo? tcpClientField = plc.GetType().GetField("tcpClient", BindingFlags.NonPublic | BindingFlags.Instance);
        if (tcpClientField is null)
        {
            throw new NullReferenceException("Could not find field 'tcpClient' on Plc object.");
        }

        // Set a value to tcpClient field so we can later ensure that it has been closed.
        tcpClientField.SetValue(plc, new TcpClient());
        object? tcpClientValue = tcpClientField.GetValue(plc);
        Assert.IsNotNull(tcpClientValue);

        try
        {
            // cancel the task before we start transmitting data
            cancellationSource.Cancel();
            Task<COTP.TPDU>? result = (Task<COTP.TPDU>)dynMethod.Invoke(plc, new object[] { stream, requestData, cancellationToken });
            await result;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Task was cancelled as expected.");

            // Ensure that the plc connection was not closed, since we cancelled the task before
            // sending data through the network. We expect that the tcpClient
            // object was NOT set to NULL
            object? tcpClientValueAfter = tcpClientField.GetValue(plc);
            Assert.IsNotNull(tcpClientValueAfter);
            return;
        }
        catch (Exception e)
        {
            Assert.Fail($"Wrong exception type received. Expected {typeof(OperationCanceledException)}, received {e.GetType()}.");
        }

        // Ensure test fails if cancellation did not occur.
        Assert.Fail("Task was not cancelled as expected.");
    }
}