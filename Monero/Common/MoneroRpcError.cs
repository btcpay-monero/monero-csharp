namespace Monero.Common;

public class MoneroRpcError : MoneroError
{
    private readonly object? _params;
    private readonly string? _rpcMethod;

    public MoneroRpcError(string message) :
        base(message, null)
    {
    }

    public MoneroRpcError(string message, int? code) :
        base(message, code)
    {
    }

    public MoneroRpcError(string message, int? code, string? rpcMethod) :
        base(message, code)
    {
        _rpcMethod = rpcMethod;
    }

    public MoneroRpcError(string message, int? code, string? rpcMethod, object? parameters) :
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