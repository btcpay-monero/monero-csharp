using Monero.Common;
using Monero.Daemon;
using Monero.Daemon.Common;
using Monero.Daemon.Rpc;
using Monero.IntegrationTests.Utils;

using NUnit.Framework;

namespace Monero.IntegrationTests;

public class MoneroDaemonRpcIntegrationTest : MoneroIntegrationTestBase
{

    #region Notification Tests

    // Can notify listeners when a new block is added to the chain
    [Test]
    public async Task TestBlockListener()
    {
        try
        {
            // register a listener
            MoneroDaemonListener listener = new();
            Daemon.AddListener(listener);

            // wait for the next block notification
            MoneroBlockHeader header = await Daemon.WaitForNextBlockHeader();
            Daemon.RemoveListener(listener); // unregister listener so daemon does not keep polling
            TestBlockHeader(header, true);

            // test that listener was called with the equivalent header
            Assert.That(header.Equals(listener.GetLastBlockHeader()));
        }
        finally
        {
            // stop mining
            try { await Daemon.StopMining(); }
            catch (MoneroError)
            {
                // ignore
            }
        }
    }

    #endregion

    #region Non Relays Tests

    // Can send a request to RPC
    [Test]
    public async Task TestSendRequest()
    {
        // Test monerod JSON request
        var jsonResponse = await Daemon.GetInfo();

        Assert.That(jsonResponse, Is.Not.Null);
        Assert.That(jsonResponse.Error, Is.Null);

        // Test monerod PATH request

        MoneroDaemonInfo pathResponse =
            await Daemon.GetRpcConnection().SendPathRequest<MoneroDaemonInfo>("get_info", []);

        Assert.That(pathResponse, Is.Not.Null);
        Assert.That(pathResponse.Error, Is.Null);
    }

    [Test]
    public async Task TestGetVersion()
    {
        MoneroVersion version = await Daemon.GetVersion();
        Assert.That(version.Number, Is.Not.Null);
        Assert.That(version.Number, Is.GreaterThan(0));
        Assert.That(version.IsRelease, Is.Not.Null);
    }

    // Can get the blockchain height
    [Test]
    public async Task TestGetHeight()
    {
        ulong height = await Daemon.GetHeight();
        Assert.That(height, Is.GreaterThan(0), "Height must be greater than 0");
    }

    // Can get a block hash by height
    [Test]
    public async Task TestGetBlockIdByHeight()
    {
        MoneroBlockHeader lastHeader = await Daemon.GetLastBlockHeader();
        string hash = await Daemon.GetBlockHash((ulong)lastHeader.GetHeight()!);
        Assert.That(hash, Is.Not.Null);
        Assert.That(hash.Length, Is.EqualTo(64));
    }

    // Can get a block template
    [Test]
    public async Task TestGetBlockTemplate()
    {
        MoneroBlockTemplate template = await Daemon.GetBlockTemplate(TestUtils.Address, 2);
        TestBlockTemplate(template);
    }

    // Can get the last block's header
    [Test]
    public async Task TestGetLastBlockHeader()
    {
        MoneroBlockHeader lastHeader = await Daemon.GetLastBlockHeader();
        TestBlockHeader(lastHeader, true);
    }

    // Can get a block header by hash
    [Test]
    public async Task TestGetBlockHeaderByHash()
    {
        // retrieve by hash of the last block
        MoneroBlockHeader lastHeader = await Daemon.GetLastBlockHeader();
        string hash = await Daemon.GetBlockHash((ulong)lastHeader.GetHeight()!);
        MoneroBlockHeader header = await Daemon.GetBlockHeaderByHash(hash);
        TestBlockHeader(header, true);
        Assert.That(header, Is.EqualTo(lastHeader));

        // retrieve by hash of previous to last block
        hash = await Daemon.GetBlockHash((ulong)lastHeader.GetHeight()! - 1);
        header = await Daemon.GetBlockHeaderByHash(hash);
        TestBlockHeader(header, true);
        Assert.That(lastHeader.GetHeight() - 1 == (ulong)header.GetHeight()!, Is.True);
    }

    // Can get a block header by height
    [Test]
    public async Task TestGetBlockHeaderByHeight()
    {
        // retrieve by height of the last block
        MoneroBlockHeader lastHeader = await Daemon.GetLastBlockHeader();
        MoneroBlockHeader header = await Daemon.GetBlockHeaderByHeight((ulong)lastHeader.GetHeight()!);
        TestBlockHeader(header, true);
        Assert.That(header, Is.EqualTo(lastHeader));

        // retrieve by height of previous to last block
        header = await Daemon.GetBlockHeaderByHeight((ulong)lastHeader.GetHeight()! - 1);
        TestBlockHeader(header, true);
        Assert.That((ulong)header.GetHeight()!, Is.EqualTo(lastHeader.GetHeight() - 1));
    }

    // Can get block headers by range
    // TODO: test start with no end, vice versa, inclusivity
    [Test]
    public async Task TestGetBlockHeadersByRange()
    {
        // determine start and end height based on the number of blocks and how many blocks ago
        ulong numBlocks = 10;
        ulong numBlocksAgo = 10;
        ulong currentHeight = await Daemon.GetHeight();
        ulong startHeight = currentHeight - numBlocksAgo;
        ulong endHeight = currentHeight - 1;

        // fetch headers
        List<MoneroBlockHeader> headers = await
            Daemon.GetBlockHeadersByRange(startHeight, endHeight);

        // test headers
        Assert.That((ulong)headers.Count, Is.EqualTo(numBlocks));
        int j = 0;
        for (ulong i = 0; i < numBlocks; i++)
        {
            MoneroBlockHeader header = headers[j];
            Assert.That((ulong)header.GetHeight()!, Is.EqualTo(startHeight + i));
            TestBlockHeader(header, true);
            j++;
        }
    }

    // Can get a block by hash
    [Test]
    public async Task TestGetBlockByHash()
    {
        // test config
        TestContext ctx = new() { HasHex = true, HasTxs = false, HeaderIsFull = true };

        // retrieve by hash of the last block
        MoneroBlockHeader lastHeader = await Daemon.GetLastBlockHeader();
        string hash = await Daemon.GetBlockHash((ulong)lastHeader.GetHeight()!);
        MoneroBlock block = await Daemon.GetBlockByHash(hash);
        TestBlock(block, ctx);
        Assert.That((await Daemon.GetBlockByHeight((ulong)block.GetHeight()!)).Equals(block), Is.True);
        Assert.That(block.Txs, Is.Null);

        // retrieve by hash of previous to last block
        hash = await Daemon.GetBlockHash((ulong)lastHeader.GetHeight()! - 1);
        block = await Daemon.GetBlockByHash(hash);
        TestBlock(block, ctx);
        Assert.That((await Daemon.GetBlockByHeight((ulong)lastHeader.GetHeight()! - 1)).Equals(block), Is.True);
        Assert.That(block.Txs, Is.Null);
    }

