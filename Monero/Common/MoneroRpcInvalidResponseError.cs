namespace Monero.Common;

public class MoneroRpcInvalidResponseError : MoneroRpcError
{
    public MoneroRpcInvalidResponseError(string method, string? uri, object? parameters) : base(
        $"Received null RPC response from RPC request with method '{method}' to {uri}", -1, method, parameters)
    {
    }
}