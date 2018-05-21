using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DBreeze.Exceptions;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Zorbit.Features.Observatory.Core.Extensions;
using Zorbit.Features.Observatory.Core.Model;
using Zorbit.Features.Observatory.Core.Node;

namespace Zorbit.Features.Observatory.Core.Indexing
{
    public interface IIndexer
    {
        IReadOnlyCollection<IIndexerTarget> Indexers { get; }
        ChainedBlock Tip { get; }
        Task IndexAsync(CancellationToken cancellationToken);
        Task Initialize(CancellationToken cancellationToken);
    }

    public interface IIndexerTarget
    {
        IndexType Type { get; set; }
        ChainedBlock Tip { get; set; }
        ICheckpoint Checkpoint { get; set; }
    }

    public sealed class IndexerTarget : IIndexerTarget
    {
        public IndexType Type { get; set; }
        public ChainedBlock Tip { get; set; }
        public ICheckpoint Checkpoint { get; set; }
    }

    public sealed class Indexer : IIndexer
    {
        private const int InitalBlockDownloadDelay = 5000;
        private const int IndexRetryInterval = 10000;

        private readonly IDictionary<IndexType, IIndexerTarget> _indexers = new ConcurrentDictionary<IndexType, IIndexerTarget>();

        private readonly INodeBlockFetcherFactory _blockFetcherFactory;
        private readonly IIndexerTaskFactory _taskFactory;
        private readonly ICheckpointStore _checkpointStore;
        private readonly ConcurrentChain _chain;
        private readonly IndexerSettings _settings;
        private readonly ILogger _logger;

        public ChainedBlock Tip { get; private set; }
        public IReadOnlyCollection<IIndexerTarget> Indexers { get; private set; }

        public Indexer(
            INodeBlockFetcherFactory blockFetcherFactory,
            IIndexerTaskFactory taskFactory,
            ICheckpointStore checkpointStore,
            ConcurrentChain chain,
            IndexerSettings settings,
            ILoggerFactory loggerFactor)
        {
            this._blockFetcherFactory = blockFetcherFactory;
            this._chain = chain;
            this._settings = settings;
            this._logger = loggerFactor.CreateLogger<Indexer>();
            this._taskFactory = taskFactory;
            this._checkpointStore = checkpointStore;
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            foreach (var type in Enum.GetValues(typeof(IndexType)).OfType<IndexType>())
            {
                var tip = await _checkpointStore.GetCheckpointAsync(type).ConfigureAwait(false);
                _indexers.Add(type, new IndexerTarget
                {
                    Type = type,
                    Checkpoint = tip,
                    Tip = _chain.FindFork(tip.BlockLocator)
                });
            }

            Indexers = new ReadOnlyCollection<IIndexerTarget>(_indexers.Values.ToList());

            var minHeight = _indexers.Values.Min(i => i.Tip.Height);
            Tip = _settings.IgnoreCheckpoints ? _chain.GetBlock(_settings.From) : _chain.GetBlock(minHeight);
        }

        public async Task IndexAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("()");

            var sw = new Stopwatch();
            sw.Start();

            while (Tip.Height < _settings.To && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_chain.Height == 0)
                    {
                        await Task.Delay(InitalBlockDownloadDelay, cancellationToken)
                            .ConfigureAwait(false);
                        continue;
                    }

                    var fetcher = _blockFetcherFactory.Create(Tip);
                    fetcher.FromHeight = Math.Max(Tip.Height + 1, _settings.From);
                    fetcher.ToHeight = Math.Min(Tip.Height + _settings.BatchSize, _settings.To);

                    if (fetcher.ToHeight <= fetcher.LastProcessed.Height)
                    {
                        return;
                    }

                    var blocks = fetcher.GetBlocks().ToList();

                    foreach (var indexer in Indexers)
                    {
                        var task = _taskFactory.CreateTask(indexer.Type);
                        await task.RollBackAsync(blocks).ConfigureAwait(false);
                        await task.IndexAsync(blocks).ConfigureAwait(false);
                        indexer.Tip = fetcher.LastProcessed;
                    }

                    Tip = fetcher.LastProcessed;

                    await SaveCheckpoints().ConfigureAwait(false);

                    sw.Stop();
                    _logger.LogTrace($"Index Time: {sw.Elapsed.Pretty()}");
                    sw.Restart();
                }
                catch (DBreezeException ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        throw ex;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);

                    await Task.Delay(TimeSpan.FromMilliseconds(IndexRetryInterval), cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            _logger.LogTrace("(-)");
        }

        private Task SaveCheckpoints()
        {
            var tasks = new List<Task<bool>>();

            foreach (var indexer in Indexers)
            {
                indexer.Checkpoint.BlockLocator = indexer.Tip.GetLocator();
                tasks.Add(_checkpointStore.SaveCheckpointAsync(indexer.Checkpoint));
            }

            return Task.WhenAll(tasks.ToArray());
        }
    }
}