    // Can get blocks by hash which includes transactions (binary)
    [Test]
    [Ignore("Binary request not implemented")]
    public Task TestGetBlocksByHashBinary()
    {
        throw new MoneroError("Not implemented");
    }

    // Can get a block by height
    [Test]
    public async Task TestGetBlockByHeight()
    {
        // config for testing blocks
        TestContext ctx = new();
        ctx.HasHex = true;
        ctx.HeaderIsFull = true;
        ctx.HasTxs = false;

        // retrieve by height of the last block
        MoneroBlockHeader lastHeader = await Daemon.GetLastBlockHeader();
        MoneroBlock block = await Daemon.GetBlockByHeight((ulong)lastHeader.GetHeight()!);
        TestBlock(block, ctx);
        Assert.That((await Daemon.GetBlockByHeight((ulong)block.GetHeight()!)).Equals(block), Is.True);

        // retrieve by height of previous to last block
        block = await Daemon.GetBlockByHeight((ulong)lastHeader.GetHeight()! - 1);
        TestBlock(block, ctx);
        Assert.That(lastHeader.GetHeight() - 1 == (ulong)block.GetHeight()!, Is.True);
    }

    // Can get a transaction by hash with and without pruning
    [Test]
    public async Task TestGetTxByHash()
    {
        // fetch transaction hashes to test
        List<string> txHashes = await GetConfirmedTxHashes(Daemon);

        // context for testing txs
        TestContext ctx = new() { IsPruned = false, IsConfirmed = true, FromGetTxPool = false };

        // fetch each tx by hash without pruning
        if (txHashes.Count > 0)
        {
            List<MoneroTx> txs = await Daemon.GetTxs(txHashes, false);
            foreach (MoneroTx tx in txs)
            {
                TestTx(tx, ctx);
            }
        }

        // fetch each tx by hash with pruning
        if (txHashes.Count > 0)
        {
            List<MoneroTx> prunedTxs = await Daemon.GetTxs(txHashes, true);
            foreach (MoneroTx tx in prunedTxs)
            {
                ctx.IsPruned = true;
                TestTx(tx, ctx);
            }
        }

        // fetch invalid hash
        try
        {
            await Daemon.GetTxs(["invalid tx hash"], false);
            throw new MoneroError("fail");
        }
        catch (MoneroError e)
        {
            Assert.That(e.Message, Is.EqualTo("Invalid transaction hash"));
        }
    }

    // Can get a transaction hex by hash with and without pruning
    [Test]
    public async Task TestGetTxHexByHash()
    {
        // fetch transaction hashes to test
        List<string> txHashes = await GetConfirmedTxHashes(Daemon);

        // fetch each tx hex by hash with and without pruning
        List<string> hexes = [];
        List<string> hexesPruned = [];
        if (txHashes.Count > 0)
        {
            hexes.AddRange(await Daemon.GetTxHexes(txHashes, false));
            hexesPruned.AddRange(await Daemon.GetTxHexes(txHashes, true));
        }

        // test results
        TestTxHexes(hexes, hexesPruned, txHashes);

        // fetch invalid hash
        try
        {
            await Daemon.GetTxHexes(["invalid tx hash"], false);
            throw new MoneroError("fail");
        }
        catch (MoneroError ex)
        {
            Assert.That(ex.Message, Is.EqualTo("Invalid transaction hash"));
        }
    }

    // Can get transaction hexes by hashes with and without pruning
    [Test]
    [Ignore("Needs monero-wallet-rpc")]
    public async Task TestGetTxHexesByHashes()
    {
        // fetch transaction hashes to test
        List<string> txHashes = await GetConfirmedTxHashes(Daemon);

        // fetch tx hexes by hash with and without pruning
        List<string> hexes = await Daemon.GetTxHexes(txHashes, false);
        List<string> hexesPruned = await Daemon.GetTxHexes(txHashes, true);

        // test results
        TestTxHexes(hexes, hexesPruned, txHashes);

        // fetch invalid hash
        txHashes.Add("invalid tx hash");
        try
        {
            await Daemon.GetTxHexes(txHashes, false);
            throw new MoneroError("fail");
        }
        catch (MoneroError ex)
        {
            Assert.That(ex.Message, Is.EqualTo("Invalid transaction hash"));
        }
    }

    // Can get the miner transaction sum
    [Test]
    [Ignore("Not supported by regtest daemon")]
    public async Task TestGetMinerTxSum()
    {
        MoneroMinerTxSum sum = await Daemon.GetMinerTxSum(0, Math.Min(50000, await Daemon.GetHeight()));
        TestMinerTxSum(sum);
    }

    // Can get a fee estimate
    [Test]
    [Ignore("Not supported by testnet daemon")]
    public async Task TestGetFeeEstimate()
    {
        GetFeeEstimateResponse feeEstimateResponse = await Daemon.GetFeeEstimate(null);
        TestUtils.TestUnsignedBigInteger(feeEstimateResponse.Fee, true);
        Assert.That(feeEstimateResponse.Fees?.Count, Is.EqualTo(4)); // slow, normal, fast, fastest
        for (int i = 0; i < 4; i++)
        {
            TestUtils.TestUnsignedBigInteger(feeEstimateResponse?.Fees?[i], true);
        }

        TestUtils.TestUnsignedBigInteger(feeEstimateResponse?.QuantizationMask, true);
    }

    // Can get hashes of transactions in the transaction pool (binary)
    [Test]
    [Ignore("Binary request not implemented")]
    public Task TestGetIdsOfTxsInPoolBin()
    {
        // TODO: get_transaction_pool_hashes.bin
        throw new MoneroError("Not implemented");
    }

    // Can get the transaction pool backlog (binary)
    [Test]
    [Ignore("Binary request not implemented")]
    public Task TestGetTxPoolBacklogBin()
    {
        // TODO: get_txpool_backlog
        throw new MoneroError("Not implemented");
    }

    // Can get output indices given a list of transaction hashes (binary)
    [Test]
    [Ignore("Binary request not implemented")]
    public Task TestGetOutputIndicesFromTxIdsBinary()
    {
        throw new Exception("Not implemented"); // get_o_indexes.bin
    }

    // Can get outputs given a list of output amounts and indices (binary)
    [Test]
    [Ignore("Binary request not implemented")]
    public Task TestGetOutputsFromAmountsAndIndicesBinary()
    {
        throw new Exception("Not implemented"); // get_outs.bin
    }

    // Can get an output histogram (binary)
    [Test]
    [Ignore("Binary request not implemented")]
    public async Task TestGetOutputHistogramBinary()
    {
        List<MoneroOutputHistogramEntry> entries = await
            Daemon.GetOutputHistogram([], null, null, null, null);
        Assert.That(entries.Count, Is.GreaterThan(0));
        foreach (MoneroOutputHistogramEntry entry in entries)
        {
            TestOutputHistogramEntry(entry);
        }
    }

