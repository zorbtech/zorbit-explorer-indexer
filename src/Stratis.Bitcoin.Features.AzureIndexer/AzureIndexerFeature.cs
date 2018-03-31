using System;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Builder;
using Stratis.Bitcoin.Builder.Feature;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.Configuration.Logging;
using Stratis.Bitcoin.Interfaces;

[assembly: InternalsVisibleTo("Stratis.Bitcoin.Features.AzureIndexer.Tests")]

namespace Stratis.Bitcoin.Features.AzureIndexer
{
    /// <summary>
    /// The AzureIndexerFeature provides the ".UseAzureIndexer" extension.
    /// </summary>
    public class AzureIndexerFeature : FullNodeFeature, INodeStats
    {
        /// <summary>The loop responsible for indexing blocks to azure.</summary>
        protected readonly AzureIndexerLoop IndexerLoop;

        /// <summary>The node's settings.</summary>
        protected readonly NodeSettings NodeSettings;

        /// <summary>The Azure Indexer settings.</summary>
        protected readonly AzureIndexerSettings IndexerSettings;

        /// <summary>Instance logger.</summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Constructs the Azure Indexer feature.
        /// </summary>
        /// <param name="azureIndexerLoop">The loop responsible for indexing blocks to azure.</param>
        /// <param name="nodeSettings">The settings of the full node.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="indexerSettings">The Azure Indexer settings.</param>
        public AzureIndexerFeature(
            AzureIndexerLoop azureIndexerLoop,
            NodeSettings nodeSettings,
            ILoggerFactory loggerFactory,
            AzureIndexerSettings indexerSettings)
        {
            this.IndexerLoop = azureIndexerLoop;
            this._logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.NodeSettings = nodeSettings;
            this.IndexerSettings = indexerSettings;
        }

        /// <summary>
        /// Displays statistics in the console.
        /// </summary>
        /// <param name="benchLogs">The sring builder to add the statistics to.</param>
        public void AddNodeStats(StringBuilder benchLogs)
        {
            var highestBlock = this.IndexerLoop.StoreTip;

            if (highestBlock == null)
                return;

            benchLogs.AppendLine($"Index.Height: ".PadRight(LoggingConfiguration.ColumnLength + 1) +
                highestBlock.Height.ToString().PadRight(8) +
                $" Index.Hash: ".PadRight(LoggingConfiguration.ColumnLength - 1) +
                highestBlock.HashBlock);
        }

        /// <summary>
        /// Starts the Azure Indexer feature.
        /// </summary>
        public override void Initialize()
        {
            this._logger.LogTrace("()");
            this.IndexerLoop.Initialize();
            this._logger.LogTrace("(-)");
        }

        public override void LoadConfiguration()
        {
            this.IndexerSettings.Load(this.NodeSettings);
        }

        public static void PrintHelp(Network network)
        {
            AzureIndexerSettings.PrintHelp(network);
        }

        /// <summary>
        /// Stops the Azure Indexer feature.
        /// </summary>
        public override void Dispose()
        {
            this._logger.LogInformation("Stopping Indexer...");
            this.IndexerLoop.Shutdown();
            this._logger.LogTrace("(-)");
        }
    }

    /// <summary>
    /// A class providing extension methods for <see cref="IFullNodeBuilder"/>.
    /// </summary>
    public static partial class FullNodeBuilderExtensions
    {
        public static IFullNodeBuilder UseAzureIndexer(this IFullNodeBuilder fullNodeBuilder, Action<AzureIndexerSettings> setup = null)
        {
            LoggingConfiguration.RegisterFeatureNamespace<AzureIndexerFeature>("azindex");

            fullNodeBuilder.ConfigureFeature(features =>
            {
                features
                .AddFeature<AzureIndexerFeature>()
                .FeatureServices(services =>
                {
                    services.AddSingleton<AzureIndexerLoop>();
                    services.AddSingleton<AzureIndexerSettings>(new AzureIndexerSettings(setup));
                });
            });

            return fullNodeBuilder;
        }
    }
}