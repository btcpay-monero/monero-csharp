namespace Monero.Common;

public abstract class MoneroNetwork
{
    public static readonly MoneroNetwork[] Types =
        [new MoneroNetworkMainnet(), new MoneroNetworkTestnet(), new MoneroNetworkStagenet()];

    private readonly int _integratedAddressCode;
    private readonly int _primaryAddressCode;
    private readonly int _subaddressCode;

    public readonly MoneroNetworkType Type;

    protected MoneroNetwork(int primaryAddressCode, int integratedAddressCode, int subaddressCode, MoneroNetworkType type)
    {
        this._primaryAddressCode = primaryAddressCode;
        this._integratedAddressCode = integratedAddressCode;
        this._subaddressCode = subaddressCode;
        Type = type;
    }

    public int GetPrimaryAddressCode()
    {
        return _primaryAddressCode;
    }

    public int GetIntegratedAddressCode()
    {
        return _integratedAddressCode;
    }

    public int GetSubaddressCode()
    {
        return _subaddressCode;
    }

    public static MoneroNetworkType Parse(string? networkTypeStr)
    {
        if (networkTypeStr == null)
        {
            throw new MoneroError("Cannot parse null network type");
        }

        return networkTypeStr.ToLower() switch
        {
            "mainnet" => MoneroNetworkType.Mainnet,
            "fakechain" => MoneroNetworkType.Mainnet,
            "testnet" => MoneroNetworkType.Testnet,
            "stagenet" => MoneroNetworkType.Stagenet,
            _ => throw new MoneroError("Invalid network type to parse: " + networkTypeStr)
        };
    }

    public static MoneroNetworkType Parse(int? netttype)
    {
        if (netttype == null)
        {
            throw new MoneroError("Cannot parse null network type");
        }

        if (netttype == 0)
        {
            return MoneroNetworkType.Mainnet;
        }

        if (netttype == 1)
        {
            return MoneroNetworkType.Testnet;
        }

        return MoneroNetworkType.Stagenet;
    }
}

public class MoneroNetworkMainnet : MoneroNetwork
{
    public MoneroNetworkMainnet() : base(18, 19, 42, MoneroNetworkType.Mainnet) { }
}

public class MoneroNetworkTestnet : MoneroNetwork
{
    public MoneroNetworkTestnet() : base(53, 54, 63, MoneroNetworkType.Testnet) { }
}

public class MoneroNetworkStagenet : MoneroNetwork
{
    public MoneroNetworkStagenet() : base(24, 25, 36, MoneroNetworkType.Stagenet) { }
}