using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing;

namespace Stratis.Bitcoin.Features.AzureIndexer.Tests
{
    public class IndexerTester : IDisposable
    {
        private readonly AzureIndexerObsolete _importer;
        private readonly AzureIndexerSettings _settings;

    
        private readonly uint256 KnownBlockId = uint256.Parse("000000000933ea01ad0ee984209779baaec3ced90fa3f408719526f8d77f4943");
        private readonly uint256 UnknownBlockId = uint256.Parse("000000000933ea01ad0ee984209779baaec3ced90fa3f408719526f8d77f4942");
        private readonly uint256 KnownTransactionId = uint256.Parse("4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33b");
        private readonly uint256 UnknownTransactionId = uint256.Parse("4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33c");

        public IndexerTester(string folder)
        {
            // TODO move to di

           // _client = Indexer.StorageClient.CreateIndexerClient();
            // _client.BalancePartitionSize = 1;

            //TestUtils.EnsureNew(folder);

            //var config = AzureIndexerLoop.IndexerConfigFromSettings(
            //    new AzureIndexerSettings() { StorageNamespace = folder }, Network.TestNet);

            //config.EnsureSetup();

            //_importer = config.CreateIndexer();

            //var creating = new List<Task>();
            //foreach (var table in config.EnumerateTables())
            //{
            //    creating.Add(table.CreateIfNotExistsAsync());
            //}

            //creating.Add(config.GetBlocksContainer().CreateIfNotExistsAsync());
            //Task.WaitAll(creating.ToArray());
        }


        #region IDisposable Members

        public void Dispose()
        {
            // TODO: Find a NodeServer replacement and fix this code
            /*
            if (_NodeServer != null)
                _NodeServer.Dispose();
            */
            //if (!Cached)
            //{
            //    foreach (var table in _importer.StorageClient.EnumerateTables())
            //    {
            //        table.CreateIfNotExistsAsync().GetAwaiter().GetResult();
            //        var entities = table.ExecuteQuery(new TableQuery()).ToList();
            //        Parallel.ForEach(entities, e =>
            //        {
            //            table.ExecuteAsync(TableOperation.Delete(e)).GetAwaiter().GetResult();
            //        });
            //    }
            //    var container = _importer.StorageClient.GetBlocksContainer();
            //    var blobs = container.ListBlobsAsync("", true, BlobListingDetails.None).GetAwaiter().GetResult().ToList();

            //    Parallel.ForEach(blobs, b =>
            //    {
            //        if (b is CloudPageBlob)
            //            ((CloudPageBlob)b).DeleteAsync().GetAwaiter().GetResult();
            //        else
            //            ((CloudBlockBlob)b).DeleteAsync().GetAwaiter().GetResult();
            //    });
            //}
        }
        
        #endregion

        // TODO: Fix IndexBlocks and this code
        /*
        internal void ImportCachedBlocks()
        {
            CreateLocalNode().ChainBuilder.Load(@"..\..\..\Data\blocks");
            if (Client.GetBlock(KnownBlockId) == null)
            {
                Indexer.IgnoreCheckpoints = true;
                Indexer.FromHeight = 0;
                Indexer.IndexBlocks();
            }
        }
        
        internal void ImportCachedTransactions()
        {
            CreateLocalNode().ChainBuilder.Load(@"..\..\..\Data\blocks");
            if (Client.GetTransaction(KnownTransactionId) == null)
            {
                Indexer.IgnoreCheckpoints = true;
                Indexer.FromHeight = 0;
                Indexer.IndexTransactions();
            }
        }
        */
        
        // TODO: Find a NodeServer replacement and fix this code
        /*
        NodeServer _NodeServer;
        internal MiniNode CreateLocalNode()
        {
            NodeServer nodeServer = new NodeServer(Client.StorageClient.Network, internalPort: (ushort)RandomUtils.GetInt32());
            nodeServer.Listen();
            _NodeServer = nodeServer;
            Indexer.StorageClient.Node = "127.0.0.1:" + nodeServer.LocalEndpoint.Port;
            return new MiniNode(this, nodeServer);
        }
        */

        internal ChainBuilder CreateChainBuilder()
        {
            return new ChainBuilder(this);
        }

        public bool Cached { get; set; }

        public FullNode FullNode => throw new NotImplementedException();

        public ConcurrentChain Chain => throw new NotImplementedException();

        public AzureIndexerObsolete Indexer => _importer;

        public AzureIndexerSettings Settings => _settings;
    }
}
