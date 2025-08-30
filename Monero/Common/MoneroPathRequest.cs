using Newtonsoft.Json;

namespace Monero.Common
{
    public class MoneroPathRequest : MoneroHttpRequest
    {
        [JsonProperty("method", Order = 1)]
        public readonly string Method;

        [JsonProperty("params", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        public object? Params;

        public MoneroPathRequest(string method, object? parameters = null)
        {
            Method = method;
            Params = parameters;
        }
    }

}
