using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using Stratis.Bitcoin.Configuration.Logging;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing;

namespace Stratis.Bitcoin.Features.AzureIndexer
{
    /// <summary>
    /// The AzureIndexerStoreLoop loads blocks from the block repository and indexes them in Azure.
    /// </summary>
    public sealed class AzureIndexerLoop
    {
        /// <summary>Factory for creating background async loop tasks.</summary>
        private readonly IAsyncLoopFactory _asyncLoopFactory;

        /// <summary>Async loops for each indexer.</summary>
        private readonly ICollection<IAsyncLoop> _loops = new List<IAsyncLoop>(5);

        /// <summary>The collection containing indexers.</summary>
        private readonly ICollection<IAzureIndexer> _indexers;

        /// <summary>Instance logger.</summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Constructs the AzureIndexerLoop.
        /// </summary>
        /// <param name="fullNode">The full node that will be indexed.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public AzureIndexerLoop(
            ChainIndexer chainIndexer,
            BlockIndexer blockIndexer,
            BlockSummaryIndexer summaryIndexer,
            TransactionIndexer transactionIndexer,
            BalanceIndexer balanceIndexer,
            WalletIndexer walletIndexer,
            IAsyncLoopFactory asyncLoopFactory,
            ILoggerFactory loggerFactory)
        {
            this._asyncLoopFactory = asyncLoopFactory;
            this._logger = loggerFactory.CreateLogger(GetType().FullName);

            this._indexers = new List<IAzureIndexer>(5)
            {
                blockIndexer,
                summaryIndexer,
                //chainIndexer,
                //transactionIndexer,
                balanceIndexer,
                walletIndexer
            };
        }

        /// <summary>
        /// Initializes the Azure Indexer.
        /// </summary>
        public void Initialize(CancellationToken cancellationToken)
        {
            this._logger.LogTrace("()");

            foreach (var indexer in this._indexers)
            {
                indexer.Initialize(cancellationToken).GetAwaiter().GetResult();

                var loop = this._asyncLoopFactory.Run($"{indexer.CheckPointType} Indexer",
                    async token => await indexer.IndexAsync(cancellationToken),
                    cancellationToken,
                    TimeSpans.RunOnce,
                    TimeSpans.FiveSeconds);

                this._loops.Add(loop);
            }

            this._logger.LogTrace("(-)");
        }

        /// <summary>
        /// Shuts down the indexing loops.
        /// </summary>
        public void Shutdown()
        {
            foreach (var loop in this._loops)
            {
                loop.Dispose();
            }
        }

        public void AddNodeStats(StringBuilder benchLogs)
        {
            benchLogs.AppendLine();
            benchLogs.AppendLine("======Indexing======");

            foreach (var indexer in this._indexers)
            {
                var tip = indexer.Tip;
                var height = tip != null ? tip.Height : 0;
                var hash = tip != null ? tip.HashBlock : uint256.Zero;
                benchLogs.AppendLine(string.Format("{0}{1}{2}{3}",
                    $"{indexer.CheckPointType}.Height: ".PadRight(LoggingConfiguration.ColumnLength + 1),
                    height.ToString().PadRight(8),
                    $" {indexer.CheckPointType}.Hash: ".PadRight(LoggingConfiguration.ColumnLength),
                    hash));
            }
        }
    }
}