    // Can get an output distribution (binary)
    [Test]
    [Ignore("Binary request not implemented")]
    public async Task TestGetOutputDistributionBinary()
    {
        List<ulong> amounts =
        [
            0,
            1,
            10,
            100,
            1000,
            10000,
            100000,
            1000000
        ];
        List<MoneroOutputDistributionEntry> entries = await Daemon.GetOutputDistribution(amounts, false, null, null);
        foreach (MoneroOutputDistributionEntry entry in entries)
        {
            TestOutputDistributionEntry(entry);
        }
    }

    // Can get general information
    [Test]
    public async Task TestGetGeneralInformation()
    {
        MoneroDaemonInfo info = await Daemon.GetInfo();
        TestInfo(info);
    }

    // Can get sync information
    [Test]
    public async Task TestGetSyncInformation()
    {
        MoneroDaemonSyncInfo syncInfo = await Daemon.GetSyncInfo();
        TestSyncInfo(syncInfo);
    }

    // Can get hard fork information
    [Test]
    public async Task TestGetHardForkInformation()
    {
        MoneroHardForkInfo hardForkInfo = await Daemon.GetHardForkInfo();
        TestHardForkInfo(hardForkInfo);
    }

    // Can get alternative chains
    [Test]
    public async Task TestGetAlternativeChains()
    {
        List<MoneroAltChain> altChains = await Daemon.GetAltChains();
        foreach (MoneroAltChain altChain in altChains)
        {
            TestAltChain(altChain);
        }
    }

    // Can get alternative block hashes
    [Test]
    public async Task TestGetAlternativeBlockIds()
    {
        List<string> altBlockIds = await Daemon.GetAltBlockHashes();
        foreach (string altBlockId in altBlockIds)
        {
            Assert.That(altBlockId, Is.Not.Null);
            Assert.That(altBlockId.Length, Is.EqualTo(64)); // TODO: common validation
        }
    }

    // Can get, set, and reset a download bandwidth limit
    [Test]
    public async Task TestSetDownloadBandwidth()
    {
        int initVal = await Daemon.GetDownloadLimit();
        Assert.That(initVal, Is.GreaterThan(0));
        int setVal = initVal * 2;
        await Daemon.SetDownloadLimit(setVal);
        Assert.That(await Daemon.GetDownloadLimit(), Is.EqualTo(setVal));
        int resetVal = await Daemon.ResetDownloadLimit();
        Assert.That(resetVal, Is.EqualTo(initVal));

        // test invalid limits
        try
        {
            await Daemon.SetDownloadLimit(0);
            throw new MoneroError("Should have thrown error on invalid input");
        }
        catch (MoneroError e)
        {
            Assert.That(e.Message, Is.EqualTo("Download limit must be an integer greater than 0"));
        }

        Assert.That(await Daemon.GetDownloadLimit(), Is.EqualTo(initVal));
    }

    // Can get, set, and reset an upload bandwidth limit
    [Test]
    public async Task TestSetUploadBandwidth()
    {
        int initVal = await Daemon.GetUploadLimit();
        Assert.That(initVal, Is.GreaterThan(0));
        int setVal = initVal * 2;
        await Daemon.SetUploadLimit(setVal);
        Assert.That(await Daemon.GetUploadLimit(), Is.EqualTo(setVal));
        int resetVal = await Daemon.ResetUploadLimit();
        Assert.That(resetVal, Is.EqualTo(initVal));

        // test invalid limits
        try
        {
            await Daemon.SetUploadLimit(0);
            throw new Exception("Should have thrown error on invalid input");
        }
        catch (MoneroError ex)
        {
            Assert.That(ex!.Message, Is.EqualTo("Upload limit must be an integer greater than 0"));
        }

        Assert.That(await Daemon.GetUploadLimit(), Is.EqualTo(initVal));
    }

    // Can get peers with active incoming or outgoing connections
    [Test]
    public async Task TestGetPeers()
    {
        List<MoneroPeer> peers = await Daemon.GetPeers();
        Assert.That(peers.Count, Is.GreaterThan(0), "Daemon has no incoming or outgoing peers to test");
        foreach (MoneroPeer peer in peers)
        {
            TestPeer(peer);
        }
    }

    // Can get all known peers that may be online or offline
    [Test]
    [Ignore("Daemon has no known peers to test")]
    public async Task TestGetKnownPeers()
    {
        List<MoneroPeer> peers = await Daemon.GetKnownPeers();
        Assert.That(peers.Count, Is.GreaterThan(0), "Daemon has no known peers to test");
        foreach (MoneroPeer peer in peers)
        {
            TestKnownPeer(peer, false);
        }
    }

    // Can limit the number of outgoing peers
    [Test]
    public async Task TestSetOutgoingPeerLimit()
    {
        await Daemon.SetOutgoingPeerLimit(0);
        await Daemon.SetOutgoingPeerLimit(8);
        await Daemon.SetOutgoingPeerLimit(10);
    }

    // Can limit the number of incoming peers
    [Test]
    public async Task TestSetIncomingPeerLimit()
    {
        await Daemon.SetIncomingPeerLimit(0);
        await Daemon.SetIncomingPeerLimit(8);
        await Daemon.SetIncomingPeerLimit(10);
    }

    // Can ban a peer
    [Test]
    public async Task TestBanPeer()
    {
        // set ban
        MoneroBan ban = new() { Host = "192.168.1.51", IsBanned = true, Seconds = 60 };
        await Daemon.SetPeerBans([ban]);

        // test ban
        List<MoneroBan> bans = await Daemon.GetPeerBans();
        bool found = false;
        foreach (MoneroBan aBan in bans)
        {
            TestMoneroBan(aBan);
            if ("192.168.1.51".Equals(aBan.Host))
            {
                found = true;
            }
        }

        Assert.That(found, Is.True);
    }

    // Can ban peers
    [Test]
    public async Task TestBanPeers()
    {
        // set bans
        MoneroBan ban1 = new() { Host = "192.168.1.52", IsBanned = true, Seconds = 60 };
        MoneroBan ban2 = new() { Host = "192.168.1.53", IsBanned = true, Seconds = 60 };
        List<MoneroBan> bans =
        [
            ban1,
            ban2
        ];
        await Daemon.SetPeerBans(bans);

        // test bans
        bans = await Daemon.GetPeerBans();
        bool found1 = false;
        bool found2 = false;
        foreach (MoneroBan aBan in bans)
        {
            TestMoneroBan(aBan);
            switch (aBan.Host)
            {
                case "192.168.1.52":
                    found1 = true;
                    break;
                case "192.168.1.53":
                    found2 = true;
                    break;
            }
        }

        Assert.That(found1, Is.True);
        Assert.That(found2, Is.True);
    }

    // Can start and stop mining
    [Test]
    [Ignore("Fails on GitHub CI")]
    public async Task TestMining()
    {
        // stop mining at the beginning of the test
        try { await Daemon.StopMining(); }
        catch (MoneroError)
        {
            // ignore
        }

        // generate address to mine to
        // TODO use wallet rpc
        string address = TestUtils.Address;
        // start mining
        await Daemon.StartMining(address, 1, false, true);

        await GenUtils.WaitForAsync(30, NUnit.Framework.TestContext.CurrentContext.CancellationToken);

        // stop mining
        await Daemon.StopMining();
    }

