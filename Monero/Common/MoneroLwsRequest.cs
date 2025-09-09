
namespace Monero.Common;

public class MoneroLwsRequest : MoneroHttpRequest
{
    public readonly string Method;

    public MoneroLwsRequest(string method)
    {
        Method = method;
    }
}