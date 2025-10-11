using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Monero.Common;

public static class MoneroUtils
{
    private const int FullBlockSize = 8;
    private const int FullEncodedBlockSize = 11;
    private const ulong XmrAuMultiplier = 1000000000000;

    private const int NumMnemonicWords = 25;
    private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    private static readonly int AlphabetSize = Alphabet.Length;

    private static readonly Dictionary<int, int> EncodedBlockSize = new()
    {
        { 0, 0 },
        { 2, 1 },
        { 3, 2 },
        { 5, 3 },
        { 6, 4 },
        { 7, 5 },
        { 9, 6 },
        { 10, 7 },
        { 11, 8 }
    };

    private const ulong Uint64Max = ulong.MaxValue;

    private static readonly Regex StandardAddressPattern = new("^[" + Alphabet + "]{95}$", RegexOptions.Compiled);
    private static readonly Regex IntegratedAddressPattern = new("^[" + Alphabet + "]{106}$", RegexOptions.Compiled);
    private static readonly int StandardAddressLength = 95;
    private static readonly int IntegratedAddressLength = 106;


    private static bool IsHex(string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return false;
        }

        return Regex.IsMatch(str, "^-?[0-9a-fA-F]+$");
    }

    private static bool IsHex64(string? str)
    {
        return str != null && str.Length == 64 && IsHex(str);
    }

    public static void ValidateMnemonic(string? mnemonic)
    {
        if (mnemonic == null)
        {
            throw new MoneroError("Mnemonic phrase is not initialized");
        }

        if (mnemonic == "")
        {
            throw new MoneroError("Mnemonic phrase is empty");
        }

        string[] words = mnemonic.Split(" ");
        if (words.Length != NumMnemonicWords)
        {
            throw new MoneroError("Mnemonic phrase is " + words.Length + " words but must be " + NumMnemonicWords);
        }
    }

    public static bool IsValidPrivateViewKey(string? privateViewKey)
    {
        try
        {
            ValidatePrivateViewKey(privateViewKey);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool IsValidPublicViewKey(string? publicViewKey)
    {
        try
        {
            ValidatePublicViewKey(publicViewKey);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool IsValidPrivateSpendKey(string? privateSpendKey)
    {
        try
        {
            ValidatePrivateSpendKey(privateSpendKey);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool IsValidPublicSpendKey(string? publicSpendKey)
    {
        try
        {
            ValidatePublicSpendKey(publicSpendKey);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static void ValidatePrivateViewKey(string? privateViewKey)
    {
        if (!IsHex64(privateViewKey))
        {
            throw new MoneroError("private view key expected to be 64 hex characters");
        }
    }

    public static void ValidatePublicViewKey(string? publicViewKey)
    {
        if (!IsHex64(publicViewKey))
        {
            throw new MoneroError("public view key expected to be 64 hex characters");
        }
    }

    public static void ValidatePrivateSpendKey(string? privateSpendKey)
    {
        if (!IsHex64(privateSpendKey))
        {
            throw new MoneroError("private spend key expected to be 64 hex characters");
        }
    }

    public static void ValidatePublicSpendKey(string? publicSpendKey)
    {
        if (!IsHex64(publicSpendKey))
        {
            throw new MoneroError("public spend key expected to be 64 hex characters");
        }
    }

    public static void ValidateHex(string? str)
    {
        if (str == null)
        {
            throw new MoneroError("Invalid hex: null");
        }

        if (!Regex.IsMatch(str, "^([0-9A-Fa-f]{2})+$"))
        {
            throw new MoneroError("Invalid hex: " + str);
        }
    }

    public static bool IsValidHex(string? str)
    {
        try
        {
            ValidateHex(str);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void Log(int level, string message)
    {
        if (level < 0)
        {
            throw new MoneroError("Log level must be an integer >= 0");
        }

        Console.WriteLine(message);
    }

    public static long XmrToAtomicUnits(double amountXmr)
    {
        decimal precise = Math.Round((decimal)amountXmr * XmrAuMultiplier, 0, MidpointRounding.AwayFromZero);
        return (long)new BigInteger(precise);
    }

    public static double AtomicUnitsToXmr(ulong amountAtomicUnits)
    {
        decimal atomicDecimal = amountAtomicUnits;
        decimal result = atomicDecimal / XmrAuMultiplier;

        return Math.Round((double)result, 12, MidpointRounding.AwayFromZero);
    }

    public static MoneroDecodedAddress DecodeAddress(string? address)
    {
        if (address == null)
        {
            throw new MoneroError("Address is null");
        }

        foreach (char c in address)
        {
            if (!Alphabet.Contains(c))
            {
                throw new MoneroError("Invalid character found in address");
            }
        }

        // determine if an address has an integrated address pattern
        bool isIntegrated = false;
        if (!StandardAddressPattern.IsMatch(address))
        {
            if (IntegratedAddressPattern.IsMatch(address) && address.Length == IntegratedAddressLength)
            {
                isIntegrated = true;
            }
            else
            {
                throw new MoneroError("Address has invalid regex pattern");
            }
        }
        else if (address.Length != StandardAddressLength)
        {
            throw new MoneroError("Invalid address length");
        }

        // decode address to hex string
        string addressHex = DecodeAddressToHex(address);

        // validate address hash
        if (!IsValidAddressHash(addressHex))
        {
            throw new MoneroError("Address has invalid hash");
        }

        // get address code
        int addressCode = Convert.ToByte(addressHex.Substring(0, 2), 16);

        // determine network and address types
        MoneroAddressType? addressType = null;
        MoneroNetwork? networkType = null;
        foreach (MoneroNetwork aNetworkType in MoneroNetwork.Types)
        {
            if (addressCode == aNetworkType.GetPrimaryAddressCode())
            {
                if (isIntegrated)
                {
                    throw new MoneroError("Address has primary address code but integrated address pattern");
                }

                addressType = MoneroAddressType.PrimaryAddress;
                networkType = aNetworkType;
                break;
            }

            if (addressCode == aNetworkType.GetIntegratedAddressCode())
            {
                if (!isIntegrated)
                {
                    throw new MoneroError("Address has integrated address code but non-integrated address pattern");
                }

                addressType = MoneroAddressType.IntegratedAddress;
                networkType = aNetworkType;
                break;
            }

            if (addressCode == aNetworkType.GetSubaddressCode())
            {
                if (isIntegrated)
                {
                    throw new MoneroError("Address has subaddress code but integrated address pattern");
                }

                addressType = MoneroAddressType.Subaddress;
                networkType = aNetworkType;
                break;
            }
        }

        // validate address and network types
        if (addressType == null || networkType == null)
        {
            throw new MoneroError("Address has invalid code: " + addressCode);
        }

        // return decoded address
        return new MoneroDecodedAddress(address, (MoneroAddressType)addressType, networkType.Type);
    }

    private static bool IsValidAddressHash(string decodedAddrStr)
    {
        if (string.IsNullOrEmpty(decodedAddrStr) || decodedAddrStr.Length < 8)
        {
            return false;
        }

        return Regex.IsMatch(decodedAddrStr, @"\A\b[0-9a-fA-F]+\b\Z");
    }

    private static string DecodeAddressToHex(string address)
    {
        int[] bin = new int[address.Length];
        for (int i = 0; i < address.Length; i++)
        {
            bin[i] = address[i];
        }

        int fullBlockCount = (int)Math.Floor((float)bin.Length / FullEncodedBlockSize);
        int lastBlockSize = bin.Length % FullEncodedBlockSize;
        int lastBlockDecodedSize = EncodedBlockSize[lastBlockSize];
        if (lastBlockDecodedSize < 0)
        {
            throw new ArgumentException("Invalid encoded length");
        }

        int dataSize = (fullBlockCount * FullBlockSize) + lastBlockDecodedSize;
        int[] data = new int[dataSize];
        for (int i = 0; i < fullBlockCount; i++)
        {
            data = DecodeBlock(
                GenUtils.Subarray(bin, i * FullEncodedBlockSize,
                    (i * FullEncodedBlockSize) + FullEncodedBlockSize), data, i * FullBlockSize);
        }

        if (lastBlockSize > 0)
        {
            int[]? subarray = GenUtils.Subarray(bin, fullBlockCount * FullEncodedBlockSize,
                (fullBlockCount * FullEncodedBlockSize) + FullBlockSize);
            data = DecodeBlock(subarray, data, fullBlockCount * FullBlockSize);
        }

        return BinToHex(data);
    }

    private static int[] DecodeBlock(int[]? data, int[] buf, int index)
    {
        if (data == null)
        {
            throw new MoneroError("Cannot decode null data");
        }

        if (data.Length < 1 || data.Length > FullEncodedBlockSize)
        {
            throw new MoneroError("Invalid block length: " + data.Length);
        }

        int resSize = EncodedBlockSize[data.Length];
        if (resSize <= 0)
        {
            throw new MoneroError("Invalid block size");
        }

        BigInteger resNum = 0;
        BigInteger order = 1;
        for (int i = data.Length - 1; i >= 0; i--)
        {
            int digit = Alphabet.IndexOf((char)data[i]);
            if (digit < 0)
            {
                throw new MoneroError("Invalid symbol");
            }

            BigInteger product = (order * digit) + resNum;
            // if product > UINT64_MAX
            if (product.CompareTo(Uint64Max) > 0)
            {
                throw new MoneroError("Overflow");
            }

            resNum = product;
            order = order * AlphabetSize;
        }

        if (resSize < FullBlockSize && BigInteger.Pow(2, 8 * resSize).CompareTo(resNum) <= 0)
        {
            throw new MoneroError("Overflow 2");
        }

        int[] tmpBuf = Uint64To8Be(resNum, resSize);
        for (int j = 0; j < tmpBuf.Length; j++)
        {
            buf[j + index] = tmpBuf[j];
        }

        return buf;
    }

    private static int[] Uint64To8Be(BigInteger num, int size)
    {
        int[] res = new int[size];
        if (size < 1 || size > 8)
        {
            throw new MoneroError("Invalid input length");
        }

        BigInteger twopow8 = BigInteger.Pow(2, 8);
        for (int i = size - 1; i >= 0; i--)
        {
            res[i] = (int)BigInteger.Remainder(num, twopow8);
            num = BigInteger.Divide(num, twopow8);
        }

        return res;
    }

    private static string BinToHex(int[] data)
    {
        StringBuilder builder = new();
        foreach (int i in data)
        {
            builder.Append(i.ToString("x2"));
        }

        return builder.ToString();
    }
}