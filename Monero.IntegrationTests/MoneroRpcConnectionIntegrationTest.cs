using Monero.Common;
using Monero.Daemon.Common;
using Monero.IntegrationTests.Utils;

using Xunit;

namespace Monero.IntegrationTests;

public class MoneroRpcConnectionIntegrationTest
{

    // Can connect to node
    [Fact]
    public async Task TestNodeRpcConnection()
    {
        MoneroRpcConnection connection = new(TestUtils.DaemonRpcUri, TestUtils.DaemonRpcUsername, TestUtils.DaemonRpcPassword);

        await TestConnection(connection, TestUtils.DaemonRpcUri.AbsoluteUri);
    }

    // Can connect to wallet
    [Fact]
    public async Task TestWalletRpcConnection()
    {
        MoneroRpcConnection connection = new(TestUtils.PrimaryWalletRpcUri, TestUtils.WalletRpcUsername, TestUtils.WalletRpcPassword);

        await TestConnection(connection, TestUtils.PrimaryWalletRpcUri.AbsoluteUri);
    }

    // Can send a request to RPC
    [Fact]
    public async Task TestSendRequest()
    {
        // Setup connection

        MoneroRpcConnection connection = new(TestUtils.DaemonRpcUri, TestUtils.DaemonRpcUsername, TestUtils.DaemonRpcPassword);

        await TestConnection(connection, TestUtils.DaemonRpcUri.AbsoluteUri);

        // Test monerod JSON request

        var jsonResponse = await connection.SendCommandAsync<NoRequestModel, MoneroDaemonInfo>("get_info", NoRequestModel.Instance);

        Assert.NotNull(jsonResponse);
        Assert.Null(jsonResponse.Error);

        // Test monerod PATH request

        MoneroDaemonInfo pathResponse = await connection.SendPathRequest<MoneroDaemonInfo>("get_info", []);

        Assert.NotNull(pathResponse);
        Assert.Null(pathResponse.Error);
    }

    private static Task TestConnection(MoneroRpcConnection? connection, string? uri)
    {
        Assert.NotNull(connection);
        Assert.NotNull(uri);
        Assert.NotEmpty(uri);
        Assert.Equal(uri, connection.GetUri());

        return Task.CompletedTask;
    }
}