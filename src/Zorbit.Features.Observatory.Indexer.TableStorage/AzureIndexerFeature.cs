using System;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin;
using Stratis.Bitcoin.Builder;
using Stratis.Bitcoin.Builder.Feature;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.Configuration.Logging;
using Stratis.Bitcoin.Features.BlockStore;
using Stratis.Bitcoin.Interfaces;
using Zorbit.Features.Observatory.Core;
using Zorbit.Features.Observatory.Core.Indexing;
using Zorbit.Features.Observatory.Core.Node;
using Zorbit.Features.Observatory.TableStorage.Indexing;

[assembly: InternalsVisibleTo("Stratis.Bitcoin.Features.AzureIndexer.Tests")]

namespace Zorbit.Features.Observatory.TableStorage
{
    /// <summary>
    /// The AzureIndexerFeature provides the ".UseAzureIndexer" extension.
    /// </summary>
    public class AzureIndexerFeature : FullNodeFeature, INodeStats
    {
        /// <summary>The full node.</summary>
        private readonly FullNode _fullNode;

        /// <summary>The loop responsible for indexing blocks to azure.</summary>
        private readonly AzureIndexerLoop _indexerLoop;

        /// <summary>The node's settings.</summary>
        private readonly NodeSettings _nodeSettings;

        /// <summary>The Block Store settings.</summary>
        private readonly StoreSettings _storeSettings;

        /// <summary>The Azure Indexer settings.</summary>
        private readonly IndexerSettings _indexerSettings;

        /// <summary>The Azure Storage client.</summary>
        private readonly AzureStorageClient _storageClient;

        /// <summary>Instance logger.</summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the Azure Indexer feature
        /// </summary>
        /// <param name="fullNode"></param>
        /// <param name="storageClient"></param>
        /// <param name="azureIndexerLoop"></param>
        /// <param name="nodeSettings"></param>
        /// <param name="indexerSettings"></param>
        /// <param name="storeSettings"></param>
        /// <param name="loggerFactory"></param>
        public AzureIndexerFeature(
            FullNode fullNode,
            AzureStorageClient storageClient,
            AzureIndexerLoop azureIndexerLoop,
            NodeSettings nodeSettings,
            IndexerSettings indexerSettings,
            StoreSettings storeSettings,
            ILoggerFactory loggerFactory)
        {
            this._fullNode = fullNode;
            this._storageClient = storageClient;
            this._indexerLoop = azureIndexerLoop;
            this._nodeSettings = nodeSettings;
            this._indexerSettings = indexerSettings;
            this._storeSettings = storeSettings;
            this._logger = loggerFactory.CreateLogger<AzureIndexerFeature>();
        }

        /// <summary>
        /// Displays statistics in the console.
        /// </summary>
        /// <param name="benchLogs">The sring builder to add the statistics to.</param>
        public void AddNodeStats(StringBuilder benchLogs)
        {
            this._indexerLoop.AddNodeStats(benchLogs);
        }

        /// <summary>
        /// Starts the Azure Indexer feature.
        /// </summary>
        public override void Initialize()
        {
            this._logger.LogTrace("()");
            this._storeSettings.TxIndex = true;
            this._storageClient.InitaliseAsync().GetAwaiter().GetResult();
            this._indexerLoop.Initialize(this._fullNode.NodeLifetime.ApplicationStopping);
            this._logger.LogTrace("(-)");
        }

        public override void LoadConfiguration()
        {
            this._indexerSettings.Load(this._nodeSettings);
        }

        public static void PrintHelp(Network network)
        {
            IndexerSettings.PrintHelp(network);
        }

        /// <summary>
        /// Stops the Azure Indexer feature.
        /// </summary>
        public override void Dispose()
        {
            this._logger.LogInformation("Stopping Indexer...");
            this._logger.LogTrace("(-)");
        }
    }

    /// <summary>
    /// A class providing extension methods for <see cref="IFullNodeBuilder"/>.
    /// </summary>
    public static partial class FullNodeBuilderExtensions
    {
        public static IFullNodeBuilder UseAzureIndexer(this IFullNodeBuilder fullNodeBuilder, Action<IndexerSettings> setup = null)
        {
            LoggingConfiguration.RegisterFeatureNamespace<AzureIndexerFeature>("azindex");

            fullNodeBuilder.ConfigureFeature(features =>
            {
                features
                .AddFeature<AzureIndexerFeature>()
                .DependOn<BlockStoreFeature>()
                .FeatureServices(services =>
                    {
                        services.AddTransient<AzureStorageClient>();
                        services.AddSingleton<ICheckpointStore, CheckpointStore>();
                        services.AddTransient<IIndexer, Indexer>();
                        services.AddSingleton<IIndexerTaskFactory, IndexerTaskFactory>();
                        services.AddSingleton<INodeBlockStore, NodeBlockStore>();
                        services.AddSingleton<INodeBlockFetcherFactory, NodeBlockFetcherFactory>();
                        services.AddSingleton<AzureIndexerLoop>();
                        services.AddSingleton<IndexerSettings>(new IndexerSettings(setup));
                    });
            });

            return fullNodeBuilder;
        }
    }
}