using Monero.Common;
using Monero.Test.Utils;

namespace Monero.Test;

public class TestMoneroRpcConnection
{
    // Can copy connection
    [Fact]
    public void TestClone()
    {
        MoneroRpcConnection connection = new("test", "user", "pass123", "test_zmq", 2);

        MoneroRpcConnection copy = connection.Clone();

        Assert.True(connection.Equals(copy));

        connection = new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI);

        TestConnection(connection, TestUtils.DAEMON_RPC_URI, true);

        copy = connection.Clone();

        Assert.True(connection.Equals(copy));
    }

    // Can connect to node
    [Fact]
    public void TestNodeRpcConnection()
    {
        MoneroRpcConnection connection = new(TestUtils.DAEMON_RPC_URI);

        TestConnection(connection, TestUtils.DAEMON_RPC_URI, true);
    }

    // Can connect to wallet
    [Fact]
    public void TestWalletRpcConnection()
    {
        MoneroRpcConnection connection = new(TestUtils.WALLET_RPC_URI);

        TestConnection(connection, TestUtils.WALLET_RPC_URI, true);
    }

    // Test invalid url
    [Fact]
    public void TestInvalidConnection()
    {
        MoneroRpcConnection connection = new("");

        TestConnection(connection, "", false);
    }

    // Can send request to RPC
    [Fact]
    public void TestSendRequest()
    {
        // Setup connection

        MoneroRpcConnection connection = new(TestUtils.DAEMON_RPC_URI);

        TestConnection(connection, TestUtils.DAEMON_RPC_URI, true);

        // Test monerod JSON request

        MoneroJsonRpcResponse<Dictionary<string, object>> jsonResponse = connection.SendJsonRequest("get_info");

        Assert.NotNull(jsonResponse);
        Assert.Null(jsonResponse.Error);
        Assert.NotNull(jsonResponse.Result);

        // Test monerod PATH request

        Dictionary<string, object> pathResponse = connection.SendPathRequest("get_info");

        Assert.NotNull(pathResponse);
        Assert.False(pathResponse.TryGetValue("error", out object _));

        // Test monerod BINARY request

        // TODO implement MoneroRpcConnection.SendBinaryRequest()

        //var binaryResponse = connection.SendBinaryRequest("get_outs.bin");
        //Assert.NotNull(binaryResponse);
    }

    private static void TestConnection(MoneroRpcConnection? connection, string? uri, bool online)
    {
        Assert.NotNull(connection);

        if (online)
        {
            Assert.NotNull(uri);
            Assert.NotEmpty(uri);
            Assert.Equal(uri, connection.GetUri());
            Assert.True(connection.IsClearnet());
            Assert.False(connection.IsOnion());
            Assert.False(connection.IsI2P());
            Assert.True(connection.CheckConnection());
            Assert.True(connection.IsOnline());
            Assert.True(connection.IsAuthenticated());
            Assert.True(connection.IsConnected());
        }
        else
        {
            Assert.Equal(uri, connection.GetUri());
            Assert.False(connection.IsClearnet());
            Assert.False(connection.IsOnion());
            Assert.False(connection.IsI2P());
            Assert.True(connection.CheckConnection());
            Assert.False(connection.IsOnline());
            Assert.Null(connection.IsAuthenticated());
            Assert.False(connection.IsConnected());
        }
    }
}