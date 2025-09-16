using Microsoft.VisualBasic;

using Monero.Common;
using Monero.Daemon;
using Monero.Daemon.Common;
using Monero.IntegrationTests.Utils;
using Monero.Wallet;
using Monero.Wallet.Common;

namespace Monero.IntegrationTests;

public class MoneroDaemonRpcFixture : IDisposable
{
    public readonly TestContext BINARY_BLOCK_CTX = new();

    public MoneroDaemonRpc Daemon;
    public bool IsRestricted;
    public MoneroWalletRpc? Wallet;

    public MoneroDaemonRpcFixture()
    {
        Daemon = TestUtils.GetDaemonRpc();
        //Wallet = TestUtils.GetWalletRpc();
        IsRestricted = Daemon.IsRestricted();

        BINARY_BLOCK_CTX.hasHex = false;
        BINARY_BLOCK_CTX.headerIsFull = false;
        BINARY_BLOCK_CTX.hasTxs = true;
        BINARY_BLOCK_CTX.txContext = new TestContext();
        BINARY_BLOCK_CTX.txContext.isPruned = false;
        BINARY_BLOCK_CTX.txContext.isConfirmed = true;
        BINARY_BLOCK_CTX.txContext.fromGetTxPool = false;
        BINARY_BLOCK_CTX.txContext.hasOutputIndices = false;
        BINARY_BLOCK_CTX.txContext.fromBinaryBlock = true;

        TestUtils.WALLET_TX_TRACKER.Reset(); // all wallets need to wait for txs to confirm to reliably sync

        // Wait for some blocks to mine
        GenUtils.WaitFor(10000);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

public class TestMoneroDaemonRpc : IClassFixture<MoneroDaemonRpcFixture>
{
    private static MoneroDaemonRpc? daemon;
    private static MoneroWalletRpc? wallet;

    private static readonly bool LITE_MODE = false;
    private static readonly bool TEST_NON_RELAYS = true;
    private static readonly bool TEST_RELAYS = true; // creates and relays outgoing txs
    private static readonly bool TEST_NOTIFICATIONS = true;
    private static bool RESTRICTED_RPC;

    private static TestContext BINARY_BLOCK_CTX = new();

    private readonly MoneroDaemonRpcFixture fixture;

    public TestMoneroDaemonRpc(MoneroDaemonRpcFixture fixture)
    {
        BINARY_BLOCK_CTX = fixture.BINARY_BLOCK_CTX;

        daemon = fixture.Daemon;
        wallet = fixture.Wallet;

        RESTRICTED_RPC = fixture.IsRestricted;

        this.fixture = fixture;
        MoneroRpcConnection? rpcConnection = daemon.GetRpcConnection();

        Assert.True(rpcConnection.IsConnected(), "Daemon offline");
        ulong daemonHeight = daemon.GetHeight();
        if (daemonHeight == 1)
        {
            MineBlocks();
        }
    }

    #region Notification Tests

    // Can notify listeners when a new block is added to the chain
    [Fact]
    public void TestBlockListener()
    {
        Assert.True(!LITE_MODE && TEST_NOTIFICATIONS);

        try
        {
            // start mining if possible to help push the network along
            // TODO use wallet rpc address
            string address = TestUtils.ADDRESS;
            try { daemon.StartMining(address, 1, false, true); }
            catch (MoneroError e) { }

            // register a listener
            MoneroDaemonListener listener = new();
            daemon.AddListener(listener);

            // wait for next block notification
            MoneroBlockHeader header = daemon.WaitForNextBlockHeader();
            daemon.RemoveListener(listener); // unregister listener so daemon does not keep polling
            TestBlockHeader(header, true);

            // test that listener was called with equivalent header
            Assert.True(header.Equals(listener.GetLastBlockHeader()));
        }
        finally
        {
            // stop mining
            try { daemon.StopMining(); }
            catch (MoneroError e) { }
        }
    }

    #endregion

    #region Non Relays Tests

    [Fact]
    public void TestGetVersion()
    {
        Assert.True(TEST_NON_RELAYS);
        MoneroVersion version = daemon.GetVersion();
        Assert.NotNull(version.GetNumber());
        Assert.True(version.GetNumber() > 0);
        Assert.NotNull(version.IsRelease());
    }

    [Fact]
    public void TestIsTrusted()
    {
        Assert.True(TEST_NON_RELAYS);
        daemon.IsTrusted();
    }

    // Can get the blockchain height
    [Fact]
    public void TestGetHeight()
    {
        Assert.True(TEST_NON_RELAYS);
        ulong height = daemon.GetHeight();
        Assert.True(height > 0, "Height must be greater than 0");
    }

    // Can get a block hash by height
    [Fact]
    public void TestGetBlockIdByHeight()
    {
        Assert.True(TEST_NON_RELAYS);
        MoneroBlockHeader lastHeader = daemon.GetLastBlockHeader();
        string hash = daemon.GetBlockHash((ulong)lastHeader.GetHeight());
        Assert.NotNull(hash);
        Assert.True(64 == hash.Length);
    }

    // Can get a block template
    [Fact]
    public void TestGetBlockTemplate()
    {
        Assert.True(TEST_NON_RELAYS);
        MoneroBlockTemplate template = daemon.GetBlockTemplate(TestUtils.ADDRESS, 2);
        TestBlockTemplate(template);
    }

    // Can get the last block's header
    [Fact]
    public void TestGetLastBlockHeader()
    {
        Assert.True(TEST_NON_RELAYS);
        MoneroBlockHeader lastHeader = daemon.GetLastBlockHeader();
        TestBlockHeader(lastHeader, true);
    }

    // Can get a block header by hash
    [Fact]
    public void TestGetBlockHeaderByHash()
    {
        Assert.True(TEST_NON_RELAYS);

        // retrieve by hash of last block
        MoneroBlockHeader lastHeader = daemon.GetLastBlockHeader();
        string hash = daemon.GetBlockHash((ulong)lastHeader.GetHeight());
        MoneroBlockHeader header = daemon.GetBlockHeaderByHash(hash);
        TestBlockHeader(header, true);
        Assert.True(lastHeader.Equals(header));

        // retrieve by hash of previous to last block
        hash = daemon.GetBlockHash((ulong)lastHeader.GetHeight() - 1);
        header = daemon.GetBlockHeaderByHash(hash);
        TestBlockHeader(header, true);
        Assert.True(lastHeader.GetHeight() - 1 == (ulong)header.GetHeight());
    }

    // Can get a block header by height
    [Fact]
    public void TestGetBlockHeaderByHeight()
    {
        Assert.True(TEST_NON_RELAYS);

        // retrieve by height of last block
        MoneroBlockHeader lastHeader = daemon.GetLastBlockHeader();
        MoneroBlockHeader header = daemon.GetBlockHeaderByHeight((ulong)lastHeader.GetHeight());
        TestBlockHeader(header, true);
        Assert.True(lastHeader.Equals(header));

        // retrieve by height of previous to last block
        header = daemon.GetBlockHeaderByHeight((ulong)lastHeader.GetHeight() - 1);
        TestBlockHeader(header, true);
        Assert.True(lastHeader.GetHeight() - 1 == (ulong)header.GetHeight());
    }

    // Can get block headers by range
    // TODO: test start with no end, vice versa, inclusivity
    [Fact]
    public void TestGetBlockHeadersByRange()
    {
        Assert.True(TEST_NON_RELAYS);

        // determine start and end height based on number of blocks and how many blocks ago
        ulong numBlocks = 10;
        ulong numBlocksAgo = 10;
        ulong currentHeight = daemon.GetHeight();
        ulong startHeight = currentHeight - numBlocksAgo;
        ulong endHeight = currentHeight - (numBlocksAgo - numBlocks) - 1;

        // fetch headers
        List<MoneroBlockHeader> headers =
            daemon.GetBlockHeadersByRange(startHeight, endHeight);

        // test headers
        Assert.True(numBlocks == (ulong)headers.Count);
        int j = 0;
        for (ulong i = 0; i < numBlocks; i++)
        {
            MoneroBlockHeader header = headers[j];
            Assert.True(startHeight + i == (ulong)header.GetHeight());
            TestBlockHeader(header, true);
            j++;
        }
    }

    // Can get a block by hash
    [Fact]
    public void TestGetBlockByHash()
    {
        Assert.True(TEST_NON_RELAYS);

        // test config
        TestContext ctx = new() { hasHex = true, hasTxs = false, headerIsFull = true };

        // retrieve by hash of last block
        MoneroBlockHeader lastHeader = daemon.GetLastBlockHeader();
        string hash = daemon.GetBlockHash((ulong)lastHeader.GetHeight());
        MoneroBlock block = daemon.GetBlockByHash(hash);
        TestBlock(block, ctx);
        Assert.True(daemon.GetBlockByHeight((ulong)block.GetHeight()).Equals(block));
        Assert.Null(block.GetTxs());

        // retrieve by hash of previous to last block
        hash = daemon.GetBlockHash((ulong)lastHeader.GetHeight() - 1);
        block = daemon.GetBlockByHash(hash);
        TestBlock(block, ctx);
        Assert.True(daemon.GetBlockByHeight((ulong)lastHeader.GetHeight() - 1).Equals(block));
        Assert.Null(block.GetTxs());
    }

    // Can get blocks by hash which includes transactions (binary)
    [Fact(Skip = "Binary request not implemented")]
    public void TestGetBlocksByHashBinary()
    {
        Assert.True(TEST_NON_RELAYS);
        throw new MoneroError("Not implemented");
    }

    // Can get a block by height
    [Fact]
    public void TestGetBlockByHeight()
    {
        Assert.True(TEST_NON_RELAYS);

        // config for testing blocks
        TestContext ctx = new();
        ctx.hasHex = true;
        ctx.headerIsFull = true;
        ctx.hasTxs = false;

        // retrieve by height of last block
        MoneroBlockHeader lastHeader = daemon.GetLastBlockHeader();
        MoneroBlock block = daemon.GetBlockByHeight((ulong)lastHeader.GetHeight());
        TestBlock(block, ctx);
        Assert.True(daemon.GetBlockByHeight((ulong)block.GetHeight()).Equals(block));

        // retrieve by height of previous to last block
        block = daemon.GetBlockByHeight((ulong)lastHeader.GetHeight() - 1);
        TestBlock(block, ctx);
        Assert.True(lastHeader.GetHeight() - 1 == (ulong)block.GetHeight());
    }

    // Can get blocks by height which includes transactions (binary)
    [Fact(Skip = "Binary request not implemented")]
    public void TestGetBlocksByHeightBinary()
    {
        Assert.True(TEST_NON_RELAYS);

        // set number of blocks to test
        int numBlocks = 100;

        // select random heights  // TODO: this is horribly inefficient way of computing last 100 blocks if not shuffling
        ulong currentHeight = daemon.GetHeight();
        List<ulong> allHeights = [];
        for (ulong i = 0; i < currentHeight - 1; i++)
        {
            allHeights.Add(i);
        }

        //GenUtils.shuffle(allHeights);
        List<ulong> heights = [];
        for (int i = allHeights.Count - numBlocks; i < allHeights.Count; i++)
        {
            heights.Add(allHeights[i]);
        }

        // fetch blocks
        List<MoneroBlock> blocks = daemon.GetBlocksByHeight(heights);

        // test blocks
        bool txFound = false;
        Assert.True(numBlocks == blocks.Count);
        for (int i = 0; i < heights.Count; i++)
        {
            MoneroBlock block = blocks[i];
            if (block.GetTxs().Count > 0)
            {
                txFound = true;
            }

            TestBlock(block, BINARY_BLOCK_CTX);
            Assert.True(block.GetHeight() == heights[i]);
        }

        Assert.True(txFound, "No transactions found to test");
    }

    // Can get blocks by range in a single request
    [Fact(Skip = "Binary request not implemented")]
    public void TestGetBlocksByRange()
    {
        Assert.True(TEST_NON_RELAYS);

        // get height range
        ulong numBlocks = 100;
        ulong numBlocksAgo = 190;
        Assert.True(numBlocks > 0);
        Assert.True(numBlocksAgo >= numBlocks);
        ulong height = daemon.GetHeight();
        Assert.True(height - numBlocksAgo + numBlocks - 1 < height);
        ulong startHeight = height - numBlocksAgo;
        ulong endHeight = height - numBlocksAgo + numBlocks - 1;

        // test known start and end heights
        TestGetBlocksRange(startHeight, endHeight, height, false);

        // test unspecified start
        TestGetBlocksRange(null, numBlocks - 1, height, false);

        // test unspecified end
        TestGetBlocksRange(height - numBlocks - 1, null, height, false);
    }

    // Can get blocks by range using chunked requests
    [Fact(Skip = "Binary request not implemented")]
    public void TestGetBlocksByRangeChunked()
    {
        Assert.True(TEST_NON_RELAYS && !LITE_MODE);

        // get ulong height range
        ulong numBlocks = Math.Min(daemon.GetHeight() - 2, 1440); // test up to ~2 days of blocks
        Assert.True(numBlocks > 0);
        ulong height = daemon.GetHeight();
        Assert.True(height - numBlocks - 1 < height);
        ulong startHeight = height - numBlocks;
        ulong endHeight = height - 1;

        // test known start and end heights
        TestGetBlocksRange(startHeight, endHeight, height, true);

        // test unspecified start
        TestGetBlocksRange(null, numBlocks - 1, height, true);

        // test unspecified end
        TestGetBlocksRange(endHeight - numBlocks - 1, null, height, true);
    }

    // Can get block hashes (binary)
    [Fact(Skip = "Binary request not implemented")]
    public void TestGetBlockIdsBinary()
    {
        Assert.True(TEST_NON_RELAYS);
        //get_hashes.bin
        throw new MoneroError("Not implemented");
    }

    // Can get a transaction by hash with and without pruning
    [Fact]
    public void TestGetTxByHash()
    {
        Assert.True(TEST_NON_RELAYS);

        // fetch transaction hashes to test
        List<string> txHashes = GetConfirmedTxHashes(daemon);

        // context for testing txs
        TestContext ctx = new() { isPruned = false, isConfirmed = true, fromGetTxPool = false };

        // fetch each tx by hash without pruning
        foreach (string txHash in txHashes)
        {
            MoneroTx? tx = daemon.GetTx(txHash);
            TestTx(tx, ctx);
        }

        // fetch each tx by hash with pruning
        foreach (string txHash in txHashes)
        {
            MoneroTx? tx = daemon.GetTx(txHash, true);
            ctx.isPruned = true;
            TestTx(tx, ctx);
        }

        // fetch invalid hash
        try
        {
            daemon.GetTx("invalid tx hash");
            throw new MoneroError("fail");
        }
        catch (MoneroError e)
        {
            Assert.True("Invalid transaction hash" == e.Message);
        }
    }

    // Can get transactions by hashes with and without pruning
    [Fact(Skip = "Needs monero-wallet-rpc")]
    public void TestGetTxsByHashes()
    {
        Assert.True(TEST_NON_RELAYS);

        // fetch transaction hashes to test
        List<string> txHashes = GetConfirmedTxHashes(daemon);
        Assert.True(txHashes.Count > 0);

        // context for testing txs
        TestContext ctx = new() { isPruned = false, isConfirmed = true, fromGetTxPool = false };

        // fetch txs by hash without pruning
        List<MoneroTx> txs = daemon.GetTxs(txHashes);
        Assert.True(txHashes.Count == txs.Count);
        foreach (MoneroTx _tx in txs)
        {
            TestTx(_tx, ctx);
        }

        // fetch txs by hash with pruning
        txs = daemon.GetTxs(txHashes, true);
        ctx.isPruned = true;
        Assert.True(txHashes.Count == txs.Count);
        foreach (MoneroTx _tx in txs)
        {
            TestTx(_tx, ctx);
        }

        // fetch missing hash
        MoneroTx tx = wallet.CreateTx(new MoneroTxConfig().SetAccountIndex(0)
            .AddDestination(wallet.GetPrimaryAddress(), TestUtils.MAX_FEE));
        Assert.True(daemon.GetTx(tx.GetHash()) != null);
        txHashes.Add(tx.GetHash());
        int numTxs = txs.Count;
        txs = daemon.GetTxs(txHashes);
        Assert.True(numTxs == txs.Count);

        // fetch invalid hash
        txHashes.Add("invalid tx hash");
        try
        {
            daemon.GetTxs(txHashes);
            throw new MoneroError("fail");
        }
        catch (MoneroError e)
        {
            Assert.True("Invalid transaction hash" == e.Message);
        }
    }

    // Can get transactions by hashes that are in the transaction pool
    [Fact(Skip = "Needs monero-wallet-rpc")]
    public void TestGetTxsByHashesInPool()
    {
        Assert.True(TEST_NON_RELAYS);
        TestUtils.WALLET_TX_TRACKER
            .WaitForWalletTxsToClearPool(wallet); // wait for wallet's txs in the pool to clear to ensure reliable sync

        // submit txs to the pool but don't relay
        List<string> txHashes = [];
        for (uint i = 1; i < 3; i++)
        {
            MoneroTx tx = GetUnrelayedTx(wallet, i);
            MoneroSubmitTxResult result = daemon.SubmitTxHex(tx.GetFullHex(), true);
            TestSubmitTxResultGood(result);
            Assert.False(result.IsRelayed());
            txHashes.Add(tx.GetHash());
        }

        // fetch txs by hash
        MoneroUtils.Log(0, "Fetching txs...");
        List<MoneroTx> txs = daemon.GetTxs(txHashes);
        MoneroUtils.Log(0, "done");

        // context for testing tx
        TestContext ctx = new() { isPruned = false, isConfirmed = false, fromGetTxPool = false };

        // test fetched txs
        Assert.True(txHashes.Count == txs.Count);
        foreach (MoneroTx tx in txs)
        {
            TestTx(tx, ctx);
        }

        // clear txs from pool
        daemon.FlushTxPool(txHashes);
        wallet.Sync();
    }

    // Can get a transaction hex by hash with and without pruning
    [Fact]
    public void TestGetTxHexByHash()
    {
        Assert.True(TEST_NON_RELAYS);

        // fetch transaction hashes to test
        List<string> txHashes = GetConfirmedTxHashes(daemon);

        // fetch each tx hex by hash with and without pruning
        List<string> hexes = [];
        List<string> hexesPruned = [];
        foreach (string txHash in txHashes)
        {
            hexes.Add(daemon.GetTxHex(txHash));
            hexesPruned.Add(daemon.GetTxHex(txHash, true));
        }

        // test results
        Assert.True(hexes.Count == txHashes.Count);
        Assert.True(hexesPruned.Count == txHashes.Count);
        for (int i = 0; i < hexes.Count; i++)
        {
            Assert.NotNull(hexes[i]);
            Assert.NotNull(hexesPruned[i]);
            Assert.True(hexesPruned.Count > 0);
            Assert.True(hexes[i].Length > hexesPruned[i].Length); // pruned hex is shorter
        }

        // fetch invalid hash
        try
        {
            daemon.GetTxHex("invalid tx hash");
            throw new MoneroError("fail");
        }
        catch (MoneroError e)
        {
            Assert.True("Invalid transaction hash" == e.Message);
        }
    }

    // Can get transaction hexes by hashes with and without pruning
    [Fact(Skip = "Needs monero-wallet-rpc")]
    public void TestGetTxHexesByHashes()
    {
        Assert.True(TEST_NON_RELAYS);

        // fetch transaction hashes to test
        List<string> txHashes = GetConfirmedTxHashes(daemon);

        // fetch tx hexes by hash with and without pruning
        List<string> hexes = daemon.GetTxHexes(txHashes);
        List<string> hexesPruned = daemon.GetTxHexes(txHashes, true);

        // test results
        Assert.True(hexes.Count == txHashes.Count);
        Assert.True(hexesPruned.Count == txHashes.Count);
        for (int i = 0; i < hexes.Count; i++)
        {
            Assert.NotNull(hexes[i]);
            Assert.NotNull(hexesPruned[i]);
            Assert.True(hexesPruned.Count > 0);
            Assert.True(hexes[i].Length > hexesPruned[i].Length); // pruned hex is shorter
        }

        // fetch invalid hash
        txHashes.Add("invalid tx hash");
        try
        {
            daemon.GetTxHexes(txHashes);
            throw new MoneroError("fail");
        }
        catch (MoneroError e)
        {
            Assert.True("Invalid transaction hash" == e.Message);
        }
    }

    // Can get the miner transaction sum
    [Fact(Skip = "Not supported by regtest daemon")]
    public void TestGetMinerTxSum()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        MoneroMinerTxSum sum = daemon.GetMinerTxSum(0, Math.Min(50000, daemon.GetHeight()));
        TestMinerTxSum(sum);
    }

    // Can get a fee estimate
    [Fact]
    public void TestGetFeeEstimate()
    {
        Assert.True(TEST_NON_RELAYS);
        MoneroFeeEstimate feeEstimate = daemon.GetFeeEstimate();
        TestUtils.TestUnsignedBigInteger(feeEstimate.GetFee(), true);
        Assert.True(feeEstimate.GetFees().Count == 4); // slow, normal, fast, fastest
        for (int i = 0; i < 4; i++)
        {
            TestUtils.TestUnsignedBigInteger(feeEstimate.GetFees()[i], true);
        }

        TestUtils.TestUnsignedBigInteger(feeEstimate.GetQuantizationMask(), true);
    }

    // Can get all transactions in the transaction pool
    [Fact(Skip = "Needs monero-wallet-rpc")]
    public void TestGetTxsInPool()
    {
        Assert.True(TEST_NON_RELAYS);
        TestUtils.WALLET_TX_TRACKER.WaitForWalletTxsToClearPool(wallet);

        // submit tx to pool but don't relay
        MoneroTx tx = GetUnrelayedTx(wallet, 1);
        MoneroSubmitTxResult result = daemon.SubmitTxHex(tx.GetFullHex(), true);
        TestSubmitTxResultGood(result);
        Assert.True(result.IsRelayed() == false);

        // fetch txs in pool
        List<MoneroTx> txs = daemon.GetTxPool();

        // context for testing tx
        TestContext ctx = new() { isPruned = false, isConfirmed = false, fromGetTxPool = true };

        // test txs
        Assert.True(txs.Count > 0, "Test requires an unconfirmed tx in the tx pool");
        foreach (MoneroTx aTx in txs)
        {
            TestTx(aTx, ctx);
        }

        // flush the tx from the pool, gg
        daemon.FlushTxPool(tx.GetHash());
        wallet.Sync();
    }

    // Can get hashes of transactions in the transaction pool (binary)
    [Fact(Skip = "Binary request not implemented")]
    public void TestGetIdsOfTxsInPoolBin()
    {
        Assert.True(TEST_NON_RELAYS);
        // TODO: get_transaction_pool_hashes.bin
        throw new MoneroError("Not implemented");
    }

    // Can get the transaction pool backlog (binary)
    [Fact(Skip = "Binary request not implemented")]
    public void TestGetTxPoolBacklogBin()
    {
        Assert.True(TEST_NON_RELAYS);
        // TODO: get_txpool_backlog
        throw new MoneroError("Not implemented");
    }

    // Can get transaction pool statistics
    [Fact(Skip = "Needs monero-wallet-rpc")]
    public void TestGetTxPoolStatistics()
    {
        Assert.True(TEST_NON_RELAYS);
        TestUtils.WALLET_TX_TRACKER.WaitForWalletTxsToClearPool(wallet);
        Exception? err = null;
        Collection txIds = [];
        try
        {
            // submit txs to the pool but don't relay
            for (uint i = 1; i < 3; i++)
            {
                // submit tx hex
                MoneroTx tx = GetUnrelayedTx(wallet, i);
                MoneroSubmitTxResult result = daemon.SubmitTxHex(tx.GetFullHex(), true);
                Assert.True(result.IsGood());
                txIds.Add(tx.GetHash());

                // get tx pool stats
                MoneroTxPoolStats stats = daemon.GetTxPoolStats();
                Assert.True(stats.GetNumTxs() > i - 1);
                TestTxPoolStats(stats);
            }
        }
        catch (Exception e)
        {
            err = e;
        }

        // flush txs
        //daemon.FlushTxPool(txIds.ToList());
        if (err != null)
        {
            throw new MoneroError(err.Message);
        }
    }

    // Can flush all transactions from the pool
    [Fact(Skip = "Needs monero-wallet-rpc")]
    public void TestFlushTxsFromPool()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        TestUtils.WALLET_TX_TRACKER.WaitForWalletTxsToClearPool(wallet);

        // preserve original transactions in the pool
        List<MoneroTx> txPoolBefore = daemon.GetTxPool();

        // submit txs to the pool but don't relay
        for (uint i = 1; i < 3; i++)
        {
            MoneroTx tx = GetUnrelayedTx(wallet, i);
            MoneroSubmitTxResult result = daemon.SubmitTxHex(tx.GetFullHex(), true);
            TestSubmitTxResultGood(result);
        }

        Assert.True(txPoolBefore.Count + 2 == daemon.GetTxPool().Count);

        // flush tx pool
        daemon.FlushTxPool();
        Assert.True(0 == daemon.GetTxPool().Count);

        // re-submit original transactions
        foreach (MoneroTx tx in txPoolBefore)
        {
            MoneroSubmitTxResult result = daemon.SubmitTxHex(tx.GetFullHex(), tx.IsRelayed() == true);
            TestSubmitTxResultGood(result);
        }

        // pool is back to original state
        Assert.True(txPoolBefore.Count == daemon.GetTxPool().Count);

        // sync wallet for next test
        wallet.Sync();
    }

