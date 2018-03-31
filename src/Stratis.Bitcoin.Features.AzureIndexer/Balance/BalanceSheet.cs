using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;

namespace Stratis.Bitcoin.Features.AzureIndexer.Balance
{
    public class BalanceSheet
    {
        private readonly ChainBase _chain;

        public ChainBase Chain
        {
            get
            {
                return _chain;
            }
        }

        public BalanceSheet(IEnumerable<OrderedBalanceChange> changes, ChainBase chain)
        {
            if (chain == null)
                throw new ArgumentNullException("chain");
            _chain = chain;

            var all = changes
                        .Where(c => c.SpentCoins != null) //Remove line whose previous coins have not been loadedcould not be deduced
                        .Where(c => c.MempoolEntry || chain.GetBlock(c.BlockId) != null) //Take only mempool entry, or confirmed one
                        .Where(c => !(c.IsCoinbase && c.MempoolEntry)) //There is no such thing as a Coinbase unconfirmed, by definition a coinbase appear in a block
                        .ToList(); 
            var confirmed = all.Where(o => o.BlockId != null).ToDictionary(o => o.TransactionId);
            var unconfirmed = new Dictionary<uint256, OrderedBalanceChange>();

            foreach(var item in all.Where(o => o.MempoolEntry && !confirmed.ContainsKey(o.TransactionId)))
            {
                unconfirmed.AddOrReplace(item.TransactionId, item);
            }

            _prunable = all.Where(o => o.BlockId == null && confirmed.ContainsKey(o.TransactionId)).ToList();
            _all = all.Where(o => 
                (unconfirmed.ContainsKey(o.TransactionId) || confirmed.ContainsKey(o.TransactionId)) 
                    &&
                    !(o.BlockId == null && confirmed.ContainsKey(o.TransactionId))
                ).ToList();
            _confirmed = _all.Where(o => o.BlockId != null && confirmed.ContainsKey(o.TransactionId)).ToList();
            _unconfirmed = _all.Where(o => o.BlockId == null && unconfirmed.ContainsKey(o.TransactionId)).ToList();
        }

        private readonly List<OrderedBalanceChange> _unconfirmed;
        public List<OrderedBalanceChange> Unconfirmed
        {
            get
            {
                return _unconfirmed;
            }
        }
        private readonly List<OrderedBalanceChange> _confirmed;
        public List<OrderedBalanceChange> Confirmed
        {
            get
            {
                return _confirmed;
            }
        }

        private readonly List<OrderedBalanceChange> _all;
        public List<OrderedBalanceChange> All
        {
            get
            {
                return _all;
            }
        }
        private readonly List<OrderedBalanceChange> _prunable;
        public List<OrderedBalanceChange> Prunable
        {
            get
            {
                return _prunable;
            }
        }

    }
}
