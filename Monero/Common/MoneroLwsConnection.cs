
namespace Monero.Common;

public class MoneroLwsConnection : MoneroConnection
{
    public override bool CheckConnection(ulong timeoutMs)
    {
        throw new NotImplementedException();
    }

    public override MoneroConnection Clone()
    {
        throw new NotImplementedException();
    }

    public override bool? IsAuthenticated()
    {
        throw new NotImplementedException();
    }

    public override bool? IsConnected()
    {
        throw new NotImplementedException();
    }
}