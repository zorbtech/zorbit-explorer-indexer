using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;

namespace Stratis.Bitcoin.Features.AzureIndexer.Tests
{
    public class ChainBuilder
    {
        private readonly IndexerTester _tester;
        private readonly ConcurrentChain _chain = new ConcurrentChain(Network.TestNet);

        public ConcurrentChain Chain
        {
            get
            {
                return _chain;
            }
        }
        public ChainBuilder(IndexerTester indexerTester)
        {
            this._tester = indexerTester;
            var genesis = indexerTester.Indexer.Configuration.Network.GetGenesis();
            _blocks.Add(genesis.GetHash(), genesis);
        }

        private Block _current;

        public Block GetCurrentBlock()
        {
            var b = _current = _current ?? CreateNewBlock();
            _chain.SetTip(b.Header);
            return b;
        }

        public bool NoRandom
        {
            get;
            set;
        }

        public Transaction EmitMoney(IDestination destination, Money amount, bool isCoinbase = true, bool indexBalance = false)
        {
            var transaction = new Transaction();
            if (isCoinbase)
                transaction.AddInput(new TxIn()
                {
                    ScriptSig = new Script(NoRandom ? new uint256(0).ToBytes() : RandomUtils.GetBytes(32)),
                });
            transaction.AddOutput(new TxOut()
            {
                ScriptPubKey = destination.ScriptPubKey,
                Value = amount
            });
            Add(transaction, indexBalance);
            return transaction;
        }

        private void Add(Transaction tx, bool indexBalances)
        {
            var b = GetCurrentBlock();
            b.Transactions.Add(tx);
            if (!tx.IsCoinBase)
            {
                _tester.Indexer.Index(new TransactionEntry.Entity(null, tx, null));
                if (indexBalances)
                    _tester.Indexer.IndexOrderedBalance(tx);
            }
        }

        private uint _nonce = 0;
        private Block CreateNewBlock()
        {
            var b = new Block();
            b.Header.Nonce = _nonce;
            _nonce++;
            b.Header.HashPrevBlock = _chain.Tip.HashBlock;
            b.Header.BlockTime = NoRandom ? new DateTimeOffset(1988, 07, 18, 0, 0, 0, TimeSpan.Zero) + TimeSpan.FromHours(_nonce) : DateTimeOffset.UtcNow;
            return b;
        }

        public Block SubmitBlock()
        {
            var b = GetCurrentBlock();
            b.UpdateMerkleRoot();
            _chain.SetTip(b.Header);
            _current = null;
            _unsyncBlocks.Add(b);
            _blocks.Add(b.Header.GetHash(), b);
            _mempool.Clear();
            return b;
        }

        private readonly List<Block> _unsyncBlocks = new List<Block>();
        public void SyncIndexer()
        {
            _tester.Indexer.IndexChain(_chain);
            var walletRules = _tester.Client.GetAllWalletRules();
            foreach (var b in _unsyncBlocks)
            {
                var height = _chain.GetBlock(b.GetHash()).Height;
                _tester.Indexer.IndexOrderedBalance(height, b);
                foreach (var tx in b.Transactions)
                {
                    _tester.Indexer.Index(new[] { new TransactionEntry.Entity(tx.GetHash(), tx, b.GetHash()) });
                }
                if (walletRules.Count() != 0)
                {
                    _tester.Indexer.IndexWalletOrderedBalance(height, b, walletRules);
                }
            }
            _unsyncBlocks.Clear();
        }

        public Transaction Emit(Transaction transaction, bool indexBalance = false)
        {
            Add(transaction, indexBalance);
            _mempool.Add(transaction.GetHash(), transaction);
            return transaction;
        }

        public Block Generate(int count = 1)
        {
            Block b = null;
            for (var i = 0 ; i < count ; i++)
                b = SubmitBlock();
            return b;
        }


        public void Emit(IEnumerable<Transaction> transactions)
        {
            foreach (var tx in transactions)
                Emit(tx);
        }

        private readonly Dictionary<uint256, Block> _blocks = new Dictionary<uint256, Block>();
        public Dictionary<uint256, Block> Blocks
        {
            get
            {
                return _blocks;
            }
        }

        private readonly Dictionary<uint256, Transaction> _mempool = new Dictionary<uint256, Transaction>();
        public Dictionary<uint256, Transaction> Mempool
        {
            get
            {
                return _mempool;
            }
        }

        public void Load(string blockFolder)
        {
            var store = new NBitcoin.BitcoinCore.BlockStore(blockFolder, this._tester.Client.Configuration.Network);
            foreach (var block in store.Enumerate(false))
            {
                SubmitBlock(block.Item);
            }
        }

        public void SubmitBlock(Block block)
        {
            if (!Blocks.ContainsKey(block.GetHash()))
            {
                _current = block;
                SubmitBlock();
            }
        }

    }
}
