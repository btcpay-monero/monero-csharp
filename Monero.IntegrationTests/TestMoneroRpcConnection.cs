using Monero.Common;
using Monero.IntegrationTests.Utils;

using Xunit;

namespace Monero.IntegrationTests;

public class TestMoneroRpcConnection
{
    // Can copy connection
    [Fact]
    public async Task TestClone()
    {
        MoneroRpcConnection connection = new("test", "user", "pass123", "test_zmq", 2);

        MoneroRpcConnection copy = connection.Clone();

        Assert.True(connection.Equals(copy));

        connection = new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI);

        await TestConnection(connection, TestUtils.DAEMON_RPC_URI, true);

        copy = connection.Clone();

        Assert.True(connection.Equals(copy));
    }

    // Can connect to node
    [Fact]
    public async Task TestNodeRpcConnection()
    {
        MoneroRpcConnection connection = new(TestUtils.DAEMON_RPC_URI);

        await TestConnection(connection, TestUtils.DAEMON_RPC_URI, true);
    }

    // Can connect to wallet
    [Fact]
    public async Task TestWalletRpcConnection()
    {
        MoneroRpcConnection connection = new(TestUtils.WALLET_RPC_URI);

        await TestConnection(connection, TestUtils.WALLET_RPC_URI, true);
    }

    // Test invalid url
    [Fact]
    public async Task TestInvalidConnection()
    {
        MoneroRpcConnection connection = new("");

        await TestConnection(connection, "", false);
    }

    // Can send request to RPC
    [Fact]
    public async Task TestSendRequest()
    {
        // Setup connection

        MoneroRpcConnection connection = new(TestUtils.DAEMON_RPC_URI);

        await TestConnection(connection, TestUtils.DAEMON_RPC_URI, true);

        // Test monerod JSON request

        MoneroJsonRpcResponse<Dictionary<string, object?>> jsonResponse = await connection.SendJsonRequest("get_info");

        Assert.NotNull(jsonResponse);
        Assert.Null(jsonResponse.Error);
        Assert.NotNull(jsonResponse.Result);

        // Test monerod PATH request

        Dictionary<string, object?> pathResponse = await connection.SendPathRequest("get_info");

        Assert.NotNull(pathResponse);
        Assert.False(pathResponse.TryGetValue("error", out object? _));

        // TODO implement MoneroRpcConnection.SendBinaryRequest()
    }

    private static async Task TestConnection(MoneroRpcConnection? connection, string? uri, bool online)
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
            Assert.True(await connection.CheckConnection());
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
            Assert.True(await connection.CheckConnection());
            Assert.False(connection.IsOnline());
            Assert.Null(connection.IsAuthenticated());
            Assert.False(connection.IsConnected());
        }
    }
}