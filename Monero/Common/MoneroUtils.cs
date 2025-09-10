using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Monero.Wallet.Common;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities.Encoders;

namespace Monero.Common;

public static class MoneroUtils
{
    private static readonly ulong XMR_AU_MULTIPLIER = 1000000000000;

    public static string GetVersion()
    {
        return "0.0.1";
    }

    public static readonly uint RING_SIZE = 16;
    private static int LOG_LEVEL = 0;
    private static readonly ulong AU_PER_XMR = 1000000000000;
    private static readonly int NUM_MNEMONIC_WORDS = 25;
    private static readonly int VIEW_KEY_LENGTH = 64;
    private static readonly string ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    private static readonly char[] CHARS = ALPHABET.ToCharArray();
    private static readonly int ALPHABET_SIZE = ALPHABET.Length;
    private static readonly Dictionary<int, int> ENCODED_BLOCK_SIZE = new Dictionary<int, int>
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

    private const int FULL_BLOCK_SIZE = 8;
    private const int FULL_ENCODED_BLOCK_SIZE = 11;

    private static readonly ulong UINT64_MAX = ulong.MaxValue;

    private static readonly Regex STANDARD_ADDRESS_PATTERN = new("^[" + ALPHABET + "]{95}$", RegexOptions.Compiled);
    private static readonly Regex INTEGRATED_ADDRESS_PATTERN = new("^[" + ALPHABET + "]{106}$", RegexOptions.Compiled);

    public static bool WalletExists(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        return File.Exists(path);
    }

    private static bool IsHex(string? str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        return Regex.IsMatch(str, @"^-?[0-9a-fA-F]+$");
    }

    private static bool IsHex64(string? str)
    {
        return str != null && str.Length == 64 && IsHex(str);
    }

    public static void ValidateLanguage(string? language)
    {
        throw new NotImplementedException();
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
        if (words.Length != NUM_MNEMONIC_WORDS) throw new MoneroError("Mnemonic phrase is " + words.Length + " words but must be " + NUM_MNEMONIC_WORDS);
    }

    public static void ValidateViewKey(string viewKey)
    {
        if (viewKey == null) throw new MoneroError("View key is null");
        if (viewKey.Length != VIEW_KEY_LENGTH) throw new MoneroError("View key is " + viewKey.Length + " characters but must be " + VIEW_KEY_LENGTH);
    }

