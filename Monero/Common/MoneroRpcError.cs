namespace Monero.Common;

public class MoneroRpcError : MoneroError
{
    private readonly object? _params;
    private readonly string? _rpcMethod;

    public MoneroRpcError(string message, int? code = null, string? rpcMethod = null, object? parameters = null) :
        base(message, code)
    {
        _rpcMethod = rpcMethod;
        _params = parameters;
    }

    public MoneroRpcError(MoneroRpcError e) : base(e)
    {
        _rpcMethod = e._rpcMethod;
        _params = e._params;
    }

    public string GetRpcMethod()
    {
        return _rpcMethod ?? "";
    }

    public object? GetRpcParams()
    {
        return _params;
    }
}