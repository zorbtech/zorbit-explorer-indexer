using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Configuration.Logging;
using Stratis.Bitcoin.Utilities;
using Zorbit.Features.Observatory.Core.Indexing;
using Zorbit.Features.Observatory.Core.Model;

namespace Zorbit.Features.Observatory.TableStorage
{
    /// <summary>
    /// The AzureIndexerStoreLoop loads blocks from the block repository and indexes them in Azure.
    /// </summary>
    public sealed class AzureIndexerLoop
    {
        /// <summary>Factory for creating background async loop tasks.</summary>
        private readonly IAsyncLoopFactory _asyncLoopFactory;

        /// <summary>Indexer.</summary>
        private readonly IIndexer _indexer;

        /// <summary>Instance logger.</summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Constructs the AzureIndexerLoop.
        /// </summary>
        /// <param name="fullNode">The full node that will be indexed.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public AzureIndexerLoop(
            IIndexer indexer,
            IAsyncLoopFactory asyncLoopFactory,
            ILoggerFactory loggerFactory)
        {
            this._indexer = indexer;
            this._asyncLoopFactory = asyncLoopFactory;
            this._logger = loggerFactory.CreateLogger(GetType().FullName);
        }

        /// <summary>
        /// Initializes the Azure Indexer.
        /// </summary>
        public void Initialize(CancellationToken cancellationToken)
        {
            this._logger.LogTrace("()");

            this._indexer.Initialize(cancellationToken).GetAwaiter().GetResult();

            this._asyncLoopFactory.Run($"Indexer",
                async token => await _indexer.IndexAsync(cancellationToken).ConfigureAwait(false),
                cancellationToken,
                TimeSpans.RunOnce,
                TimeSpans.FiveSeconds);

            this._logger.LogTrace("(-)");
        }

        public void AddNodeStats(StringBuilder benchLogs)
        {
            benchLogs.AppendLine();
            benchLogs.AppendLine("======Indexing======");

            foreach (var indexer in this._indexer.Indexers)
            {
                var height = indexer.Tip != null ? indexer.Tip.Height : 0;
                var hash = indexer.Tip != null ? indexer.Tip.HashBlock : uint256.Zero;
                benchLogs.AppendLine(string.Format("{0}{1}{2}{3}",
                    $"{indexer.Type}.Height: ".PadRight(LoggingConfiguration.ColumnLength + 1),
                    height.ToString().PadRight(8),
                    $" {indexer.Checkpoint.IndexType}.Hash: ".PadRight(LoggingConfiguration.ColumnLength),
                    hash));
            }
        }
    }
}