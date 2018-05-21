using System;
using NBitcoin;
using Stratis.Bitcoin;
using Stratis.Bitcoin.Features.BlockStore;
using Zorbit.Features.Observatory.Core;
using Zorbit.Features.Observatory.Core.Indexing;
using Zorbit.Features.Observatory.Core.Model;
using Zorbit.Features.Observatory.TableStorage.Indexing.Tasks;

namespace Zorbit.Features.Observatory.TableStorage.Indexing
{
    public sealed class IndexerTaskFactory : IIndexerTaskFactory
    {
        private readonly Network _network;
        private readonly IBlockRepository _blockRepository;
        private readonly AzureStorageClient _storageClient;
        private readonly IndexerSettings _settings;

        public IndexerTaskFactory(
            FullNode node,
            IBlockRepository blockRepository,
            AzureStorageClient storageClient,
            IndexerSettings settings)
        {
            _network = node.Network;
            _blockRepository = blockRepository;
            _storageClient = storageClient;
            _settings = settings;
        }

        public IIndexerTask CreateTask(IndexType indexType)
        {
            switch (indexType)
            {
                case IndexType.Block:
                    return new BlockTask(_storageClient, _settings);
                case IndexType.Summary:
                    return new BlockSummaryTask(_storageClient, _settings);
                case IndexType.Transaction:
                    return new TransactionTask(_storageClient, _settings);
                case IndexType.Address:
                    return new AddressTask(_network, _blockRepository, _storageClient, _settings);
                default:
                    throw new ArgumentOutOfRangeException(nameof(indexType), indexType, null);
            }
        }
    }
}
