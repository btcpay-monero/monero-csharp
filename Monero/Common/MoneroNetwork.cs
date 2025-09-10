
namespace Monero.Common;

public abstract class MoneroNetwork
{
    private readonly int primaryAddressCode;
    private readonly int integratedAddressCode;
    private readonly int subaddressCode;

    public readonly MoneroNetworkType Type;

    public MoneroNetwork(int primaryAddressCode, int integratedAddressCode, int subaddressCode, MoneroNetworkType type)
    {
        this.primaryAddressCode = primaryAddressCode;
        this.integratedAddressCode = integratedAddressCode;
        this.subaddressCode = subaddressCode;
        Type = type;
    }

    public int GetPrimaryAddressCode()
    {
        return primaryAddressCode;
    }

    public int GetIntegratedAddressCode()
    {
        return integratedAddressCode;
    }

    public int GetSubaddressCode()
    {
        return subaddressCode;
    }

    public static MoneroNetworkType Parse(string? networkTypeStr)
    {
        if (networkTypeStr == null) throw new MoneroError("Cannot parse null network type");
        return networkTypeStr.ToLower() switch
        {
            "mainnet" => MoneroNetworkType.MAINNET,
            "fakechain" => MoneroNetworkType.MAINNET,
            "testnet" => MoneroNetworkType.TESTNET,
            "stagenet" => MoneroNetworkType.STAGENET,
            _ => throw new MoneroError("Invalid network type to parse: " + networkTypeStr),
        };
    }

    public static MoneroNetworkType Parse(int? netttype)
    {
        if (netttype == null) throw new MoneroError("Cannot parse null network type");
        if (netttype == 0) return MoneroNetworkType.MAINNET;
        else if (netttype == 1) return MoneroNetworkType.TESTNET;
        else return MoneroNetworkType.STAGENET;
    }

    public static readonly MoneroNetwork[] Types = [new MoneroNetworkMainnet(), new MoneroNetworkTestnet(), new MoneroNetworkStagenet()];
}

public class MoneroNetworkMainnet : MoneroNetwork
{
    public MoneroNetworkMainnet() : base(18, 19, 42, MoneroNetworkType.MAINNET) { }
}

public class MoneroNetworkTestnet : MoneroNetwork
{
    public MoneroNetworkTestnet() : base(53, 54, 63, MoneroNetworkType.TESTNET) { }
}

public class MoneroNetworkStagenet : MoneroNetwork
{
    public MoneroNetworkStagenet() : base(24, 25, 36, MoneroNetworkType.STAGENET) { }
}