    // Can get mining status
    // TODO why this test fails on github runner?
    [Test]
    [Ignore("Fails on github CI")]
    public async Task TestGetMiningStatus()
    {
        try
        {
            // stop mining at the beginning of the test
            try { await Daemon.StopMining(); }
            catch (MoneroError)
            {
                // ignore
            }

            // test status without mining
            MoneroMiningStatus status = await Daemon.GetMiningStatus();
            Assert.That(status.IsActive, Is.False);
            Assert.That(status.Address, Is.Null);
            Assert.That((long)status.Speed!, Is.EqualTo(0));
            Assert.That((int)status.NumThreads!, Is.EqualTo(0));
            Assert.That(status.IsBackground, Is.Null);

            // test status with mining
            // TODO use wallet rpc address
            string address = TestUtils.Address;
            ulong threadCount = 1;
            bool isBackground = false;
            await Daemon.StartMining(address, threadCount, isBackground, true);
            status = await Daemon.GetMiningStatus();
            Assert.That(status.IsActive, Is.True);
            Assert.That(status.Address, Is.EqualTo(address));
            Assert.That(status.Speed, Is.GreaterThanOrEqualTo(0));
            Assert.That(status.NumThreads, Is.EqualTo(threadCount));
            Assert.That(status.IsBackground, Is.EqualTo(isBackground));
        }
        finally
        {
            // stop mining at the end of the test
            try { await Daemon.StopMining(); }
            catch (MoneroError)
            {
                // ignore
            }
        }
    }

    // Can submit a mined block to the network
    [Test]
    [Ignore("Not supported by regtest daemon")]
    public async Task TestSubmitMinedBlock()
    {
        // get template to mine on
        MoneroBlockTemplate template = await Daemon.GetBlockTemplate(TestUtils.Address, 0);

        // TODO monero rpc: way to get mining nonce when found in order to submit?

        // try to submit a block hashing blob without nonce
        try
        {
            await Daemon.SubmitBlocks([template.BlockTemplateBlob!]);
            throw new Exception("Should have thrown error");
        }
        catch (MoneroRpcError e)
        {
            Assert.That(e.GetCode(), Is.EqualTo(-7));
            Assert.That(e.Message, Is.EqualTo("Block not accepted"));
        }
    }

    // Can prune the blockchain
    [Test]
    [Ignore("Not supported by regtest daemon")]
    public async Task TestPruneBlockchain()
    {
        MoneroPruneResponse response = await Daemon.PruneBlockchain(true);
        if (response.IsPruned == true)
        {
            Assert.That(response.PruningSeed, Is.GreaterThan(0));
        }
        else
        {
            Assert.That(response.PruningSeed, Is.EqualTo(0));
        }
    }

    // Can check for an update
    [Test]
    [Ignore("Unstable update call")]
    public async Task TestCheckForUpdate()
    {
        MoneroDaemonUpdateCheckResponse response = await Daemon.CheckForUpdate();
        TestUpdateCheckResult(response);
    }

    // Can download an update
    [Test]
    [Ignore("Non supported by regtest daemon")]
    public async Task TestDownloadUpdate()
    {
        // download to a default path
        MoneroDaemonUpdateDownloadResponse response = await Daemon.DownloadUpdate("");
        TestUpdateDownloadResult(response, null);

        // download to a defined path
        string path = "test_download_" + DateTime.Now + ".tar.bz2";
        response = await Daemon.DownloadUpdate(path);
        TestUpdateDownloadResult(response, path);

        // test invalid path
        if (response.IsUpdateAvailable == true)
        {
            try
            {
                await Daemon.DownloadUpdate("./ohhai/there");
                throw new Exception("Should have thrown error");
            }
            catch (MoneroRpcError e)
            {
                Assert.That(e.Message, Is.Not.EqualTo("Should have thrown error"));
                Assert.That(e.GetCode(), Is.EqualTo(500)); // TODO monerod: this causes a 500 in daemon rpc
            }
        }
    }

    // Can be stopped
    [Test]
    [Ignore("Disabled")]
    public async Task TestStop()
    {
        // stop the daemon
        await Daemon.Stop();

        // give the daemon time to shut down
        await GenUtils.WaitForAsync(TestUtils.SyncPeriodInMs,
            NUnit.Framework.TestContext.CurrentContext.CancellationToken);
        // try to interact with the daemon
        try
        {
            await Daemon.GetHeight();
            throw new Exception("Should have thrown error");
        }
        catch (MoneroError e)
        {
            Assert.That(e.Message, Is.Not.EqualTo("Should have thrown error"));
        }
    }

    #endregion

    #region Test Helpers

    private static void TestBlockHeader(MoneroBlockHeader? header, bool isFull)
    {
        Assert.That(header, Is.Not.Null);
        Assert.That(header!.GetHeight(), Is.GreaterThanOrEqualTo(0));
        Assert.That(header.GetMajorVersion(), Is.GreaterThan(0));
        Assert.That(header.GetMinorVersion(), Is.GreaterThanOrEqualTo(0));
        if (header.GetHeight() == 0)
        {
            Assert.That(header.GetTimestamp(), Is.EqualTo(0));
        }
        else
        {
            Assert.That(header.GetTimestamp(), Is.GreaterThan(0));
        }

        Assert.That(header.GetPrevHash(), Is.Not.Null);
        Assert.That(header.GetNonce(), Is.Not.Null);
        if (header.GetNonce() == 0)
        {
            MoneroUtils.Log(0,
                "WARNING: header nonce is 0 at height " +
                header.GetHeight()); // TODO (monero-project): why is header nonce 0?
        }
        else
        {
            Assert.That(header.GetNonce(), Is.GreaterThan(0));
        }

        Assert.That(header.GetPowHash(), Is.Not.Null); // never seen defined
        if (isFull)
        {
            Assert.That(header.GetSize(), Is.GreaterThan(0));
            Assert.That(header.GetDepth(), Is.GreaterThanOrEqualTo(0));
            Assert.That(header.GetDifficulty(), Is.GreaterThan(0));
            Assert.That(header.GetCumulativeDifficulty(), Is.GreaterThan(0));
            Assert.That(header.GetHash()!.Length, Is.EqualTo(64));
            Assert.That(header.GetMinerTxHash()!.Length, Is.EqualTo(64));
            Assert.That(header.GetNumTxs(), Is.GreaterThanOrEqualTo(0));
            Assert.That(header.GetOrphanStatus(), Is.Not.Null);
            Assert.That(header.GetReward(), Is.Not.Null);
            Assert.That(header.GetWeight(), Is.Not.Null);
            Assert.That(header.GetWeight(), Is.GreaterThan(0));
        }
        else
        {
            Assert.That(header.GetSize(), Is.Null);
            Assert.That(header.GetDepth(), Is.Null);
            Assert.That(header.GetDifficulty(), Is.Null);
            Assert.That(header.GetCumulativeDifficulty(), Is.Null);
            Assert.That(header.GetHash(), Is.Null);
            Assert.That(header.GetMinerTxHash(), Is.Null);
            Assert.That(header.GetNumTxs(), Is.Null);
            Assert.That(header.GetOrphanStatus(), Is.Null);
            Assert.That(header.GetReward(), Is.Null);
            Assert.That(header.GetWeight(), Is.Null);
        }
    }