    // Can flush a transaction from the pool by hash
    [Fact(Skip = "Needs monero-wallet-rpc")]
    public void TestFlushTxFromPoolByHash()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        TestUtils.WALLET_TX_TRACKER.WaitForWalletTxsToClearPool(wallet);

        // preserve original transactions in the pool
        List<MoneroTx> txPoolBefore = daemon.GetTxPool();

        // submit txs to the pool but don't relay
        List<MoneroTx> txs = [];
        for (uint i = 1; i < 3; i++)
        {
            MoneroTx tx = GetUnrelayedTx(wallet, i);
            MoneroSubmitTxResult result = daemon.SubmitTxHex(tx.GetFullHex(), true);
            TestSubmitTxResultGood(result);
            txs.Add(tx);
        }

        // remove each tx from the pool by hash and test
        for (int i = 0; i < txs.Count; i++)
        {
            // flush tx from pool
            daemon.FlushTxPool(txs[i].GetHash());

            // test tx pool
            List<MoneroTx> poolTxs = daemon.GetTxPool();
            Assert.True(txs.Count - i - 1 == poolTxs.Count);
        }

        // pool is back to original state
        Assert.True(txPoolBefore.Count == daemon.GetTxPool().Count);

        // sync wallet for next test
        wallet.Sync();
    }

    // Can flush transactions from the pool by hashes
    [Fact(Skip = "Needs monero wallet rpc")]
    public void TestFlushTxsFromPoolByHashes()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        TestUtils.WALLET_TX_TRACKER.WaitForWalletTxsToClearPool(wallet);

        // preserve original transactions in the pool
        List<MoneroTx> txPoolBefore = daemon.GetTxPool();

        // submit txs to the pool but don't relay
        List<string> txHashes = [];
        for (uint i = 1; i < 3; i++)
        {
            MoneroTx tx = GetUnrelayedTx(wallet, i);
            MoneroSubmitTxResult result = daemon.SubmitTxHex(tx.GetFullHex(), true);
            TestSubmitTxResultGood(result);
            txHashes.Add(tx.GetHash());
        }

        Assert.True(txPoolBefore.Count + txHashes.Count == daemon.GetTxPool().Count);

        // remove all txs by hashes
        daemon.FlushTxPool(txHashes);

        // pool is back to original state
        Assert.True(txPoolBefore.Count == daemon.GetTxPool().Count);
        wallet.Sync();
    }

    // Can get the spent status of key images
    [Fact(Skip = "Needs monero-wallet-rpc")]
    public void TestGetSpentStatusOfKeyImages()
    {
        Assert.True(TEST_NON_RELAYS);
        TestUtils.WALLET_TX_TRACKER.WaitForWalletTxsToClearPool(wallet);

        // submit txs to the pool to collect key images then flush them
        List<MoneroTx> txs = [];
        for (uint i = 1; i < 3; i++)
        {
            MoneroTx tx = GetUnrelayedTx(wallet, i);
            daemon.SubmitTxHex(tx.GetFullHex(), true);
            txs.Add(tx);
        }

        List<string> keyImages = [];
        List<string> txHashes = [];
        foreach (MoneroTx tx in txs)
        {
            txHashes.Add(tx.GetHash());
        }

        foreach (MoneroTx tx in daemon.GetTxs(txHashes))
        {
            foreach (MoneroOutput input in tx.GetInputs())
            {
                keyImages.Add(input.GetKeyImage().GetHex());
            }
        }

        daemon.FlushTxPool(txHashes);

        // key images are not spent
        TestSpentStatuses(keyImages, MoneroKeyImage.SpentStatus.NotSpent);

        // submit txs to the pool but don't relay
        foreach (MoneroTx tx in txs)
        {
            daemon.SubmitTxHex(tx.GetFullHex(), true);
        }

        // key images are in the tx pool
        TestSpentStatuses(keyImages, MoneroKeyImage.SpentStatus.TxPool);

        // collect key images of confirmed txs
        keyImages = [];
        txs = GetConfirmedTxs(daemon, 10);
        foreach (MoneroTx tx in txs)
        {
            foreach (MoneroOutput input in tx.GetInputs())
            {
                keyImages.Add(input.GetKeyImage().GetHex());
            }
        }

        // key images are all spent
        TestSpentStatuses(keyImages, MoneroKeyImage.SpentStatus.Confirmed);

        // flush this test's txs from pool
        daemon.FlushTxPool(txHashes);
    }

    // Can get output indices given a list of transaction hashes (binary)
    [Fact(Skip = "Binary request not implemented")]
    public void TestGetOutputIndicesFromTxIdsBinary()
    {
        Assert.True(TEST_NON_RELAYS);
        throw new Exception("Not implemented"); // get_o_indexes.bin
    }

    // Can get outputs given a list of output amounts and indices (binary)
    [Fact(Skip = "Binary request not implemented")]
    public void TestGetOutputsFromAmountsAndIndicesBinary()
    {
        Assert.True(TEST_NON_RELAYS);
        throw new Exception("Not implemented"); // get_outs.bin
    }

    // Can get an output histogram (binary)
    [Fact(Skip = "Binary request not implemented")]
    public void TestGetOutputHistogramBinary()
    {
        Assert.True(TEST_NON_RELAYS);
        List<MoneroOutputHistogramEntry> entries =
            daemon.GetOutputHistogram();
        Assert.True(entries.Count > 0);
        foreach (MoneroOutputHistogramEntry entry in entries)
        {
            TestOutputHistogramEntry(entry);
        }
    }

    // Can get an output distribution (binary)
    [Fact(Skip = "Binary request not implemented")]
    public void TestGetOutputDistributionBinary()
    {
        Assert.True(TEST_NON_RELAYS);
        List<ulong> amounts = [];
        amounts.Add(0);
        amounts.Add(1);
        amounts.Add(10);
        amounts.Add(100);
        amounts.Add(1000);
        amounts.Add(10000);
        amounts.Add(100000);
        amounts.Add(1000000);
        List<MoneroOutputDistributionEntry> entries = daemon.GetOutputDistribution(amounts);
        foreach (MoneroOutputDistributionEntry entry in entries)
        {
            TestOutputDistributionEntry(entry);
        }
    }

    // Can get general information
    [Fact]
    public void TestGetGeneralInformation()
    {
        Assert.True(TEST_NON_RELAYS);
        MoneroDaemonInfo info = daemon.GetInfo();
        TestInfo(info);
    }

    // Can get sync information
    [Fact]
    public void TestGetSyncInformation()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        MoneroDaemonSyncInfo syncInfo = daemon.GetSyncInfo();
        TestSyncInfo(syncInfo);
    }

    // Can get hard fork information
    [Fact]
    public void TestGetHardForkInformation()
    {
        Assert.True(TEST_NON_RELAYS);
        MoneroHardForkInfo hardForkInfo = daemon.GetHardForkInfo();
        TestHardForkInfo(hardForkInfo);
    }

    // Can get alternative chains
    [Fact]
    public void TestGetAlternativeChains()
    {
        Assert.True(TEST_NON_RELAYS);
        //Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        List<MoneroAltChain> altChains = daemon.GetAltChains();
        foreach (MoneroAltChain altChain in altChains)
        {
            TestAltChain(altChain);
        }
    }

    // Can get alternative block hashes
    [Fact]
    public void TestGetAlternativeBlockIds()
    {
        Assert.True(TEST_NON_RELAYS);
        List<string> altBlockIds = daemon.GetAltBlockHashes();
        foreach (string altBlockId in altBlockIds)
        {
            Assert.NotNull(altBlockId);
            Assert.True(64 == altBlockId.Length); // TODO: common validation
        }
    }

    // Can get, set, and reset a download bandwidth limit
    [Fact]
    public void TestSetDownloadBandwidth()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        int initVal = daemon.GetDownloadLimit();
        Assert.True(initVal > 0);
        int setVal = initVal * 2;
        daemon.SetDownloadLimit(setVal);
        Assert.True(setVal == daemon.GetDownloadLimit());
        int resetVal = daemon.ResetDownloadLimit();
        Assert.True(initVal == resetVal);

        // test invalid limits
        try
        {
            daemon.SetDownloadLimit(0);
            throw new MoneroError("Should have thrown error on invalid input");
        }
        catch (MoneroError e)
        {
            Assert.True("Download limit must be an integer greater than 0" == e.Message);
        }

        Assert.True(daemon.GetDownloadLimit() == initVal);
    }

    // Can get, set, and reset an upload bandwidth limit
    [Fact]
    public void TestSetUploadBandwidth()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        int initVal = daemon.GetUploadLimit();
        Assert.True(initVal > 0);
        int setVal = initVal * 2;
        daemon.SetUploadLimit(setVal);
        Assert.True(setVal == daemon.GetUploadLimit());
        int resetVal = daemon.ResetUploadLimit();
        Assert.True(initVal == resetVal);

        // test invalid limits
        try
        {
            daemon.SetUploadLimit(0);
            throw new Exception("Should have thrown error on invalid input");
        }
        catch (MoneroError e)
        {
            Assert.True("Upload limit must be an integer greater than 0" == e.Message);
        }

        Assert.True(initVal == daemon.GetUploadLimit());
    }

    // Can get peers with active incoming or outgoing connections
    [Fact]
    public void TestGetPeers()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        List<MoneroPeer> peers = daemon.GetPeers();
        Assert.True(peers.Count > 0, "Daemon has no incoming or outgoing peers to test");
        foreach (MoneroPeer peer in peers)
        {
            TestPeer(peer);
        }
    }

    // Can get all known peers which may be online or offline
    [Fact(Skip = "Daemon has no known peers to test")]
    public void TestGetKnownPeers()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        List<MoneroPeer> peers = daemon.GetKnownPeers();
        Assert.True(peers.Count > 0, "Daemon has no known peers to test");
        foreach (MoneroPeer peer in peers)
        {
            TestKnownPeer(peer, false);
        }
    }

    // Can limit the number of outgoing peers
    [Fact]
    public void TestSetOutgoingPeerLimit()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        daemon.SetOutgoingPeerLimit(0);
        daemon.SetOutgoingPeerLimit(8);
        daemon.SetOutgoingPeerLimit(10);
    }

    // Can limit the number of incoming peers
    [Fact]
    public void TestSetIncomingPeerLimit()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        daemon.SetIncomingPeerLimit(0);
        daemon.SetIncomingPeerLimit(8);
        daemon.SetIncomingPeerLimit(10);
    }

    // Can ban a peer
    [Fact]
    public void TestBanPeer()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");

        // set ban
        MoneroBan ban = new();
        ban.SetHost("192.168.1.51");
        ban.SetIsBanned(true);
        ban.SetSeconds(60);
        daemon.SetPeerBan(ban);

        // test ban
        List<MoneroBan> bans = daemon.GetPeerBans();
        bool found = false;
        foreach (MoneroBan aBan in bans)
        {
            TestMoneroBan(aBan);
            if ("192.168.1.51".Equals(aBan.GetHost()))
            {
                found = true;
            }
        }

        Assert.True(found);
    }

    // Can ban peers
    [Fact]
    public void TestBanPeers()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");

        // set bans
        MoneroBan ban1 = new();
        ban1.SetHost("192.168.1.52");
        ban1.SetIsBanned(true);
        ban1.SetSeconds(60);
        MoneroBan ban2 = new();
        ban2.SetHost("192.168.1.53");
        ban2.SetIsBanned(true);
        ban2.SetSeconds(60);
        List<MoneroBan> bans = [];
        bans.Add(ban1);
        bans.Add(ban2);
        daemon.SetPeerBans(bans);

        // test bans
        bans = daemon.GetPeerBans();
        bool found1 = false;
        bool found2 = false;
        foreach (MoneroBan aBan in bans)
        {
            TestMoneroBan(aBan);
            if ("192.168.1.52".Equals(aBan.GetHost()))
            {
                found1 = true;
            }

            if ("192.168.1.53".Equals(aBan.GetHost()))
            {
                found2 = true;
            }
        }

        Assert.True(found1);
        Assert.True(found2);
    }

    // Can start and stop mining
    [Fact(Skip = "Fails on github CI")]
    public void TestMining()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");

        // stop mining at beginning of test
        try { daemon.StopMining(); }
        catch (MoneroError e) { }

        // generate address to mine to
        // TODO use wallet rpc
        // string address = wallet.GetPrimaryAddress();
        string address = TestUtils.ADDRESS;
        // start mining
        daemon.StartMining(address, 1, false, true);

        GenUtils.WaitFor(30);

        // stop mining
        daemon.StopMining();
    }

    // Can get mining status
    // TODO why this test fails on github runner?
    [Fact(Skip = "Fails on github CI")]
    public void TestGetMiningStatus()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");

        try
        {
            // stop mining at beginning of test
            try { daemon.StopMining(); }
            catch (MoneroError e) { }

            // test status without mining
            MoneroMiningStatus status = daemon.GetMiningStatus();
            Assert.False(status.IsActive());
            Assert.Null(status.GetAddress());
            Assert.True(0 == (long)status.GetSpeed());
            Assert.True(0 == (int)status.GetNumThreads());
            Assert.Null(status.IsBackground());

            // test status with mining
            // TODO use wallet rpc address
            //string address = wallet.GetPrimaryAddress();
            string address = TestUtils.ADDRESS;
            ulong threadCount = 1;
            bool isBackground = false;
            daemon.StartMining(address, threadCount, isBackground, true);
            status = daemon.GetMiningStatus();
            Assert.True(status.IsActive());
            Assert.True(address == status.GetAddress());
            Assert.True(status.GetSpeed() >= 0);
            Assert.True(threadCount == status.GetNumThreads());
            Assert.True(isBackground == status.IsBackground());
        }
        finally
        {
            // stop mining at end of test
            try { daemon.StopMining(); }
            catch (MoneroError e)
            {
                // ignore
            }
        }
    }

    // Can submit a mined block to the network
    [Fact(Skip = "Not supported by regtest daemon")]
    public void TestSubmitMinedBlock()
    {
        Assert.True(TEST_NON_RELAYS);

        // get template to mine on
        MoneroBlockTemplate template = daemon.GetBlockTemplate(TestUtils.ADDRESS);

        // TODO monero rpc: way to get mining nonce when found in order to submit?

        // try to submit block hashing blob without nonce
        try
        {
            daemon.SubmitBlock(template.GetBlockTemplateBlob());
            throw new Exception("Should have thrown error");
        }
        catch (MoneroRpcError e)
        {
            Assert.True(-7 == e.GetCode());
            Assert.True("Block not accepted" == e.Message);
        }
    }

    // Can prune the blockchain
    [Fact(Skip = "Not supported by regtest daemon")]
    public void TestPruneBlockchain()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        MoneroPruneResult result = daemon.PruneBlockchain(true);
        if (result.IsPruned() == true)
        {
            Assert.True(result.GetPruningSeed() > 0);
        }
        else
        {
            Assert.True(0 == result.GetPruningSeed());
        }
    }

    // Can check for an update
    [Fact(Skip = "Unstable update call")]
    public void TestCheckForUpdate()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");
        MoneroDaemonUpdateCheckResult result = daemon.CheckForUpdate();
        TestUpdateCheckResult(result);
    }

    // Can download an update
    [Fact(Skip = "Non supported by regtest daemon")]
    public void TestDownloadUpdate()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");

        // download to default path
        MoneroDaemonUpdateDownloadResult result = daemon.DownloadUpdate();
        TestUpdateDownloadResult(result, null);

        // download to defined path
        string path = "test_download_" + DateTime.Now + ".tar.bz2";
        result = daemon.DownloadUpdate(path);
        TestUpdateDownloadResult(result, path);

        // test invalid path
        if (result.IsUpdateAvailable() == true)
        {
            try
            {
                daemon.DownloadUpdate("./ohhai/there");
                throw new Exception("Should have thrown error");
            }
            catch (MoneroRpcError e)
            {
                Assert.True(e.Message != "Should have thrown error");
                Assert.True(500 == e.GetCode()); // TODO monerod: this causes a 500 in daemon rpc
            }
        }
    }

    // Can be stopped
    [Fact(Skip = "Disabled")]
    public void TestStop()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.False(RESTRICTED_RPC, "Daemon RPC is restricted");

        // stop the daemon
        daemon.Stop();

        // give the daemon time to shut down
        GenUtils.WaitFor(TestUtils.SYNC_PERIOD_IN_MS);
        // try to interact with the daemon
        try
        {
            daemon.GetHeight();
            throw new Exception("Should have thrown error");
        }
        catch (MoneroError e)
        {
            Assert.True("Should have thrown error" != e.Message);
        }
    }

    #endregion

    #region Relay Tests

    // Can submit a tx in hex format to the pool and relay in one call
    [Fact(Skip = "Needs monero-wallet-rpc")]
    public void TestSubmitAndRelayTxHex()
    {
        Assert.True(TEST_RELAYS && !LITE_MODE);

        // wait one time for wallet txs in the pool to clear
        // TODO monero-project: update from pool does not prevent creating double spend tx
        TestUtils.WALLET_TX_TRACKER.WaitForWalletTxsToClearPool(wallet);

        // create 2 txs, the second will double spend outputs of first
        MoneroTx
            tx1 = GetUnrelayedTx(wallet,
                2); // TODO: this test requires tx to be from/to different accounts else the occlusion issue (#4500) causes the tx to not be recognized by the wallet at all
        MoneroTx tx2 = GetUnrelayedTx(wallet, 2);

        // submit and relay tx1
        MoneroSubmitTxResult result = daemon.SubmitTxHex(tx1.GetFullHex());
        Assert.True(result.IsRelayed());
        TestSubmitTxResultGood(result);

        // tx1 is in the pool
        List<MoneroTx> txs = daemon.GetTxPool();
        bool found = false;
        foreach (MoneroTx aTx in txs)
        {
            if (aTx.GetHash().Equals(tx1.GetHash()))
            {
                Assert.True(aTx.IsRelayed());
                found = true;
                break;
            }
        }

        Assert.True(found, "Tx1 was not found after being submitted to the daemon's tx pool");

        // tx1 is recognized by the wallet
        wallet.Sync();
        wallet.GetTx(tx1.GetHash());

        // submit and relay tx2 hex which double spends tx1
        result = daemon.SubmitTxHex(tx2.GetFullHex());
        Assert.True(result.IsRelayed());
        TestSubmitTxResultDoubleSpend(result);

        // tx2 is in not the pool
        txs = daemon.GetTxPool();
        found = false;
        foreach (MoneroTx aTx in txs)
        {
            if (aTx.GetHash().Equals(tx2.GetHash()))
            {
                found = true;
                break;
            }
        }

        Assert.True(!found, "Tx2 should not be in the pool because it double spends tx1 which is in the pool");

        // all wallets will need to wait for tx to confirm in order to properly sync
        TestUtils.WALLET_TX_TRACKER.Reset();
    }

    // Can submit a tx in hex format to the pool then relay
    [Fact(Skip = "Needs monero-wallet-rpc")]
    public void TestSubmitThenRelayTxHex()
    {
        Assert.True(TEST_RELAYS && !LITE_MODE);
        TestUtils.WALLET_TX_TRACKER.WaitForWalletTxsToClearPool(wallet);
        MoneroTx tx = GetUnrelayedTx(wallet, 1);
        TestSubmitThenRelay([tx]);
    }

    // Can submit txs in hex format to the pool then relay
    [Fact(Skip = "Needs monero-wallet-rpc")]
    public void TestSubmitThenRelayTxHexes()
    {
        Assert.True(TEST_RELAYS && !LITE_MODE);
        TestUtils.WALLET_TX_TRACKER.WaitForWalletTxsToClearPool(wallet);
        List<MoneroTx> txs = [];
        txs.Add(GetUnrelayedTx(wallet, 1));
        txs.Add(GetUnrelayedTx(wallet,
            2)); // TODO: accounts cannot be re-used across send tests else isRelayed is true; wallet needs to update?
        TestSubmitThenRelay(txs);
    }

    #endregion

    #region Test Helpers

    private static void MineBlocks()
    {
        try
        {
            StartMining.Start();
            GenUtils.WaitFor(10000);
            StopMining.Stop();
            GenUtils.WaitFor(10000);
        }
        catch (Exception e)
        {
            MoneroUtils.Log(0, e.Message);
        }
    }

    private static void TestBlockHeader(MoneroBlockHeader? header, bool isFull)
    {
        Assert.NotNull(header);
        Assert.True(header.GetHeight() >= 0);
        Assert.True(header.GetMajorVersion() > 0);
        Assert.True(header.GetMinorVersion() >= 0);
        if (header.GetHeight() == 0)
        {
            Assert.True(header.GetTimestamp() == 0);
        }
        else
        {
            Assert.True(header.GetTimestamp() > 0);
        }

        Assert.NotNull(header.GetPrevHash());
        Assert.NotNull(header.GetNonce());
        if (header.GetNonce() == 0)
        {
            MoneroUtils.Log(0,
                "WARNING: header nonce is 0 at height " +
                header.GetHeight()); // TODO (monero-project): why is header nonce 0?
        }
        else
        {
            Assert.True(header.GetNonce() > 0);
        }

        Assert.Null(header.GetPowHash()); // never seen defined
        if (isFull)
        {
            Assert.True(header.GetSize() > 0);
            Assert.True(header.GetDepth() >= 0);
            Assert.True(header.GetDifficulty() > 0);
            Assert.True(header.GetCumulativeDifficulty() > 0);
            Assert.True(64 == header.GetHash().Length);
            Assert.True(64 == header.GetMinerTxHash().Length);
            Assert.True(header.GetNumTxs() >= 0);
            Assert.NotNull(header.GetOrphanStatus());
            Assert.NotNull(header.GetReward());
            Assert.NotNull(header.GetWeight());
            Assert.True(header.GetWeight() > 0);
        }
        else
        {
            Assert.Null(header.GetSize());
            Assert.Null(header.GetDepth());
            Assert.Null(header.GetDifficulty());
            Assert.Null(header.GetCumulativeDifficulty());
            Assert.Null(header.GetHash());
            Assert.Null(header.GetMinerTxHash());
            Assert.Null(header.GetNumTxs());
            Assert.Null(header.GetOrphanStatus());
            Assert.Null(header.GetReward());
            Assert.Null(header.GetWeight());
        }
    }

    private static void TestGetBlocksRange(ulong? startHeight, ulong? endHeight, ulong chainHeight, bool chunked)
    {
        // fetch blocks by range
        ulong realStartHeight = startHeight == null ? 0 : (ulong)startHeight;
        ulong realEndHeight = endHeight == null ? chainHeight - 1 : (ulong)endHeight;
        List<MoneroBlock> blocks = chunked
            ? daemon.GetBlocksByRangeChunked((ulong)startHeight, (ulong)endHeight)
            : daemon.GetBlocksByRange((ulong)startHeight, (ulong)endHeight);
        Assert.True(realEndHeight - realStartHeight + 1 == (ulong)blocks.Count);

        // test each block
        for (int i = 0; i < blocks.Count; i++)
        {
            Assert.True(realStartHeight + (ulong)i == (ulong)blocks[i].GetHeight());
            TestBlock(blocks[i], BINARY_BLOCK_CTX);
        }
    }

    private static List<string> GetConfirmedTxHashes(MoneroDaemon daemon)
    {
        int numTxs = 5;
        List<string> txHashes = [];
        ulong height = daemon.GetHeight();
        while (txHashes.Count < numTxs && height > 0)
        {
            MoneroBlock block = daemon.GetBlockByHeight(--height);
            foreach (string txHash in block.GetTxHashes())
            {
                txHashes.Add(txHash);
            }
        }

        return txHashes;
    }

    private static MoneroTx GetUnrelayedTx(MoneroWallet wallet, uint accountIdx)
    {
        Assert.True(accountIdx > 0,
            "Txs sent from/to same account are not properly synced from the pool"); // TODO monero-project
        MoneroTxConfig config = new MoneroTxConfig().SetAccountIndex(accountIdx).SetAddress(wallet.GetPrimaryAddress())
            .SetAmount(TestUtils.MAX_FEE);
        MoneroTx tx = wallet.CreateTx(config);
        Assert.True(tx.GetFullHex().Length > 0);
        Assert.False(tx.GetRelay());
        return tx;
    }

    private static void TestBlockTemplate(MoneroBlockTemplate template)
    {
        Assert.NotNull(template);
        Assert.NotNull(template.GetBlockTemplateBlob());
        Assert.NotNull(template.GetBlockHashingBlob());
        Assert.NotNull(template.GetDifficulty());
        Assert.NotNull(template.GetExpectedReward());
        Assert.NotNull(template.GetHeight());
        Assert.NotNull(template.GetPrevHash());
        Assert.NotNull(template.GetReservedOffset());
        Assert.NotNull(template.GetSeedHeight());
        // regtest daemon has seed height equal to zero
        //Assert.True(template.GetSeedHeight() > 0);
        Assert.NotNull(template.GetSeedHash());
        Assert.True(template.GetSeedHash().Length > 0);
        // next seed hash can be null or initialized  // TODO: test circumstances for each
    }

    // TODO: test block deep copy
    private static void TestBlock(MoneroBlock block, TestContext ctx)
    {
        // test required fields
        Assert.NotNull(block);
        TestMinerTx(block.GetMinerTx()); // TODO: miner tx doesn't have as much stuff, can't call TestTx?
        TestBlockHeader(block, ctx.headerIsFull == true);

        if (ctx.hasHex == true)
        {
            Assert.NotNull(block.GetHex());
            Assert.True(block.GetHex().Length > 1);
        }
        else
        {
            Assert.NotNull(block.GetHex());
        }

        if (ctx.hasTxs == true)
        {
            Assert.NotNull(ctx.txContext);
            foreach (MoneroTx tx in block.GetTxs())
            {
                Assert.True(block == tx.GetBlock());
                TestTx(tx, ctx.txContext);
            }
        }
        else
        {
            Assert.Null(ctx.txContext);
            Assert.Null(block.GetTxs());
        }
    }

    private static void TestMinerTx(MoneroTx? minerTx)
    {
        Assert.NotNull(minerTx);
        Assert.NotNull(minerTx.IsMinerTx());
        Assert.True(minerTx.GetVersion() >= 0);
        Assert.NotNull(minerTx.GetExtra());
        Assert.True(minerTx.GetExtra().Length > 0);
        Assert.True(minerTx.GetUnlockTime() >= 0);

        //    // TODO: miner tx does not have hashes in binary requests so this will fail, need to derive using prunable data
        //    TestContext ctx = new TestContext();
        //    ctx.hasJson = false;
        //    ctx.isPruned = true;
        //    ctx.isFull = false;
        //    ctx.isConfirmed = true;
        //    ctx.isMiner = true;
        //    ctx.fromGetTxPool = true;
        //    testTx(minerTx, ctx);
    }

    private static void TestTx(MoneroTx? tx, TestContext? ctx)
    {
        // check inputs
        Assert.NotNull(tx);
        Assert.NotNull(ctx);
        Assert.NotNull(ctx.isPruned);
        Assert.NotNull(ctx.isConfirmed);
        Assert.NotNull(ctx.fromGetTxPool);

        // standard across all txs
        Assert.True(64 == tx.GetHash().Length);
        if (tx.IsRelayed() == null)
        {
            Assert.True(tx.InTxPool()); // TODO monerod: add relayed to get_transactions
        }
        else
        {
            Assert.NotNull(tx.IsRelayed());
        }

        Assert.NotNull(tx.IsConfirmed());
        Assert.NotNull(tx.InTxPool());
        Assert.NotNull(tx.IsMinerTx());
        Assert.NotNull(tx.IsDoubleSpendSeen());
        Assert.True(tx.GetVersion() >= 0);
        Assert.True(tx.GetUnlockTime() >= 0);
        Assert.NotNull(tx.GetInputs());
        Assert.NotNull(tx.GetOutputs());
        Assert.True(tx.GetExtra().Length > 0);
        TestUtils.TestUnsignedBigInteger(tx.GetFee(), true);

        // test presence of output indices
        // TODO: change this over to outputs only
        if (tx.IsMinerTx() == true)
        {
            Assert.Null(tx.GetOutputIndices()); // TODO: how to get output indices for miner transactions?
        }

        if (tx.InTxPool() == true || ctx.fromGetTxPool == true || ctx.hasOutputIndices == false)
        {
            Assert.Null(tx.GetOutputIndices());
        }
        else
        {
            Assert.NotNull(tx.GetOutputIndices());
        }

        if (tx.GetOutputIndices() != null)
        {
            Assert.True(tx.GetOutputIndices().Count > 0);
        }

        // test confirmed ctx
        if (ctx.isConfirmed == true)
        {
            Assert.True(tx.IsConfirmed());
        }

        if (ctx.isConfirmed == false)
        {
            Assert.False(tx.IsConfirmed());
        }

        // test confirmed
        if (tx.IsConfirmed() == true)
        {
            Assert.NotNull(tx.GetBlock());
            Assert.Contains(tx, tx.GetBlock().GetTxs());
            Assert.True(tx.GetBlock().GetHeight() > 0);
            Assert.Contains(tx, tx.GetBlock().GetTxs());
            Assert.True(tx.GetBlock().GetHeight() > 0);
            Assert.True(tx.GetBlock().GetTimestamp() > 0);
            Assert.True(tx.GetRelay());
            Assert.True(tx.IsRelayed());
            Assert.False(tx.IsFailed());
            Assert.False(tx.InTxPool());
            Assert.False(tx.IsDoubleSpendSeen());
            if (ctx.fromBinaryBlock == true)
            {
                Assert.Null(tx.GetNumConfirmations());
            }
            else
            {
                Assert.True(tx.GetNumConfirmations() > 0);
            }
        }
        else
        {
            Assert.Null(tx.GetBlock());
            Assert.True(0 == (long)tx.GetNumConfirmations());
        }

        // test in tx pool
        if (tx.InTxPool() == true)
        {
            Assert.False(tx.IsConfirmed());
            Assert.False(tx.IsDoubleSpendSeen());
            Assert.Null(tx.GetLastFailedHeight());
            Assert.Null(tx.GetLastFailedHash());
            Assert.True(tx.GetReceivedTimestamp() > 0);
            if (ctx.fromGetTxPool == true)
            {
                Assert.True(tx.GetSize() > 0);
                Assert.True(tx.GetWeight() > 0);
                Assert.NotNull(tx.IsKeptByBlock());
                Assert.True(tx.GetMaxUsedBlockHeight() >= 0);
                Assert.NotNull(tx.GetMaxUsedBlockHash());
            }

            Assert.Null(tx.GetLastFailedHeight());
            Assert.Null(tx.GetLastFailedHash());
        }
        else
        {
            Assert.Null(tx.GetLastRelayedTimestamp());
        }

        // test miner tx
        if (tx.IsMinerTx() == true)
        {
            Assert.True(0 == tx.GetFee());
            Assert.Null(tx.GetInputs());
            Assert.Null(tx.GetSignatures());
        }
        else
        {
            if (tx.GetSignatures() != null)
            {
                Assert.True(tx.GetSignatures().Count > 0);
            }
        }

        // test failed  // TODO: what else to test associated with failed
        if (tx.IsFailed() == true)
        {
            Assert.True(tx.GetReceivedTimestamp() > 0);
        }
        else
        {
            if (tx.IsRelayed() == null)
            {
                Assert.Null(tx.GetRelay()); // TODO monerod: add relayed to get_transactions
            }
            else if (tx.IsRelayed() == true)
            {
                Assert.False(tx.IsDoubleSpendSeen());
            }
            else
            {
                Assert.False(tx.IsRelayed());
                if (ctx.fromGetTxPool == true)
                {
                    Assert.False(tx.GetRelay());
                    Assert.NotNull(tx.IsDoubleSpendSeen());
                }
            }
        }

        Assert.Null(tx.GetLastFailedHeight());
        Assert.Null(tx.GetLastFailedHash());

        // received time only for tx pool or failed txs
        if (tx.GetReceivedTimestamp() != null)
        {
            Assert.True(tx.InTxPool() == true || tx.IsFailed() == true);
        }

        // test inputs and outputs
        if (tx.IsMinerTx() == false)
        {
            Assert.True(tx.GetInputs().Count > 0);
        }

        foreach (MoneroOutput input in tx.GetInputs())
        {
            Assert.True(tx == input.GetTx());
            TestInput(input);
        }

        Assert.True(tx.GetOutputs().Count > 0);
        foreach (MoneroOutput output in tx.GetOutputs())
        {
            Assert.True(tx == output.GetTx());
            TestOutput(output, ctx);
        }

        // test pruned vs not pruned
        if (ctx.fromGetTxPool == true || ctx.fromBinaryBlock == true)
        {
            Assert.Null(tx
                .GetPrunableHash()); // TODO monerod: tx pool txs do not have prunable hash, TODO: GetBlocksByHeight() has inconsistent client-side pruning
        }
        else
        {
            Assert.NotNull(tx.GetPrunableHash());
        }

        if (ctx.isPruned == true)
        {
            Assert.Null(tx.GetRctSigPrunable());
            Assert.Null(tx.GetSize());
            Assert.Null(tx.GetLastRelayedTimestamp());
            Assert.Null(tx.GetReceivedTimestamp());
            Assert.Null(tx.GetFullHex());
            Assert.NotNull(tx.GetPrunedHex());
        }
        else
        {
            Assert.Null(tx.GetPrunedHex());
            if (ctx.fromBinaryBlock == true)
            {
                Assert.Null(tx.GetFullHex()); // TODO: GetBlocksByHeight() has inconsistent client-side pruning
            }
            else
            {
                Assert.True(tx.GetFullHex().Length > 0);
            }

            if (ctx.fromBinaryBlock == true)
            {
                Assert.Null(tx.GetRctSigPrunable()); // TODO: GetBlocksByHeight() has inconsistent client-side pruning
            }

            //else Assert.NotNull((tx.GetRctSigPrunable()); // TODO: define and test this
            Assert.False(tx.IsDoubleSpendSeen());
            if (tx.IsConfirmed() == true)
            {
                Assert.Null(tx.GetLastRelayedTimestamp());
                Assert.Null(tx.GetReceivedTimestamp());
            }
            else
            {
                if (tx.IsRelayed() == true)
                {
                    Assert.True(tx.GetLastRelayedTimestamp() > 0);
                }
                else
                {
                    Assert.Null(tx.GetLastRelayedTimestamp());
                }

                Assert.True(tx.GetReceivedTimestamp() > 0);
            }
        }

        if (tx.IsFailed() == true)
        {
            // TODO: implement this
        }

        // test deep copy
        if (true != ctx.doNotTestCopy)
        {
            TestTxCopy(tx, ctx);
        }
    }

    private static void TestInput(MoneroOutput input)
    {
        TestOutput(input);
        TestKeyImage(input.GetKeyImage());
        Assert.True(input.GetRingOutputIndices().Count > 0);
    }

    private static void TestKeyImage(MoneroKeyImage image)
    {
        Assert.True(image.GetHex().Length > 0);
        if (image.GetSignature() != null)
        {
            Assert.NotNull(image.GetSignature());
            Assert.True(image.GetSignature().Length > 0);
        }
    }

    private static void TestOutput(MoneroOutput output, TestContext ctx)
    {
        TestOutput(output);
        if (output.GetTx().InTxPool() == true || ctx.hasOutputIndices == false)
        {
            Assert.Null(output.GetIndex());
        }
        else
        {
            Assert.True(output.GetIndex() >= 0);
        }

        Assert.True(64 == output.GetStealthPublicKey().Length);
    }

    private static void TestOutput(MoneroOutput output)
    {
        TestUtils.TestUnsignedBigInteger(output.GetAmount());
    }

    private static void TestTxCopy(MoneroTx tx, TestContext ctx)
    {
        // copy tx and assert deep equality
        MoneroTx copy = tx.Clone();
        //Assert.That(copy instanceof MoneroTx);
        Assert.Null(copy.GetBlock());
        if (tx.GetBlock() != null)
        {
            copy.SetBlock(tx.GetBlock().Clone().SetTxs([copy]));
        }

        Assert.True(tx.ToString() == copy.ToString());
        Assert.True(copy != tx);

        // test different input references
        if (copy.GetInputs() == null)
        {
            Assert.Null(tx.GetInputs());
        }
        else
        {
            Assert.True(copy.GetInputs() != tx.GetInputs());
            for (int i = 0; i < copy.GetInputs().Count; i++)
            {
                Assert.True(tx.GetInputs()[i].GetAmount().Equals(copy.GetInputs()[i].GetAmount()));
            }
        }

        // test different output references
        if (copy.GetOutputs() == null)
        {
            Assert.Null(tx.GetOutputs());
        }
        else
        {
            Assert.True(copy.GetOutputs() != tx.GetOutputs());
            for (int i = 0; i < copy.GetOutputs().Count; i++)
            {
                Assert.True(tx.GetOutputs()[i].GetAmount().Equals(copy.GetOutputs()[i].GetAmount()));
            }
        }

        // test copied tx
        ctx = new TestContext(ctx);
        ctx.doNotTestCopy = true; // to prevent infinite recursion
        if (tx.GetBlock() != null)
        {
            copy.SetBlock(tx.GetBlock().Clone().SetTxs([copy])); // copy block for testing
        }

        TestTx(copy, ctx);

        // test merging with copy
        //oneroTx merged = copy.Merge(copy.Clone());
        //Assert.That(tx.tostring(), merged.tostring());
    }

    private static void TestMinerTxSum(MoneroMinerTxSum txSum)
    {
        TestUtils.TestUnsignedBigInteger(txSum.GetEmissionSum(), true);
        TestUtils.TestUnsignedBigInteger(txSum.GetFeeSum(), true);
    }

    private static void TestTxPoolStats(MoneroTxPoolStats? stats)
    {
        Assert.NotNull(stats);
        Assert.True(stats.GetNumTxs() >= 0);
        if (stats.GetNumTxs() > 0)
        {
            if (stats.GetNumTxs() == 1)
            {
                Assert.Null(stats.GetHisto());
            }
            else
            {
                Dictionary<ulong, int>? histo = stats.GetHisto();
                Assert.NotNull(histo);
                Assert.True(histo.Count > 0);
                foreach (KeyValuePair<ulong, int> kv in histo)
                {
                    Assert.True(kv.Value >= 0);
                }
            }

            Assert.True(stats.GetBytesMax() > 0);
            Assert.True(stats.GetBytesMed() > 0);
            Assert.True(stats.GetBytesMin() > 0);
            Assert.True(stats.GetBytesTotal() > 0);
            Assert.True(stats.GetHisto98pc() == null || stats.GetHisto98pc() > 0);
            Assert.True(stats.GetOldestTimestamp() > 0);
            Assert.True(stats.GetNum10m() >= 0);
            Assert.True(stats.GetNumDoubleSpends() >= 0);
            Assert.True(stats.GetNumFailing() >= 0);
            Assert.True(stats.GetNumNotRelayed() >= 0);
        }
        else
        {
            Assert.Null(stats.GetBytesMax());
            Assert.Null(stats.GetBytesMed());
            Assert.Null(stats.GetBytesMin());
            Assert.True(0 == (long)stats.GetBytesTotal());
            Assert.Null(stats.GetHisto98pc());
            Assert.Null(stats.GetOldestTimestamp());
            Assert.True(0 == (int)stats.GetNum10m());
            Assert.True(0 == (int)stats.GetNumDoubleSpends());
            Assert.True(0 == (int)stats.GetNumFailing());
            Assert.True(0 == (int)stats.GetNumNotRelayed());
            Assert.Null(stats.GetHisto());
        }
    }

    // helper function to check the spent status of a key image or array of key images
    private static void TestSpentStatuses(List<string> keyImages,
        MoneroKeyImage.SpentStatus expectedStatus)
    {
        // test image
        foreach (string keyImage in keyImages)
        {
            Assert.True(expectedStatus == daemon.GetKeyImageSpentStatus(keyImage));
        }

        // test array of images
        List<MoneroKeyImage.SpentStatus> statuses =
            keyImages.Count == 0 ? [] : daemon.GetKeyImageSpentStatuses(keyImages);
        Assert.True(keyImages.Count == statuses.Count);
        foreach (MoneroKeyImage.SpentStatus status in statuses)
        {
            Assert.True(expectedStatus == status);
        }
    }

    private static List<MoneroTx> GetConfirmedTxs(MoneroDaemon daemon, int numTxs)
    {
        List<MoneroTx> txs = [];
        ulong numBlocksPerReq = 50;
        for (ulong startIdx = daemon.GetHeight() - numBlocksPerReq - 1; startIdx >= 0; startIdx -= numBlocksPerReq)
        {
            List<MoneroBlock> blocks =
                daemon.GetBlocksByRange(startIdx, startIdx + numBlocksPerReq);
            foreach (MoneroBlock block in blocks)
            {
                if (block.GetTxs() == null)
                {
                    continue;
                }

                foreach (MoneroTx tx in block.GetTxs())
                {
                    txs.Add(tx);
                    if (txs.Count == numTxs)
                    {
                        return txs;
                    }
                }
            }
        }

        throw new MoneroError("Could not get " + numTxs + " confirmed txs");
    }

    private static void TestOutputDistributionEntry(MoneroOutputDistributionEntry entry)
    {
        TestUtils.TestUnsignedBigInteger(entry.GetAmount());
        Assert.True(entry.GetBase() >= 0);
        Assert.True(entry.GetDistribution().Count > 0);
        Assert.True(entry.GetStartHeight() >= 0);
    }

    private static void TestInfo(MoneroDaemonInfo info)
    {
        Assert.NotNull(info.GetVersion());
        Assert.True(info.GetNumAltBlocks() >= 0);
        Assert.True(info.GetBlockSizeLimit() > 0);
        Assert.True(info.GetBlockSizeMedian() > 0);
        Assert.True(info.GetBootstrapDaemonAddress() == null || info.GetBootstrapDaemonAddress().Length > 0);
        TestUtils.TestUnsignedBigInteger(info.GetCumulativeDifficulty());
        TestUtils.TestUnsignedBigInteger(info.GetFreeSpace());
        Assert.True(info.GetNumOfflinePeers() >= 0);
        Assert.True(info.GetNumOnlinePeers() >= 0);
        Assert.True(info.GetHeight() >= 0);
        Assert.True(info.GetNumIncomingConnections() >= 0);
        Assert.NotNull(info.GetNetworkType());
        Assert.NotNull(info.IsOffline());
        Assert.True(info.GetNumOutgoingConnections() >= 0);
        Assert.True(info.GetNumRpcConnections() >= 0);
        Assert.True(info.GetAdjustedTimestamp() > 0);
        Assert.True(info.GetTarget() > 0);
        Assert.True(info.GetTargetHeight() >= 0);
        Assert.True(info.GetNumTxs() >= 0);
        Assert.True(info.GetNumTxsPool() >= 0);
        Assert.NotNull(info.GetWasBootstrapEverUsed());
        Assert.True(info.GetBlockWeightLimit() > 0);
        Assert.True(info.GetBlockWeightMedian() > 0);
        Assert.True(info.GetDatabaseSize() > 0);
        Assert.NotNull(info.GetUpdateAvailable());
        TestUtils.TestUnsignedBigInteger(info.GetCredits(), false); // 0 credits
        string? topBlockHash = info.GetTopBlockHash();
        Assert.NotNull(topBlockHash);
        Assert.True(topBlockHash.Length > 0);
        Assert.NotNull(info.IsBusySyncing());
        Assert.NotNull(info.IsSynchronized());

        if (info.IsRestricted() == false)
        {
            Assert.True(info.GetHeightWithoutBootstrap() > 0);
            Assert.True(info.GetStartTimestamp() > 0);
        }
    }

    private static void TestSyncInfo(MoneroDaemonSyncInfo syncInfo)
    {
        // TODO: consistent naming, daemon in name?
        MoneroDaemonSyncInfo testObj = new();

        Assert.True(testObj.GetType().IsInstanceOfType(syncInfo));
        Assert.True(syncInfo.GetHeight() >= 0);
        if (syncInfo.GetPeers() != null)
        {
            Assert.True(syncInfo.GetPeers().Count > 0);
            foreach (MoneroPeer connection in syncInfo.GetPeers())
            {
                TestPeer(connection);
            }
        }

        if (syncInfo.GetSpans() != null)
        {
            // TODO: test that this is being hit, so far not used
            Assert.True(syncInfo.GetSpans().Count > 0);
            foreach (MoneroConnectionSpan span in syncInfo.GetSpans())
            {
                TestConnectionSpan(span);
            }
        }

        Assert.True(syncInfo.GetNextNeededPruningSeed() >= 0);
        Assert.Null(syncInfo.GetOverview());
        TestUtils.TestUnsignedBigInteger(syncInfo.GetCredits(), false); // 0 credits
        Assert.Null(syncInfo.GetTopBlockHash());
    }

    private static void TestConnectionSpan(MoneroConnectionSpan? span)
    {
        Assert.NotNull(span);
        Assert.NotNull(span.GetConnectionId());
        Assert.True(span.GetConnectionId().Length > 0);
        Assert.True(span.GetStartHeight() > 0);
        Assert.True(span.GetNumBlocks() > 0);
        Assert.True(span.GetRemoteAddress() == null || span.GetRemoteAddress().Length > 0);
        Assert.True(span.GetRate() > 0);
        Assert.True(span.GetSpeed() >= 0);
        Assert.True(span.GetSize() > 0);
    }

    private static void TestHardForkInfo(MoneroHardForkInfo hardForkInfo)
    {
        Assert.NotNull(hardForkInfo.GetEarliestHeight());
        Assert.NotNull(hardForkInfo.IsEnabled());
        Assert.NotNull(hardForkInfo.GetState());
        Assert.NotNull(hardForkInfo.GetThreshold());
        Assert.NotNull(hardForkInfo.GetVersion());
        Assert.NotNull(hardForkInfo.GetNumVotes());
        Assert.NotNull(hardForkInfo.GetVoting());
        Assert.NotNull(hardForkInfo.GetWindow());
        TestUtils.TestUnsignedBigInteger(hardForkInfo.GetCredits(), false); // 0 credits
        Assert.Null(hardForkInfo.GetTopBlockHash());
    }

    private static void TestMoneroBan(MoneroBan ban)
    {
        Assert.NotNull(ban.GetHost());
        Assert.NotNull(ban.GetIp());
        Assert.NotNull(ban.GetSeconds());
    }

    private static void TestAltChain(MoneroAltChain altChain)
    {
        Assert.NotNull(altChain);
        Assert.True(altChain.GetBlockHashes().Count > 0);
        TestUtils.TestUnsignedBigInteger(altChain.GetDifficulty(), true);
        Assert.True(altChain.GetHeight() > 0);
        Assert.True(altChain.GetLength() > 0);
        Assert.True(64 == altChain.GetMainChainParentBlockHash().Length);
    }

    private static void TestPeer(MoneroPeer peer)
    {
        //Assert.That(peer instanceof MoneroPeer);
        TestKnownPeer(peer, true);
        Assert.True(peer.GetHash().Length > 0);
        Assert.True(peer.GetAvgDownload() >= 0);
        Assert.True(peer.GetAvgUpload() >= 0);
        Assert.True(peer.GetCurrentDownload() >= 0);
        Assert.True(peer.GetCurrentUpload() >= 0);
        Assert.True(peer.GetHeight() >= 0);
        Assert.True(peer.GetLiveTime() >= 0);
        Assert.True(peer.GetNumReceives() >= 0);
        Assert.True(peer.GetReceiveIdleTime() >= 0);
        Assert.True(peer.GetNumSends() >= 0);
        Assert.True(peer.GetSendIdleTime() >= 0);
        Assert.NotNull(peer.GetState());
        Assert.True(peer.GetNumSupportFlags() >= 0);
        Assert.NotNull(peer.GetConnectionType());
    }

    private static void TestKnownPeer(MoneroPeer peer, bool fromConnection)
    {
        Assert.NotNull(peer);
        Assert.True(peer.GetId().Length > 0);
        Assert.True(peer.GetHost().Length > 0);
        Assert.True(peer.GetPort() > 0);
        Assert.True(peer.GetRpcPort() == null || peer.GetRpcPort() >= 0);
        //if (peer.GetRpcCreditsPerHash() != null) TestUtils.testUnsignedBigInteger(peer.GetRpcCreditsPerHash());
        if (fromConnection)
        {
            Assert.Null(peer.GetLastSeenTimestamp());
        }
        else
        {
            if (peer.GetLastSeenTimestamp() < 0)
            {
                MoneroUtils.Log(0, "Last seen timestamp is invalid: " + peer.GetLastSeenTimestamp());
            }

            Assert.True(peer.GetLastSeenTimestamp() >= 0);
        }

        Assert.True(peer.GetPruningSeed() == null || peer.GetPruningSeed() >= 0);
    }

    private static void TestUpdateCheckResult(MoneroDaemonUpdateCheckResult result)
    {
        //Assert.That(result instanceof MoneroDaemonUpdateCheckResult);
        Assert.NotNull(result.IsUpdateAvailable());
        if (result.IsUpdateAvailable() == true)
        {
            Assert.True(result.GetAutoUri().Length > 0, "No auto uri; is daemon online?");
            Assert.True(result.GetUserUri().Length > 0);
            Assert.True(result.GetVersion().Length > 0);
            Assert.True(result.GetHash().Length > 0);
            Assert.True(64 == result.GetHash().Length);
        }
        else
        {
            Assert.Null(result.GetAutoUri());
            Assert.Null(result.GetUserUri());
            Assert.Null(result.GetVersion());
            Assert.Null(result.GetHash());
        }
    }

    private static void TestUpdateDownloadResult(MoneroDaemonUpdateDownloadResult result, string? path)
    {
        TestUpdateCheckResult(result);
        if (result.IsUpdateAvailable() == true)
        {
            if (path != null)
            {
                Assert.True(path == result.GetDownloadPath());
            }
            else
            {
                Assert.NotNull(result.GetDownloadPath());
            }
        }
        else
        {
            Assert.Null(result.GetDownloadPath());
        }
    }

    private static void TestSubmitTxResultGood(MoneroSubmitTxResult result)
    {
        TestSubmitTxResultCommon(result);
        try
        {
            Assert.False(result.IsDoubleSpend(), "tx submission is double spend.");
            Assert.False(result.IsFeeTooLow());
            Assert.False(result.IsMixinTooLow());
            Assert.False(result.HasInvalidInput());
            Assert.False(result.HasInvalidOutput());
            Assert.False(result.HasTooFewOutputs());
            Assert.False(result.IsOverspend());
            Assert.False(result.IsTooBig());
            Assert.False(result.GetSanityCheckFailed());
            TestUtils.TestUnsignedBigInteger(result.GetCredits(), false); // 0 credits
            Assert.Null(result.GetTopBlockHash());
            Assert.False(result.IsTxExtraTooBig());
            Assert.True(result.IsGood());
            Assert.False(result.IsNonzeroUnlockTime());
        }
        catch (Exception e)
        {
            MoneroUtils.Log(0, "Submit result is not good");
            throw;
        }
    }

    private static void TestSubmitTxResultDoubleSpend(MoneroSubmitTxResult result)
    {
        TestSubmitTxResultCommon(result);
        Assert.False(result.IsGood());
        Assert.True(result.IsDoubleSpend());
        Assert.False(result.IsFeeTooLow());
        Assert.False(result.IsMixinTooLow());
        Assert.False(result.HasInvalidInput());
        Assert.False(result.HasInvalidOutput());
        Assert.False(result.IsOverspend());
        Assert.False(result.IsTooBig());
    }

    private static void TestSubmitTxResultCommon(MoneroSubmitTxResult result)
    {
        Assert.NotNull(result.IsGood());
        Assert.NotNull(result.IsRelayed());
        Assert.NotNull(result.IsDoubleSpend());
        Assert.NotNull(result.IsFeeTooLow());
        Assert.NotNull(result.IsMixinTooLow());
        Assert.NotNull(result.HasInvalidInput());
        Assert.NotNull(result.HasInvalidOutput());
        Assert.NotNull(result.IsOverspend());
        Assert.NotNull(result.IsTooBig());
        Assert.NotNull(result.GetSanityCheckFailed());
        Assert.True(result.GetReason() == null || result.GetReason().Length > 0);
    }

    private static void TestOutputHistogramEntry(MoneroOutputHistogramEntry entry)
    {
        TestUtils.TestUnsignedBigInteger(entry.GetAmount());
        Assert.True(entry.GetNumInstances() >= 0);
        Assert.True(entry.GetNumUnlockedInstances() >= 0);
        Assert.True(entry.GetNumRecentInstances() >= 0);
    }

    private static void TestSubmitThenRelay(List<MoneroTx> txs)
    {
        // submit txs hex but don't relay
        List<string> txHashes = [];
        foreach (MoneroTx tx in txs)
        {
            txHashes.Add(tx.GetHash());
            MoneroSubmitTxResult result = daemon.SubmitTxHex(tx.GetFullHex(), true);
            TestSubmitTxResultGood(result);
            Assert.False(result.IsRelayed());

            // ensure tx is in pool
            List<MoneroTx> _poolTxs = daemon.GetTxPool();
            bool found = false;
            foreach (MoneroTx aTx in _poolTxs)
            {
                if (aTx.GetHash().Equals(tx.GetHash()))
                {
                    Assert.False(aTx.IsRelayed());
                    found = true;
                    break;
                }
            }

            Assert.True(found, "Tx was not found after being submitted to the daemon's tx pool");

            // fetch tx by hash and ensure not relayed
            MoneroTx? fetchedTx = daemon.GetTx(tx.GetHash());
            Assert.False(fetchedTx.IsRelayed());
        }

        // relay the txs
        try
        {
            if (txHashes.Count == 1)
            {
                daemon.RelayTxByHash(txHashes[0]);
            }
            else
            {
                daemon.RelayTxsByHash(txHashes);
            }
        }
        catch (Exception e)
        {
            daemon.FlushTxPool(txHashes); // flush txs when relay fails to prevent double spends in other tests
            throw;
        }

        // wait for txs to be relayed // TODO (monero-project): all txs should be relayed: https://github.com/monero-project/monero/issues/8523
        try { Thread.Sleep(1000); }
        catch (Exception e) { throw new Exception(e.Message); }

        // ensure txs are relayed
        List<MoneroTx> poolTxs = daemon.GetTxPool();
        foreach (MoneroTx tx in txs)
        {
            bool found = false;
            foreach (MoneroTx aTx in poolTxs)
            {
                if (aTx.GetHash().Equals(tx.GetHash()))
                {
                    Assert.True(aTx.IsRelayed());
                    found = true;
                    break;
                }
            }

            Assert.True(found, "Tx was not found after being submitted to the daemon's tx pool");
        }

        // wallets will need to wait for tx to confirm in order to properly sync
        TestUtils.WALLET_TX_TRACKER.Reset();
    }

    #endregion
}

public class TestContext
{
    public bool? doNotTestCopy;
    public bool? fromBinaryBlock;
    public bool? fromGetTxPool;
    public bool? hasHex;
    public bool? hasJson;
    public bool? hasOutputIndices;
    public bool? hasTxs;
    public bool? headerIsFull;
    public bool? isConfirmed;
    public bool? isFull;
    public bool? isMinerTx;
    public bool? isPruned;
    public TestContext? txContext;

    public TestContext() { }

    public TestContext(TestContext ctx)
    {
        hasJson = ctx.hasJson;
        isPruned = ctx.isPruned;
        isFull = ctx.isFull;
        isConfirmed = ctx.isConfirmed;
        isMinerTx = ctx.isMinerTx;
        fromGetTxPool = ctx.fromGetTxPool;
        fromBinaryBlock = ctx.fromBinaryBlock;
        hasOutputIndices = ctx.hasOutputIndices;
        doNotTestCopy = ctx.doNotTestCopy;
        hasTxs = ctx.hasTxs;
        hasHex = ctx.hasHex;
        headerIsFull = ctx.headerIsFull;
    }
}