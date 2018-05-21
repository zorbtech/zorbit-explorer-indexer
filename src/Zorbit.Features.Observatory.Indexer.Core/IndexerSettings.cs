using System;
using System.Text;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Configuration;

namespace Zorbit.Features.Observatory.Core
{
    /// <summary>
    /// StorageClient related to Azure Indexer feature.
    /// </summary>
    public class IndexerSettings
    {
        /// <summary>Azure storage account.</summary>
        public string AzureConnectionString { get; set; }

        /// <summary>Azure storage emulator used.</summary>
        public bool AzureEmulatorUsed { get; set; }

        /// <summary>Checkpoint interval determines how often to record checkpoints.</summary>
        public TimeSpan CheckpointInterval { get; set; }

        /// <summary>Determines whether to regard or update checkpoints.</summary>
        public bool IgnoreCheckpoints { get; set; }

        /// <summary>The block to start indexing from.</summary>
        public int From { get; set; }

        /// <summary>The last block to index.</summary>
        public int To { get; set; }

        /// <summary>The storage namespace to use.</summary>
        public string StorageNamespace { get; set; }

        /// <summary>Option to reset Azure storage on init.</summary>
        public bool ResetStorage { get; set; }

        /// <summary>Batch size to index.</summary>
        public int BatchSize { get; set; }

        /// <summary>Maximum tasks that can be run in parallel.</summary>
        public int TaskCount { get; set; }

        /// <summary>The callback used to modify settings on startup.</summary>
        private readonly Action<IndexerSettings> _callback = null;

        /// <summary>
        /// Initializes an instance of the object.
        /// </summary>
        public IndexerSettings()
        {
            this.AzureEmulatorUsed = true;
            this.From = 0;
            this.To = int.MaxValue;
            this.StorageNamespace = "";
            this.CheckpointInterval = TimeSpan.Parse("00:15:00");
            this.ResetStorage = false;
            this.BatchSize = 1000;
            this.TaskCount = 200;
        }

        /// <summary>
        /// Initializes an instance of the object.
        /// </summary>
        /// <param name="callback">A callback for modifying the settings during startup.</param>
        public IndexerSettings(Action<IndexerSettings> callback) : this()
        {
            this._callback = callback;
        }

        /// <summary>
        /// Loads the Azure Indexer settings from the application storageClient.
        /// </summary>
        /// <param name="nodeSettings">Application storageClient.</param>
        private void LoadSettingsFromConfig(NodeSettings nodeSettings)
        {
            var config = nodeSettings.ConfigReader;
            this.AzureEmulatorUsed = config.GetOrDefault<bool>("azemu", false);
            if (!this.AzureEmulatorUsed)
            {
                this.AzureConnectionString = config.GetOrDefault<string>("azureconnectionstring", string.Empty);
            }
            this.CheckpointInterval = TimeSpan.Parse(config.GetOrDefault<string>("chkptint", "00:15:00"));
            this.IgnoreCheckpoints = config.GetOrDefault<bool>("nochkpts", false);
            this.From = int.Parse(config.GetOrDefault<string>("indexfrom", "0"));
            this.To = int.Parse(config.GetOrDefault<string>("indexto", int.MaxValue.ToString()));
            this.StorageNamespace = config.GetOrDefault<string>("indexprefix", string.Empty);
            this.ResetStorage = config.GetOrDefault<bool>("resetstorage", false);
            this.BatchSize = config.GetOrDefault<int>("batchsize", 1000);
            this.TaskCount = config.GetOrDefault<int>("taskcount", 200);
        }

        /// <summary>
        /// Loads the Azure Indexer settings from the application storageClient.
        /// Allows the callback to override those settings.
        /// </summary>
        /// <param name="nodeSettings">Application storageClient.</param>
        public void Load(NodeSettings nodeSettings)
        {
            // Get values from config
            this.LoadSettingsFromConfig(nodeSettings);

            // Invoke callback
            this._callback?.Invoke(this);
        }

        /// <summary>
        /// Prints command line help.
        /// </summary>
        /// <param name="mainNet">Used for network-specific help (if any).</param>
        public static void PrintHelp(Network mainNet)
        {
            var defaults = NodeSettings.Default();
            var builder = new StringBuilder();

            builder.AppendLine($"-azureconnectionstring=<string>        Azure connection string.");
            builder.AppendLine($"-azemu                                 Azure storage emulator used. Default is not to use the emulator.");
            builder.AppendLine($"-chkptint=<hh:mm:ss>                   Indexing checkpoint interval.");
            builder.AppendLine($"-nochkpts                              Do not use checkpoints. Default is to use checkpoints.");
            builder.AppendLine($"-indexfrom=<int (0 to N)>              Block height to start indexing from.");
            builder.AppendLine($"-indexto=<int (0 to N)>                Maximum block height to index.");
            builder.AppendLine($"-indexprefix=<string>                  Name prefix for index tables and blob container.");
            builder.AppendLine($"-resetsorage                           Reset Azure storage on init.");
            builder.AppendLine($"-batchsize                             The size of the batch index.");

            defaults.Logger.LogInformation(builder.ToString());
        }
    }
}