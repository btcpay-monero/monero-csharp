﻿
namespace Monero.Test.Utils
{
    public static class StartMining
    {

        public static void Start()
        {
            Start(1);
        }

        public static void Start(ulong numThreads)
        {
            //TestUtils.getWalletRpc().startMining(numThreads, false, true);
            //TestUtils.getDaemonRpc().startMining("59dF9pSotECe1Fn4dBGZXWHYyNdo53rbZ7YYseu9jBKCf4c2cUzhuFVRH8HuD4wyaKTqtD3VF3F4eQe3Kzq342F5U8R4jeq", numThreads, false, false); // stagenet
            TestUtils.GetDaemonRpc().StartMining("9tsUiG9bwcU7oTbAdBwBk2PzxFtysge5qcEsHEpetmEKgerHQa1fDqH7a4FiquZmms7yM22jdifVAD7jAb2e63GSJMuhY75", numThreads, false, false); // testnet
        }
    }
}