    private static async Task<List<string>> GetConfirmedTxHashes(IMoneroDaemon daemon)
    {
        const int numTxs = 5;
        List<string> txHashes = [];
        ulong height = await daemon.GetHeight();
        while (txHashes.Count < numTxs && height > 0)
        {
            MoneroBlock block = await daemon.GetBlockByHeight(--height);
            txHashes.AddRange(block.TxHashes);
        }

        return txHashes;
    }

    private static void TestBlockTemplate(MoneroBlockTemplate template)
    {
        Assert.That(template, Is.Not.Null);
        Assert.That(template.BlockTemplateBlob, Is.Not.Null);
        Assert.That(template.BlockHashingBlob, Is.Not.Null);
        Assert.That(template.Difficulty, Is.Not.Null);
        Assert.That(template.ExpectedReward, Is.Not.Null);
        Assert.That(template.Height, Is.Not.Null);
        Assert.That(template.PrevHash, Is.Not.Null);
        Assert.That(template.ReservedOffset, Is.Not.Null);
        Assert.That(template.SeedHeight, Is.Not.Null);
        // regtest daemon has seed height equal to zero
        Assert.That(template.SeedHash, Is.Not.Null);
        Assert.That(template.SeedHash!.Length, Is.EqualTo(0));
        // next seed hash can be null or initialized  // TODO: test circumstances for each
    }

    // TODO: test block deep copy
    private static void TestBlock(MoneroBlock block, TestContext ctx)
    {
        // test required fields
        Assert.That(block, Is.Not.Null);
        TestBlockHeader(block, ctx.HeaderIsFull == true);

        if (ctx.HasHex == true)
        {
            Assert.That(block.Hex, Is.Not.Null);
            Assert.That(block.Hex!.Length > 1, Is.True);
        }
        else
        {
            Assert.That(block.Hex, Is.Not.Null);
        }

        if (ctx.HasTxs == true)
        {
            Assert.That(ctx.TxContext, Is.Not.Null);
            foreach (MoneroTx tx in block.Txs!)
            {
                Assert.That(block.Equals(tx.GetBlock()), Is.True);
                TestTx(tx, ctx.TxContext);
            }
        }
        else
        {
            Assert.That(ctx.TxContext, Is.Null);
            Assert.That(block.Txs, Is.Null);
        }
    }

