﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    public class IndexerTrace
    {
        private static ILogger _logger = NullLogger.Instance;
		public static void Configure(ILoggerFactory factory)
		{
			_logger = factory.CreateLogger("Stratis.Bitcoin.Features.AzureIndexer");
		}

        internal static void ErrorWhileImportingBlockToAzure(uint256 id, Exception ex)
        {
			_logger.LogError(ex, $"Error while importing {id} in azure blob");
        }


        internal static void BlockAlreadyUploaded()
        {
			_logger.LogDebug("Block already uploaded");
        }

        internal static void BlockUploaded(TimeSpan time, int bytes)
        {
            if (time.TotalSeconds == 0.0)
                time = TimeSpan.FromMilliseconds(10);
            var speed = ((double)bytes / 1024.0) / time.TotalSeconds;
			_logger.LogDebug($"Block uploaded successfully ({speed:0.00} KB/S)");
        }

        internal static IDisposable NewCorrelation(string activityName)
        {
			return _logger.BeginScope(activityName);
        }

        internal static void CheckpointLoaded(ChainedBlock block, string checkpointName)
        {
			_logger.LogInformation($"Checkpoint {checkpointName} loaded at {ToString(block)}");
        }

        internal static void CheckpointSaved(ChainedBlock block, string checkpointName)
        {
			_logger.LogInformation($"Checkpoint {checkpointName} saved at {ToString(block)}");
        }


        internal static void ErrorWhileImportingEntitiesToAzure(ITableEntity[] entities, Exception ex)
        {
            var builder = new StringBuilder();
            var i = 0;
            foreach (var entity in entities)
            {
                builder.AppendLine($"[{i}] {entity.RowKey}");
                i++;
            }
            _logger.LogError(ex, $"Error while importing entities (len:{entities.Length})\r\n{builder}");
        }

        internal static void RetryWorked()
        {
            _logger.LogInformation("Retry worked");
        }

        public static string Pretty(TimeSpan span)
        {
            if (span == TimeSpan.Zero)
                return "0m";

            var sb = new StringBuilder();
            if (span.Days > 0)
                sb.AppendFormat("{0}d ", span.Days);
            if (span.Hours > 0)
                sb.AppendFormat("{0}h ", span.Hours);
            if (span.Minutes > 0)
                sb.AppendFormat("{0}m", span.Minutes);
            var result = sb.ToString();
            if (result == string.Empty)
                return "< 1min";
            return result;
        }

        internal static void TaskCount(int count)
        {
			_logger.LogInformation($"Upload thread count : {count}");
        }

        internal static void ErrorWhileImportingBalancesToAzure(Exception ex, uint256 txid)
        {
			_logger.LogError(ex, $"Error while importing balances on {txid}");
        }

        internal static void MissingTransactionFromDatabase(uint256 txid)
        {
			_logger.LogError($"Missing transaction from index while fetching outputs {txid}");
        }


        internal static void InputChainTip(ChainedBlock block)
        {
            _logger.LogInformation($"The input chain tip is at height {ToString(block)}");
        }

        private static string ToString(uint256 blockId, int height)
        {
            return height.ToString();
        }

        internal static void IndexedChainTip(uint256 blockId, int height)
        {
			_logger.LogInformation($"Indexed chain is at height {ToString(blockId, height)}");
        }

        internal static void InputChainIsLate()
        {
			_logger.LogInformation("The input chain is late compared to the indexed one");
        }

        public static void IndexingChain(ChainedBlock from, ChainedBlock to)
        {
			_logger.LogInformation($"Indexing blocks from {ToString(@from)} to {ToString(to)} (both included)");
        }

        private static string ToString(ChainedBlock chainedBlock)
        {
            return chainedBlock == null ? "(null)" : ToString(chainedBlock.HashBlock, chainedBlock.Height);
        }

        internal static void RemainingBlockChain(int height, int maxHeight)
        {
            var remaining = height - maxHeight;
            if (remaining % 1000 == 0 && remaining != 0)
            {
				_logger.LogInformation($"Remaining chain block to index : {remaining} ({height}/{maxHeight})");
            }
        }

        internal static void IndexedChainIsUpToDate(ChainedBlock block)
        {
			_logger.LogInformation($"Indexed chain is up to date at height {ToString(block)}");
        }

        public static void Information(string message)
        {
			_logger.LogInformation(message);
        }

        internal static void NoForkFoundWithStored()
        {
			_logger.LogInformation("No fork found with the stored chain");
        }

        public static void Processed(int height, int totalHeight, Queue<DateTime> lastLogs, Queue<int> lastHeights)
        {
            var lastLog = lastLogs.LastOrDefault();
            if (DateTime.UtcNow - lastLog > TimeSpan.FromSeconds(10))
            {
                if (lastHeights.Count > 0)
                {
                    var lastHeight = lastHeights.Peek();
                    var time = DateTimeOffset.UtcNow - lastLogs.Peek();

                    var downloadedSize = GetSize(lastHeight, height);
                    var remainingSize = GetSize(height, totalHeight);
                    var estimatedTime = downloadedSize < 1.0m ? TimeSpan.FromDays(999.0)
                        : TimeSpan.FromTicks((long)((remainingSize / downloadedSize) * time.Ticks));
					_logger.LogInformation("Blocks {0}/{1} (estimate : {2})", height, totalHeight, Pretty(estimatedTime));
                }
                lastLogs.Enqueue(DateTime.UtcNow);
                lastHeights.Enqueue(height);
                while (lastLogs.Count > 20)
                {
                    lastLogs.Dequeue();
                    lastHeights.Dequeue();
                }
            }
        }

        private static decimal GetSize(int t1, int t2)
        {
            var cumul = 0.0m;
            for (var i = t1 ; i < t2 ; i++)
            {
				var size = EstimateSize(i);
                cumul += (decimal)size;
            }
            return cumul;
        }

        private static readonly int OneMbHeight = 390000;

		private static decimal EstimateSize(decimal height)
        {
			if(height > OneMbHeight)
				return 1.0m;
            return (decimal)Math.Exp((double)(A * height + B));
        }

        private static readonly decimal A = 0.0000221438236661323m;
        private static readonly decimal B = -8.492328726823666132321613096m;

    }
}