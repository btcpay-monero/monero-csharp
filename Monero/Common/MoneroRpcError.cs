namespace Monero.Common
{
    public class MoneroRpcError : MoneroError
    {
        private readonly string _rpcMethod;
        private readonly object? _params;

        public MoneroRpcError(string message, int? code, string rpcMethod, object? parameters = null) : base(message, code)
        {
            _rpcMethod = rpcMethod;
            _params = parameters;
        }

        public string GetRpcMethod()
        {
            return _rpcMethod;
        }

        public object? GetRpcParams() {
            return _params;
        }
    }
}