    private static void TestTx(MoneroTx? tx, TestContext? ctx)
    {
        // check inputs
        Assert.That(tx, Is.Not.Null);
        Assert.That(ctx, Is.Not.Null);
        Assert.That(ctx!.IsPruned, Is.Not.Null);
        Assert.That(ctx.IsConfirmed, Is.Not.Null);
        Assert.That(ctx.FromGetTxPool, Is.Not.Null);

        // standard across all txs
        Assert.That(tx!.GetHash()!.Length, Is.EqualTo(64));
        if (tx.IsRelayed() == null)
        {
            Assert.That(tx.InTxPool(), Is.True); // TODO monerod: add relayed to get_transactions
        }
        else
        {
            Assert.That(tx.IsRelayed(), Is.Not.Null);
        }

        Assert.That(tx.IsConfirmed(), Is.Not.Null);
        Assert.That(tx.InTxPool(), Is.Not.Null);
        Assert.That(tx.IsMinerTx(), Is.Not.Null);
        Assert.That(tx.IsDoubleSpendSeen(), Is.Not.Null);
        Assert.That(tx.GetVersion(), Is.GreaterThanOrEqualTo(0));
        Assert.That(tx.GetUnlockTime(), Is.GreaterThanOrEqualTo(0));
        Assert.That(tx.GetInputs(), Is.Not.Null);
        Assert.That(tx.GetOutputs(), Is.Not.Null);
        Assert.That(tx.GetExtra()!.Length, Is.GreaterThan(0));
        TestUtils.TestUnsignedBigInteger(tx.GetFee(), true);

        // test presence of output indices
        // TODO: change this over to outputs only
        if (tx.IsMinerTx() == true)
        {
            Assert.That(tx.GetOutputIndices(), Is.Null); // TODO: how to get output indices for miner transactions?
        }

        if (tx.InTxPool() == true || ctx.FromGetTxPool == true || ctx.HasOutputIndices == false)
        {
            Assert.That(tx.GetOutputIndices(), Is.Null);
        }
        else
        {
            Assert.That(tx.GetOutputIndices(), Is.Not.Null);
        }

        if (tx.GetOutputIndices() != null)
        {
            Assert.That(tx.GetOutputIndices()!.Count, Is.GreaterThan(0));
        }

        // test confirmed ctx
        if (ctx.IsConfirmed == true)
        {
            Assert.That(tx!.IsConfirmed(), Is.True);
        }

        if (ctx.IsConfirmed == false)
        {
            Assert.That(tx!.IsConfirmed(), Is.False);
        }

        // test confirmed
        if (tx.IsConfirmed() == true)
        {
            Assert.That(tx.GetBlock(), Is.Not.Null);
            Assert.That(tx.GetBlock()!.Txs!, Does.Contain(tx));
            Assert.That(tx.GetBlock()!.GetHeight(), Is.GreaterThan(0));
            Assert.That(tx.GetBlock()!.GetTimestamp(), Is.GreaterThan(0));
            Assert.That(tx.GetRelay(), Is.True);
            Assert.That(tx.IsRelayed(), Is.True);
            Assert.That(tx.IsFailed(), Is.False);
            Assert.That(tx.InTxPool(), Is.False);
            Assert.That(tx.IsDoubleSpendSeen(), Is.False);
            if (ctx.FromBinaryBlock == true)
            {
                Assert.That(tx.GetNumConfirmations(), Is.Null);
            }
            else
            {
                Assert.That(tx.GetNumConfirmations(), Is.GreaterThan(0));
            }
        }
        else
        {
            Assert.That(tx.GetBlock(), Is.Null);
            Assert.That((long)tx.GetNumConfirmations()!, Is.EqualTo(0));
        }

        // test in tx pool
        if (tx!.InTxPool() == true)
        {
            Assert.That(tx.IsConfirmed(), Is.False);
            Assert.That(tx.IsDoubleSpendSeen(), Is.False);
            Assert.That(tx.GetLastFailedHeight(), Is.Null);
            Assert.That(tx.GetLastFailedHash(), Is.Null);
            Assert.That(tx.GetReceivedTimestamp(), Is.GreaterThan(0));
            if (ctx.FromGetTxPool == true)
            {
                Assert.That(tx.GetSize(), Is.GreaterThan(0));
                Assert.That(tx.GetWeight(), Is.GreaterThan(0));
                Assert.That(tx.IsKeptByBlock(), Is.Not.Null);
                Assert.That(tx.GetMaxUsedBlockHeight(), Is.GreaterThanOrEqualTo(0));
                Assert.That(tx.GetMaxUsedBlockHash(), Is.Not.Null);
            }

            Assert.That(tx.GetLastFailedHeight(), Is.Null);
            Assert.That(tx.GetLastFailedHash(), Is.Null);
        }
        else
        {
            Assert.That(tx.GetLastRelayedTimestamp(), Is.Null);
        }

        // test miner tx
        if (tx.IsMinerTx() == true)
        {
            Assert.That(tx.GetFee(), Is.EqualTo(0));
            Assert.That(tx.GetInputs(), Is.Null);
            Assert.That(tx.GetSignatures(), Is.Null);
        }
        else
        {
            if (tx.GetSignatures() != null)
            {
                Assert.That(tx.GetSignatures()!.Count, Is.GreaterThan(0));
            }
        }

        // test failed
        // TODO: what else to test associated with failed
        if (tx.IsFailed() == true)
        {
            Assert.That(tx.GetReceivedTimestamp(), Is.GreaterThan(0));
        }
        else
        {
            if (tx.IsRelayed() == null)
            {
                Assert.That(tx.GetRelay(), Is.Null); // TODO monerod: add relayed to get_transactions
            }
            else if (tx.IsRelayed() == true)
            {
                Assert.That(tx.IsDoubleSpendSeen(), Is.False);
            }
            else
            {
                Assert.That(tx.IsRelayed(), Is.False);
                if (ctx!.FromGetTxPool == true)
                {
                    Assert.That(tx.GetRelay(), Is.False);
                    Assert.That(tx.IsDoubleSpendSeen(), Is.Not.Null);
                }
            }
        }

        Assert.That(tx!.GetLastFailedHeight(), Is.Null);
        Assert.That(tx.GetLastFailedHash(), Is.Null);

        // received time only for tx pool or failed txs
        if (tx.GetReceivedTimestamp() != null)
        {
            Assert.That(tx.InTxPool() == true || tx.IsFailed() == true, Is.True);
        }

        // test inputs and outputs
        if (tx.IsMinerTx() == false)
        {
            Assert.That(tx.GetInputs()!.Count, Is.GreaterThan(0));
        }

        foreach (MoneroOutput input in tx.GetInputs()!)
        {
            Assert.That(tx, Is.EqualTo(input.GetTx()));
            TestInput(input);
        }

        Assert.That(tx.GetOutputs()!.Count, Is.GreaterThan(0));
        foreach (MoneroOutput output in tx.GetOutputs()!)
        {
            Assert.That(tx, Is.EqualTo(output.GetTx()));
            TestOutput(output, ctx);
        }

        // test pruned vs not pruned
        if (ctx.FromGetTxPool == true || ctx.FromBinaryBlock == true)
        {
            Assert.That(tx!.GetPrunableHash(),
                Is.Null); // TODO monerod: tx pool txs do not have prunable hash, TODO: GetBlocksByHeight() has inconsistent client-side pruning
        }
        else
        {
            Assert.That(tx!.GetPrunableHash(), Is.Not.Null);
        }

        if (ctx.IsPruned == true)
        {
            Assert.That(tx.GetRctSigPrunable(), Is.Null);
            Assert.That(tx.GetSize(), Is.Null);
            Assert.That(tx.GetLastRelayedTimestamp(), Is.Null);
            Assert.That(tx.GetReceivedTimestamp(), Is.Null);
            Assert.That(tx.GetFullHex(), Is.Null);
            Assert.That(tx.GetPrunedHex(), Is.Not.Null);
        }
        else
        {
            Assert.That(tx!.GetPrunedHex(), Is.Null);
            if (ctx.FromBinaryBlock == true)
            {
                Assert.That(tx.GetFullHex(), Is.Null); // TODO: GetBlocksByHeight() has inconsistent client-side pruning
            }
            else
            {
                Assert.That(tx.GetFullHex()!.Length, Is.GreaterThan(0));
            }

            if (ctx.FromBinaryBlock == true)
            {
                Assert.That(tx.GetRctSigPrunable(),
                    Is.Null); // TODO: GetBlocksByHeight() has inconsistent client-side pruning
            }

            //else Assert.NotNull((tx.GetRctSigPrunable()); // TODO: define and test this
            Assert.That(tx.IsDoubleSpendSeen(), Is.False);
            if (tx.IsConfirmed() == true)
            {
                Assert.That(tx.GetLastRelayedTimestamp(), Is.Null);
                Assert.That(tx.GetReceivedTimestamp(), Is.Null);
            }
            else
            {
                if (tx.IsRelayed() == true)
                {
                    Assert.That(tx.GetLastRelayedTimestamp(), Is.GreaterThan(0));
                }
                else
                {
                    Assert.That(tx.GetLastRelayedTimestamp(), Is.Null);
                }

                Assert.That(tx.GetReceivedTimestamp(), Is.GreaterThan(0));
            }
        }

        if (tx.IsFailed() == true)
        {
            // TODO: implement this
        }

        // test deep copy
        if (true != ctx.DoNotTestCopy)
        {
            TestTxCopy(tx, ctx);
        }
    }

    private static void TestInput(MoneroOutput input)
    {
        TestOutput(input);
        TestKeyImage(input.GetKeyImage()!);
        Assert.That(input.GetRingOutputIndices()!.Count, Is.GreaterThan(0));
    }

    private static void TestKeyImage(MoneroKeyImage image)
    {
        Assert.That(image.Hex!.Length, Is.GreaterThan(0));
        if (image.Signature != null)
        {
            Assert.That(image.Signature, Is.Not.Null);
            Assert.That(image.Signature!.Length, Is.GreaterThan(0));
        }
    }

    private static void TestOutput(MoneroOutput output, TestContext ctx)
    {
        TestOutput(output);
        if (output.GetTx()!.InTxPool() == true || ctx.HasOutputIndices == false)
        {
            Assert.That(output.GetIndex(), Is.Null);
        }
        else
        {
            Assert.That(output.GetIndex(), Is.GreaterThanOrEqualTo(0));
        }

        Assert.That(output.GetStealthPublicKey()!.Length, Is.EqualTo(64));
    }

    private static void TestOutput(MoneroOutput output)
    {
        TestUtils.TestUnsignedBigInteger(output.GetAmount());
    }

