namespace Monero.Common;

public class JsonRpcApiException(JsonRpcResultError error) : Exception
{
    private JsonRpcResultError Error { get; set; } = error;

    public override string Message => Error.Message;
}