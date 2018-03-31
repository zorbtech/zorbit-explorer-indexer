using System;
using System.Collections.Generic;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks
{
    public class BulkImport<T>
    {
        private readonly Dictionary<string, Queue<T>> _currentPartitions = new Dictionary<string, Queue<T>>();

        public BulkImport(int partitionSize)
        {
            this.PartitionSize = partitionSize;
        }

        public void Add(string partitionName, T item)
        {
            var partition = GetPartition(partitionName);
            partition.Enqueue(item);

            if (partition.Count < PartitionSize)
            {
                return;
            }

            var fullPartition = new T[PartitionSize];
            for (var i = 0; i < PartitionSize; i++)
            {
                fullPartition[i] = partition.Dequeue();
            }

            ReadyPartitions.Enqueue(Tuple.Create(partitionName, fullPartition));
        }

        public void FlushUncompletePartitions()
        {
            foreach (var partition in _currentPartitions)
            {
                while (partition.Value.Count != 0)
                {
                    var fullPartition = new T[Math.Min(PartitionSize, partition.Value.Count)];
                    for (var i = 0; i < fullPartition.Length; i++)
                    {
                        fullPartition[i] = partition.Value.Dequeue();
                    }
                    ReadyPartitions.Enqueue(Tuple.Create(partition.Key, fullPartition));
                }
            }
        }

        private Queue<T> GetPartition(string partition)
        {
            if (_currentPartitions.TryGetValue(partition, out var result))
            {
                return result;
            }

            result = new Queue<T>();
            _currentPartitions.Add(partition, result);
            return result;
        }

        internal Queue<Tuple<string, T[]>> ReadyPartitions => new Queue<Tuple<string, T[]>>();

        public int PartitionSize { get; set; }

        public bool HasFullPartition => ReadyPartitions.Count > 0;
    }
}
