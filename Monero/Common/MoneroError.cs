namespace Monero.Common;

public class MoneroError : Exception
{
    private readonly int? _code;

    public MoneroError(string message, int? code = null) : base(message)
    {
        _code = code;
    }

    public MoneroError(Exception e) : base(e.Message) { }

    public MoneroError(MoneroError e) : base(e.Message)
    {
        _code = e._code;
    }

    public int? GetCode()
    {
        return _code;
    }

    public override string ToString()
    {
        return _code.HasValue ? $"{base.ToString()} Code: {_code.Value}" : base.ToString();
    }
}