    public static bool IsValidLanguage(string? language)
    {
        try
        {
            ValidateLanguage(language);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidViewKey(string viewKey)
    {
        try
        {
            ValidateViewKey(viewKey);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidPrivateViewKey(string? privateViewKey)
    {
        try
        {
            ValidatePrivateViewKey(privateViewKey);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static bool IsValidPublicViewKey(string publicViewKey)
    {
        try
        {
            ValidatePublicViewKey(publicViewKey);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static bool IsValidPrivateSpendKey(string privateSpendKey)
    {
        try
        {
            ValidatePrivateSpendKey(privateSpendKey);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static bool IsValidPublicSpendKey(string publicSpendKey)
    {
        try
        {
            ValidatePublicSpendKey(publicSpendKey);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static void ValidatePrivateViewKey(string? privateViewKey)
    {
        if (!IsHex64(privateViewKey)) throw new MoneroError("private view key expected to be 64 hex characters");
    }

    public static void ValidatePublicViewKey(string publicViewKey)
    {
        if (!IsHex64(publicViewKey)) throw new MoneroError("public view key expected to be 64 hex characters");
    }

    public static void ValidatePrivateSpendKey(string privateSpendKey)
    {
        if (!IsHex64(privateSpendKey)) throw new MoneroError("private spend key expected to be 64 hex characters");
    }

    public static void ValidatePublicSpendKey(string publicSpendKey)
    {
        if (!IsHex64(publicSpendKey)) throw new MoneroError("public spend key expected to be 64 hex characters");
    }

    public static bool IsValidAddress(string? address, MoneroNetworkType networkType)
    {
        try
        {
            ValidateAddress(address, networkType);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static void ValidateAddress(string? address, MoneroNetworkType networkType)
    {
        if (address == null) throw new MoneroError("Address is null");
        DecodeAddress(address);
    }

    public static void ValidatePaymentId(string paymentId)
    {
        bool result = paymentId.Length == 16 || paymentId.Length == 64;

        if (!result) throw new MoneroInvalidPaymentIdError(paymentId);
    }

    public static bool IsValidPaymentId(string paymentId)
    {
        try
        {
            ValidatePaymentId(paymentId);
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static void ValidateHex(string str)
    {
        if (!Regex.IsMatch(str, @"^([0-9A-Fa-f]{2})+$"))
            throw new MoneroError("Invalid hex: " + str);
    }

    public static bool IsValidHex(string str)
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

    public static void ValidateBase58(string standardAddress)
    {
        foreach (char c in standardAddress)
        {
            if (!CHARS.Contains(c))
                throw new MoneroError("Invalid Base58 " + standardAddress);
        }
    }

    public static bool IsValidBase58(string standardAddress)
    {
        try
        {
            ValidateBase58(standardAddress);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void Log(int level, string message)
    {
        if (level < 0) throw new MoneroError("Log level must be an integer >= 0");
        if (LOG_LEVEL >= level) Console.WriteLine(message);
    }

    public static int GetLogLevel() { return LOG_LEVEL; }

    public static void SetLogLevel(int level)
    {
        if (level < 0) throw new MoneroError("Log level must be an integer >= 0");
        LOG_LEVEL = level;
    }

    public static ulong XmrToAtomicUnits(double amountXmr)
    {
        decimal precise = Math.Round((decimal)amountXmr * XMR_AU_MULTIPLIER, 0, MidpointRounding.AwayFromZero);
        return ((ulong)new BigInteger(precise));
    }

    public static double AtomicUnitsToXmr(ulong amountAtomicUnits)
    {
        decimal atomicDecimal = (decimal)amountAtomicUnits;
        decimal result = atomicDecimal / XMR_AU_MULTIPLIER;

        return Math.Round((double)result, 12, MidpointRounding.AwayFromZero);
    }

    public static MoneroDecodedAddress DecodeAddress(string? address)
    {
        if (address == null) throw new MoneroError("Address is null");

        // determine if address has integrated address pattern
        bool isIntegrated = false;
        if (!STANDARD_ADDRESS_PATTERN.IsMatch(address))
        {
            if (INTEGRATED_ADDRESS_PATTERN.IsMatch(address))
            {
                isIntegrated = true;
            }
            else
            {
                throw new MoneroError("Address has invalid regex pattern");
            }
        }

        // decode address to hex string
        string addressHex = DecodeAddressToHex(address);

        // validate address hash
        if (!IsValidAddressHash(addressHex)) throw new MoneroError("Address has invalid hash");

        // get address code
        int addressCode = Convert.ToByte(addressHex.Substring(0, 2), 16);

        // determine network and address types
        MoneroAddressType? addressType = null;
        MoneroNetwork? networkType = null;
        foreach (MoneroNetwork aNetworkType in MoneroNetwork.Types)
        {
            if (addressCode == aNetworkType.GetPrimaryAddressCode())
            {
                if (isIntegrated) throw new MoneroError("Address has primary address code but integrated address pattern");
                addressType = MoneroAddressType.PRIMARY_ADDRESS;
                networkType = aNetworkType;
                break;
            }
            else if (addressCode == aNetworkType.GetIntegratedAddressCode())
            {
                if (!isIntegrated) throw new MoneroError("Address has integrated address code but non-integrated address pattern");
                addressType = MoneroAddressType.INTEGRATED_ADDRESS;
                networkType = aNetworkType;
                break;
            }
            else if (addressCode == aNetworkType.GetSubaddressCode())
            {
                if (isIntegrated) throw new MoneroError("Address has subaddress code but integrated address pattern");
                addressType = MoneroAddressType.SUBADDRESS;
                networkType = aNetworkType;
                break;
            }
        }

        // validate address and network types
        if (addressType == null || networkType == null) throw new MoneroError("Address has invalid code: " + addressCode);

        // return decoded address
        return new MoneroDecodedAddress(address, (MoneroAddressType)addressType, networkType.Type);
    }

    public static MoneroIntegratedAddress GetIntegratedAddress(MoneroNetworkType networkType, string standardAddress, string? paymentId = null)
    {
        throw new NotImplementedException("MoneroUtils.GetIntegratedAddress(): not implemented.");
    }

    public static Uri ParseUri(string uri)
    {
        if (!string.IsNullOrEmpty(uri) && !Regex.IsMatch(uri.ToLower(), @"^\w+://.+"))
        {
            uri = "http://" + uri; // assume http if protocol not given
        }

        try
        {
            return new Uri(uri);
        }
        catch (Exception e)
        {
            throw new MoneroError(e);
        }
    }

    private static bool IsValidAddressHash(string decodedAddrStr)
    {
        if (string.IsNullOrEmpty(decodedAddrStr) || decodedAddrStr.Length < 8)
            return false;

        string checksumCheck = decodedAddrStr.Substring(decodedAddrStr.Length - 8);
        string withoutChecksumStr = decodedAddrStr.Substring(0, decodedAddrStr.Length - 8);

        byte[] withoutChecksumBytes;
        try
        {
            withoutChecksumBytes = Hex.Decode(withoutChecksumStr);
        }
        catch
        {
            return false;
        }

        var digest256 = new KeccakDigest(256);
        byte[] hashBytes = new byte[digest256.GetDigestSize()];
        digest256.BlockUpdate(withoutChecksumBytes, 0, withoutChecksumBytes.Length);
        digest256.DoFinal(hashBytes, 0);

        string encodedStr = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        string hashChecksum = encodedStr.Substring(0, 8);

        return hashChecksum == checksumCheck;
    }

    private static string DecodeAddressToHex(string address)
    {
        int[] bin = new int[address.Length];
        for (int i = 0; i < address.Length; i++)
        {
            bin[i] = address[i];
        }

        int fullBlockCount = (int)Math.Floor((float)bin.Length / FULL_ENCODED_BLOCK_SIZE);
        int lastBlockSize = bin.Length % FULL_ENCODED_BLOCK_SIZE;
        int lastBlockDecodedSize = ENCODED_BLOCK_SIZE[lastBlockSize];
        if (lastBlockDecodedSize < 0)
        {
            throw new ArgumentException("Invalid encoded length");
        }

        int dataSize = fullBlockCount * FULL_BLOCK_SIZE + lastBlockDecodedSize;
        int[] data = new int[dataSize];
        for (int i = 0; i < fullBlockCount; i++)
        {
            data = DecodeBlock(GenUtils.Subarray(bin, i * FULL_ENCODED_BLOCK_SIZE, i * FULL_ENCODED_BLOCK_SIZE + FULL_ENCODED_BLOCK_SIZE), data, i * FULL_BLOCK_SIZE);
        }
        if (lastBlockSize > 0)
        {
            int[] subarray = GenUtils.Subarray(bin, fullBlockCount * FULL_ENCODED_BLOCK_SIZE, fullBlockCount * FULL_ENCODED_BLOCK_SIZE + FULL_BLOCK_SIZE);
            data = DecodeBlock(subarray, data, fullBlockCount * FULL_BLOCK_SIZE);
        }

        return BinToHex(data);
    }

    private static int[] DecodeBlock(int[] data, int[] buf, int index)
    {

        if (data.Length < 1 || data.Length > FULL_ENCODED_BLOCK_SIZE)
        {
            throw new MoneroError("Invalid block length: " + data.Length);
        }

        int resSize = ENCODED_BLOCK_SIZE[data.Length];
        if (resSize <= 0)
        {
            throw new MoneroError("Invalid block size");
        }
        BigInteger resNum = 0;
        BigInteger order = 1;
        for (int i = data.Length - 1; i >= 0; i--)
        {
            int digit = ALPHABET.IndexOf((char)data[i]);
            if (digit < 0)
            {
                throw new MoneroError("Invalid symbol");
            }
            BigInteger product = (order * digit) + resNum;
            // if product > UINT64_MAX
            if (product.CompareTo(UINT64_MAX) > 0)
            {
                throw new MoneroError("Overflow");
            }
            resNum = product;
            order = order * ALPHABET_SIZE;
        }
        if (resSize < FULL_BLOCK_SIZE && (BigInteger.Pow(2, 8 * resSize).CompareTo(resNum) <= 0))
        {
            throw new MoneroError("Overflow 2");
        }

        int[] tmpBuf = Uint64To8be(resNum, resSize);
        for (int j = 0; j < tmpBuf.Length; j++)
        {
            buf[j + index] = tmpBuf[j];
        }

        return buf;
    }

    private static int[] Uint64To8be(BigInteger num, int size)
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
        var builder = new StringBuilder();
        foreach (int i in data)
        {
            builder.Append(i.ToString("x2"));
        }
        return builder.ToString();
    }

    public static Dictionary<string, object> BinaryBlocksToMap(byte[] blocks)
    {
        throw new NotImplementedException("MoneroUtils.BinaryBlocksToMap(): not implemented");
    }

    public static void MergeTx(List<MoneroTx> txs, MoneroTx tx)
    {
        foreach (MoneroTx aTx in txs)
        {
            if (aTx.GetHash()!.Equals(tx.GetHash()))
            {
                aTx.Merge(tx);
                return;
            }
        }
        txs.Add(tx);
    }

    public static void MergeTx(List<MoneroTxWallet> txs, MoneroTxWallet tx)
    {
        foreach (MoneroTx aTx in txs)
        {
            if (aTx.GetHash()!.Equals(tx.GetHash()))
            {
                aTx.Merge(tx);
                return;
            }
        }
        txs.Add(tx);
    }
}