    private static void TestTxCopy(MoneroTx tx, TestContext ctx)
    {
        // copy tx and assert deep equality
        MoneroTx copy = tx.Clone();
        Assert.That(copy.GetBlock(), Is.Null);

        Assert.That(tx.ToString(), Is.EqualTo(copy.ToString()));
        Assert.That(copy, Is.Not.SameAs(tx));


        // test different input references
        if (copy.GetInputs() == null)
        {
            Assert.That(tx.GetInputs(), Is.Null);
        }
        else
        {
            Assert.That(copy.GetInputs(), Is.Not.SameAs(tx.GetInputs()));
            for (int i = 0; i < copy.GetInputs()!.Count; i++)
            {
                Assert.That(tx.GetInputs()![i].GetAmount().Equals(copy.GetInputs()![i].GetAmount()), Is.True);
            }
        }

        // test different output references
        if (copy.GetOutputs() == null)
        {
            Assert.That(tx.GetOutputs(), Is.Null);
        }
        else
        {
            Assert.That(copy.GetOutputs(), Is.Not.SameAs(tx.GetOutputs()));
            for (int i = 0; i < copy.GetOutputs()!.Count; i++)
            {
                Assert.That(tx.GetOutputs()![i].GetAmount().Equals(copy.GetOutputs()![i].GetAmount()), Is.True);
            }
        }

        // test copied tx
        ctx = new TestContext(ctx)
        {
            DoNotTestCopy = true // to prevent infinite recursion
        };

        TestTx(copy, ctx);
    }

    private static void TestMinerTxSum(MoneroMinerTxSum txSum)
    {
        TestUtils.TestUnsignedBigInteger(txSum.EmissionSum, true);
        TestUtils.TestUnsignedBigInteger(txSum.FeeSum, true);
    }

    private static void TestOutputDistributionEntry(MoneroOutputDistributionEntry entry)
    {
        TestUtils.TestUnsignedBigInteger(entry.Amount);
        Assert.That(entry.Base, Is.GreaterThanOrEqualTo(0));
        Assert.That(entry.Distribution!.Count, Is.GreaterThan(0));
        Assert.That(entry.StartHeight, Is.GreaterThanOrEqualTo(0));
    }

    private static void TestInfo(MoneroDaemonInfo info)
    {
        Assert.That(info.Version, Is.Not.Null);
        Assert.That(info.NumAltBlocks, Is.GreaterThanOrEqualTo(0));
        Assert.That(info.BlockSizeLimit, Is.GreaterThan(0));
        Assert.That(info.BlockSizeMedian, Is.GreaterThan(0));
        TestUtils.TestUnsignedBigInteger(info.CumulativeDifficulty);
        TestUtils.TestUnsignedBigInteger(info.FreeSpace);
        Assert.That(info.NumOfflinePeers, Is.GreaterThanOrEqualTo(0));
        Assert.That(info.NumOnlinePeers, Is.GreaterThanOrEqualTo(0));
        Assert.That(info.Height, Is.GreaterThanOrEqualTo(0));
        Assert.That(info.NumIncomingConnections, Is.GreaterThanOrEqualTo(0));
        Assert.That(info.NumOutgoingConnections, Is.GreaterThanOrEqualTo(0));
        Assert.That(info.NumRpcConnections, Is.GreaterThanOrEqualTo(0));
        Assert.That(info.AdjustedTimestamp, Is.GreaterThan(0));
        Assert.That(info.Target, Is.GreaterThan(0));
        Assert.That(info.TargetHeight, Is.GreaterThanOrEqualTo(0));
        Assert.That(info.NumTxs, Is.GreaterThanOrEqualTo(0));
        Assert.That(info.NumTxsPool, Is.GreaterThanOrEqualTo(0));
        Assert.That(info.WasBootstrapEverUsed, Is.Not.Null);
        Assert.That(info.BlockWeightLimit, Is.GreaterThan(0));
        Assert.That(info.BlockWeightMedian, Is.GreaterThan(0));
        Assert.That(info.DatabaseSize, Is.GreaterThan(0));
        Assert.That(info.UpdateAvailable, Is.Not.Null);
        TestUtils.TestUnsignedBigInteger(info.Credits, false); // 0 credits
        string? topBlockHash = info.TopBlockHash;
        Assert.That(topBlockHash, Is.Not.Null);
        Assert.That(topBlockHash!.Length, Is.GreaterThan(0));

        if (info.IsRestricted)
        {
            return;
        }

        Assert.That(info.HeightWithoutBootstrap, Is.GreaterThan(0));
        Assert.That(info.StartTimestamp, Is.GreaterThan(0));
    }

    private static void TestSyncInfo(MoneroDaemonSyncInfo syncInfo)
    {
        // TODO: consistent naming, daemon in name?
        MoneroDaemonSyncInfo testObj = new();

        Assert.That(testObj.GetType().IsInstanceOfType(syncInfo), Is.True);
        Assert.That(syncInfo.Height, Is.GreaterThanOrEqualTo(0));
        if (syncInfo.Peers != null)
        {
            Assert.That(syncInfo.Peers!.Count, Is.GreaterThan(0));
            foreach (MoneroPeerInfo connection in syncInfo.Peers!)
            {
                TestPeer(connection.Info!);
            }
        }

        if (syncInfo.Spans != null)
        {
            // TODO: test that this is being hit, so far not used
            Assert.That(syncInfo.Spans!.Count, Is.GreaterThan(0));
            foreach (MoneroConnectionSpan span in syncInfo.Spans!)
            {
                TestConnectionSpan(span);
            }
        }

        Assert.That(syncInfo.NextNeededPruningSeed, Is.GreaterThanOrEqualTo(0));
        Assert.That(syncInfo.Overview, Is.Not.Null);
        TestUtils.TestUnsignedBigInteger(syncInfo.Credits, false); // 0 credits
        Assert.That(syncInfo.TopBlockHash, Is.Null);
    }

    private static void TestConnectionSpan(MoneroConnectionSpan? span)
    {
        Assert.That(span, Is.Not.Null);
        Assert.That(span!.ConnectionId, Is.Not.Null);
        Assert.That(span.ConnectionId!.Length, Is.GreaterThan(0));
        Assert.That(span.StartHeight, Is.GreaterThan(0));
        Assert.That(span.NumBlocks, Is.GreaterThan(0));
        Assert.That(span.RemoteAddress == null || span.RemoteAddress!.Length > 0, Is.True);
        Assert.That(span.Rate, Is.GreaterThan(0));
        Assert.That(span.Speed, Is.GreaterThanOrEqualTo(0));
        Assert.That(span.Size, Is.GreaterThan(0));
    }

    private static void TestHardForkInfo(MoneroHardForkInfo hardForkInfo)
    {
        Assert.That(hardForkInfo.EarliestHeight, Is.Not.Null);
        Assert.That(hardForkInfo.IsEnabled, Is.Not.Null);
        Assert.That(hardForkInfo.State, Is.Not.Null);
        Assert.That(hardForkInfo.Threshold, Is.Not.Null);
        Assert.That(hardForkInfo.Version, Is.Not.Null);
        Assert.That(hardForkInfo.NumVotes, Is.Not.Null);
        Assert.That(hardForkInfo.Voting, Is.Not.Null);
        Assert.That(hardForkInfo.Window, Is.Not.Null);
        TestUtils.TestUnsignedBigInteger(hardForkInfo.Credits, false); // 0 credits
        Assert.That(hardForkInfo.TopBlockHash, Is.Null);
    }

