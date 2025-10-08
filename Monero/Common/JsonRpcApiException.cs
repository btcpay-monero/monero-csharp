namespace Monero.Common;

public class JsonRpcApiException(JsonRpcResultError error) : Exception
{
    public JsonRpcResultError Error { get; set; } = error;

    public override string Message => Error.Message;
}