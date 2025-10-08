namespace Monero.Common;

public class JsonRpcApiException(JsonRpcResponseError error) : Exception
{
    public JsonRpcResponseError Error { get; set; } = error;

    public override string Message => Error.Message;
}