    private static void TestMoneroBan(MoneroBan ban)
    {
        Assert.That(ban.Host, Is.Not.Null);
        Assert.That(ban.Ip, Is.Not.Null);
        Assert.That(ban.Seconds, Is.Not.Null);
    }

    private static void TestAltChain(MoneroAltChain altChain)
    {
        Assert.That(altChain, Is.Not.Null);
        Assert.That(altChain.BlockHashes!.Count, Is.GreaterThan(0));
        TestUtils.TestUnsignedBigInteger(altChain.Difficulty, true);
        Assert.That(altChain.Height, Is.GreaterThan(0));
        Assert.That(altChain.Length, Is.GreaterThan(0));
        Assert.That(altChain.MainChainParentBlockHash!.Length, Is.EqualTo(64));
    }

    private static void TestPeer(MoneroPeer peer)
    {
        TestKnownPeer(peer, true);
        Assert.That(peer.Hash!.Length, Is.GreaterThan(0));
        Assert.That(peer.AvgDownload, Is.GreaterThanOrEqualTo(0));
        Assert.That(peer.AvgUpload, Is.GreaterThanOrEqualTo(0));
        Assert.That(peer.CurrentDownload, Is.GreaterThanOrEqualTo(0));
        Assert.That(peer.CurrentUpload, Is.GreaterThanOrEqualTo(0));
        Assert.That(peer.Height, Is.GreaterThanOrEqualTo(0));
        Assert.That(peer.LiveTime, Is.GreaterThanOrEqualTo(0));
        Assert.That(peer.NumReceives, Is.GreaterThanOrEqualTo(0));
        Assert.That(peer.ReceiveIdleTime, Is.GreaterThanOrEqualTo(0));
        Assert.That(peer.NumSends, Is.GreaterThanOrEqualTo(0));
        Assert.That(peer.SendIdleTime, Is.GreaterThanOrEqualTo(0));
        Assert.That(peer.State, Is.Not.Null);
        Assert.That(peer.NumSupportFlags, Is.GreaterThanOrEqualTo(0));
    }

    private static void TestKnownPeer(MoneroPeer peer, bool fromConnection)
    {
        Assert.That(peer, Is.Not.Null);
        Assert.That(peer.Port != null && int.Parse(peer.Port) > 0, Is.True);
        Assert.That(peer.RpcPort is null || peer.RpcPort >= 0, Is.True);
        if (fromConnection)
        {
            Assert.That(peer.LastSeenTimestamp, Is.Null);
        }
        else
        {
            if (peer.LastSeenTimestamp < 0)
            {
                MoneroUtils.Log(0, "Last seen timestamp is invalid: " + peer.LastSeenTimestamp);
            }

            Assert.That(peer.LastSeenTimestamp, Is.GreaterThanOrEqualTo(0));
        }

        Assert.That(peer.PruningSeed == null || peer.PruningSeed >= 0, Is.True);
    }

    private static void TestUpdateCheckResult(MoneroDaemonUpdateCheckResponse response)
    {
        Assert.That(response.IsUpdateAvailable, Is.Not.Null);
        if (response.IsUpdateAvailable == true)
        {
            Assert.That(response.AutoUri, Is.Not.Null.And.Not.Empty, "No auto uri; is daemon online?");
            Assert.That(response.UserUri, Is.Not.Null.And.Not.Empty);
            Assert.That(response.Version, Is.Not.Null.And.Not.Empty);
            Assert.That(response.Hash, Is.Not.Null.And.Not.Empty);
            Assert.That(response.Hash!.Length, Is.EqualTo(64));
        }
        else
        {
            Assert.That(response.AutoUri, Is.Null);
            Assert.That(response.UserUri, Is.Null);
            Assert.That(response.Version, Is.Null);
            Assert.That(response.Hash, Is.Null);
        }
    }

    private static void TestUpdateDownloadResult(MoneroDaemonUpdateDownloadResponse response, string? path)
    {
        TestUpdateCheckResult(response);
        if (response.IsUpdateAvailable == true)
        {
            if (path != null)
            {
                Assert.That(response.DownloadPath, Is.EqualTo(path));
            }
            else
            {
                Assert.That(response.DownloadPath, Is.Not.Null);
            }
        }
        else
        {
            Assert.That(response.DownloadPath, Is.Null);
        }
    }

    private static void TestOutputHistogramEntry(MoneroOutputHistogramEntry entry)
    {
        TestUtils.TestUnsignedBigInteger(entry.Amount);
        Assert.That(entry.NumInstances, Is.GreaterThanOrEqualTo(0));
        Assert.That(entry.NumUnlockedInstances, Is.GreaterThanOrEqualTo(0));
        Assert.That(entry.NumRecentInstances, Is.GreaterThanOrEqualTo(0));
    }

    private static void TestTxHexes(List<string> hexes, List<string> hexesPruned, List<string> txHashes)
    {
        Assert.That(hexes.Count, Is.EqualTo(txHashes.Count));
        Assert.That(hexesPruned.Count, Is.EqualTo(txHashes.Count));
        for (int i = 0; i < hexes.Count; i++)
        {
            Assert.That(hexes[i], Is.Not.Null);
            Assert.That(hexesPruned[i], Is.Not.Null);
            Assert.That(hexesPruned.Count, Is.GreaterThan(0));
            Assert.That(hexes[i].Length, Is.GreaterThan(hexesPruned[i].Length)); // pruned hex is shorter
        }
    }

    #endregion
}

public class TestContext
{
    public bool? DoNotTestCopy;
    public bool? FromBinaryBlock;
    public bool? FromGetTxPool;
    public bool? HasHex;
    public bool? HasJson;
    public bool? HasOutputIndices;
    public bool? HasTxs;
    public bool? HeaderIsFull;
    public bool? IsConfirmed;
    public bool? IsFull;
    public bool? IsMinerTx;
    public bool? IsPruned;
    public TestContext? TxContext;

    public TestContext() { }

    public TestContext(TestContext ctx)
    {
        HasJson = ctx.HasJson;
        IsPruned = ctx.IsPruned;
        IsFull = ctx.IsFull;
        IsConfirmed = ctx.IsConfirmed;
        IsMinerTx = ctx.IsMinerTx;
        FromGetTxPool = ctx.FromGetTxPool;
        FromBinaryBlock = ctx.FromBinaryBlock;
        HasOutputIndices = ctx.HasOutputIndices;
        DoNotTestCopy = ctx.DoNotTestCopy;
        HasTxs = ctx.HasTxs;
        HasHex = ctx.HasHex;
        HeaderIsFull = ctx.HeaderIsFull;
    }
}