namespace Monero.Common;

public class MoneroRpcInvalidResponseError(string method, Uri uri, object? parameters) : MoneroRpcError(
    $"Received null RPC response from RPC request with method '{method}' to {uri.AbsoluteUri}", -1, method, parameters);