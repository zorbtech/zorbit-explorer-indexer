﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using NBitcoin.OpenAsset;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Features.AzureIndexer.Utils;
using Stratis.Bitcoin.Features.AzureIndexer.Wallet;

namespace Stratis.Bitcoin.Features.AzureIndexer.Balance
{
    public class OrderedBalanceChange
    {
        private Script _scriptPubKey;
        private Money _amount;

        public static IEnumerable<OrderedBalanceChange> ExtractScriptBalances(uint256 txId, Transaction transaction, uint256 blockId, BlockHeader blockHeader, int height)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (txId == null)
            {
                txId = transaction.GetHash();
            }

            if (blockId == null && blockHeader != null)
            {
                blockId = blockHeader.GetHash();
            }

            var changeByScriptPubKey = new Dictionary<Script, OrderedBalanceChange>();
            uint i = 0;
            foreach (var input in transaction.Inputs)
            {
                if (transaction.IsCoinBase)
                {
                    i++;
                    break;
                }

                TxDestination signer = null;

                signer = input.ScriptSig.Length != 0 ?
                    input.ScriptSig.GetSigner() :
                    GetSigner(input.WitScript);

                if (signer != null)
                {
                    if (!changeByScriptPubKey.TryGetValue(signer.ScriptPubKey, out var entry))
                    {
                        entry = new OrderedBalanceChange(txId, signer.ScriptPubKey, blockId, blockHeader, height);
                        changeByScriptPubKey.Add(signer.ScriptPubKey, entry);
                    }

                    entry.SpentOutpoints.Add(input.PrevOut);
                    entry.SpentIndices.Add(i);
                }

                i++;
            }

            i = 0;
            var hasOpReturn = false;
            foreach (var output in transaction.Outputs)
            {
                var bytes = output.ScriptPubKey.ToBytes(true);
                if (bytes.Length != 0 && bytes[0] == (byte)OpcodeType.OP_RETURN)
                {
                    hasOpReturn = true;
                    i++;
                    continue;
                }

                if (!changeByScriptPubKey.TryGetValue(output.ScriptPubKey, out var entry))
                {
                    entry = new OrderedBalanceChange(txId, output.ScriptPubKey, blockId, blockHeader, height);
                    changeByScriptPubKey.Add(output.ScriptPubKey, entry);
                }

                entry.ReceivedCoins.Add(new Coin()
                {
                    Outpoint = new OutPoint(txId, i),
                    TxOut = output
                });
                i++;
            }

            foreach (var entity in changeByScriptPubKey)
            {
                entity.Value.HasOpReturn = hasOpReturn;
                entity.Value.IsCoinbase = transaction.IsCoinBase;
            }

            return changeByScriptPubKey.Values;
        }

        public static TxDestination GetSigner(WitScript witScript)
        {
            if (witScript == WitScript.Empty)
            {
                return null;
            }

            var parameters = PayToWitPubKeyHashTemplate.Instance.ExtractWitScriptParameters(witScript);
            if (parameters != null)
            {
                return parameters.PublicKey.WitHash;
            }

            return Script.FromBytesUnsafe(witScript.GetUnsafePush(witScript.PushCount - 1)).WitHash;
        }

        public static IEnumerable<OrderedBalanceChange> ExtractWalletBalances(
                                                                            uint256 txId,
                                                                            Transaction tx,
                                                                            uint256 blockId,
                                                                            BlockHeader blockHeader,
                                                                            int height,
                                                                            WalletRuleEntryCollection walletCollection)
        {
            var entitiesByWallet = new Dictionary<string, OrderedBalanceChange>();
            var scriptBalances = ExtractScriptBalances(txId, tx, blockId, blockHeader, height);
            foreach (var scriptBalance in scriptBalances)
            {
                foreach (var walletRuleEntry in walletCollection.GetRulesFor(scriptBalance.ScriptPubKey))
                {
                    if (!entitiesByWallet.TryGetValue(walletRuleEntry.WalletId, out var walletEntity))
                    {
                        walletEntity = new OrderedBalanceChange(walletRuleEntry.WalletId, scriptBalance);
                        entitiesByWallet.Add(walletRuleEntry.WalletId, walletEntity);
                    }

                    walletEntity.Merge(scriptBalance, walletRuleEntry.Rule);
                }
            }

            foreach (var b in entitiesByWallet.Values)
            {
                b.UpdateToScriptCoins();
            }

            return entitiesByWallet.Values;
        }

        public async Task<bool> EnsureSpentCoinsLoadedAsync(ITransactionRepository transactions)
        {
            if (SpentCoins != null)
                return true;

            var cleanSpent = false;
            var result = new CoinCollection();
            for (var i = 0; i < SpentOutpoints.Count; i++)
            {
                var outpoint = SpentOutpoints[i];
                if (outpoint.IsNull)
                    continue;
                var prev = await transactions.GetAsync(outpoint.Hash).ConfigureAwait(false);
                if (prev == null)
                    return false;

                var coin = new Coin(outpoint, prev.Outputs[SpentOutpoints[i].N]);
                if (coin.ScriptPubKey != GetScriptPubkey(i))
                {
                    cleanSpent = true;
                    SpentOutpoints[i] = null;
                }
                else
                    result.Add(coin);
            }

            if (cleanSpent)
            {
                var spentIndices = new List<uint>();
                var spentOutpoints = new List<OutPoint>();
                var matchedRules = new List<MatchedRule>();
                for (var i = 0; i < SpentOutpoints.Count; i++)
                {
                    if (SpentOutpoints[i] != null)
                    {
                        spentIndices.Add(SpentIndices[i]);
                        spentOutpoints.Add(SpentOutpoints[i]);
                        if (MatchedRules != null && MatchedRules.Count != 0)
                            matchedRules.Add(MatchedRules[i]);
                    }
                }
                SpentIndices = spentIndices;
                SpentOutpoints = spentOutpoints;
                MatchedRules = matchedRules;
            }

            SpentCoins = result;
            UpdateToScriptCoins();
            return true;
        }

        public void UpdateToScriptCoins()
        {
            foreach (var match in MatchedRules)
            {
                var scriptRule = match.Rule as ScriptRule;
                if (scriptRule != null && scriptRule.RedeemScript != null)
                {
                    if (match.MatchType == MatchLocation.Output)
                    {
                        var outpoint = new OutPoint(TransactionId, match.Index);
                        var coin = ReceivedCoins[outpoint] as Coin;
                        if (coin != null)
                        {
                            ReceivedCoins[outpoint] = coin.ToScriptCoin(scriptRule.RedeemScript);
                        }
                    }
                    else
                    {
                        if (SpentCoins == null)
                            continue;
                        var n = this.SpentIndices.IndexOf(match.Index);
                        var coin = SpentCoins[n] as Coin;
                        if (coin != null)
                        {
                            this.SpentCoins[n] = coin.ToScriptCoin(scriptRule.RedeemScript);
                        }
                    }
                }
            }
        }

        public OrderedBalanceChange()
        {
            SpentIndices = new List<uint>();
            SpentOutpoints = new List<OutPoint>();
            ReceivedCoins = new CoinCollection();
        }

        internal OrderedBalanceChange(DynamicTableEntity entity)
        {
            var splitted = entity.RowKey.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            Height = Helper.StringToHeight(splitted[1]);
            BalanceId = BalanceId.Parse(splitted[0]);

            var locator = BalanceLocator.Parse(string.Join("-", splitted.Skip(1).ToArray()), true);
            var confLocator = locator as ConfirmedBalanceLocator;
            if (confLocator != null)
            {
                Height = confLocator.Height;
                TransactionId = confLocator.TransactionId;
                BlockId = confLocator.BlockHash;
            }

            var unconfLocator = locator as UnconfirmedBalanceLocator;
            if (unconfLocator != null)
            {
                TransactionId = unconfLocator.TransactionId;
            }

            SeenUtc = entity.Properties["s"].DateTime.Value;

            SpentOutpoints = Helper.DeserializeList<OutPoint>(Helper.GetEntityProperty(entity, "a"));

            if (entity.Properties.ContainsKey("b0"))
                SpentCoins = new CoinCollection(Helper.DeserializeList<Spendable>(Helper.GetEntityProperty(entity, "b")).Select(s => new Coin()
                {
                    Outpoint = s.OutPoint,
                    TxOut = s.TxOut
                }).ToList());
            else if (SpentOutpoints.Count == 0)
                SpentCoins = new CoinCollection();

            SpentIndices = Helper.DeserializeList<IntCompactVarInt>(Helper.GetEntityProperty(entity, "ss")).Select(i => (uint)i.ToLong()).ToList();

            var receivedIndices = Helper.DeserializeList<IntCompactVarInt>(Helper.GetEntityProperty(entity, "c")).Select(i => (uint)i.ToLong()).ToList();
            var receivedTxOuts = Helper.DeserializeList<TxOut>(Helper.GetEntityProperty(entity, "d"));

            ReceivedCoins = new CoinCollection();
            for (var i = 0; i < receivedIndices.Count; i++)
            {
                ReceivedCoins.Add(new Coin()
                {
                    Outpoint = new OutPoint(TransactionId, receivedIndices[i]),
                    TxOut = receivedTxOuts[i]
                });
            }

            var flags = entity.Properties["e"].StringValue;
            HasOpReturn = flags[0] == 'o';
            IsCoinbase = flags[1] == 'o';

            MatchedRules = Helper.DeserializeObject<List<MatchedRule>>(entity.Properties["f"].StringValue).ToList();

            if (entity.Properties.ContainsKey("g"))
            {
                var ctx = new ColoredTransaction();
                ctx.FromBytes(entity.Properties["g"].BinaryValue);
                ColoredTransaction = ctx;
            }

            if (entity.Properties.ContainsKey("h"))
            {
                _scriptPubKey = new Script(entity.Properties["h"].BinaryValue);
            }

            var data = Helper.GetEntityProperty(entity, "cu");
            if (data != null)
                CustomData = Encoding.UTF8.GetString(data);
        }

        public ColoredTransaction ColoredTransaction
        {
            get;
            set;
        }

        public void UpdateToColoredCoins()
        {
            if (ColoredTransaction == null)
                throw new InvalidOperationException("Impossible to get colored coin if ColoredTransaction is unknown");
            UpdateToColoredCoins(SpentCoins, true);
            UpdateToColoredCoins(ReceivedCoins, false);
        }

        public void UpdateToUncoloredCoins()
        {
            if (SpentCoins != null)
            {
                UpdateToUncoloredCoins(SpentCoins);
                UpdateToUncoloredCoins(ReceivedCoins);
            }
        }

        public BalanceLocator CreateBalanceLocator()
        {
            if (Height == UnconfirmedBalanceLocator.UnconfHeight)
                return new UnconfirmedBalanceLocator(SeenUtc, TransactionId);
            else
                return new ConfirmedBalanceLocator(this);
        }

        public static IEnumerable<OrderedBalanceChange> ExtractScriptBalances(Transaction tx)
        {
            return ExtractScriptBalances(null, tx, null, null, 0);
        }

        public IEnumerable<WalletRule> GetMatchedRules(int index, MatchLocation matchType)
        {
            return MatchedRules.Where(r => r.Index == index && r.MatchType == matchType).Select(c => c.Rule);
        }

        public IEnumerable<WalletRule> GetMatchedRules(ICoin coin)
        {
            return GetMatchedRules(coin.Outpoint);
        }

        public IEnumerable<WalletRule> GetMatchedRules(OutPoint outPoint)
        {
            if (outPoint.Hash == TransactionId)
                return GetMatchedRules((int)outPoint.N, MatchLocation.Output);
            else
            {
                var index = SpentOutpoints.IndexOf(outPoint);
                if (index == -1)
                    return new WalletRule[0];
                return GetMatchedRules((int)SpentIndices[index], MatchLocation.Input);
            }
        }

        public async Task<bool> EnsureColoredTransactionLoadedAsync(IColoredTransactionRepository repository)
        {
            if (ColoredTransaction != null)
            {
                this.UpdateToColoredCoins();
                return true;
            }
            if (!(repository is CachedColoredTransactionRepository))
                repository = new CachedColoredTransactionRepository(repository);
            var tx = await repository.Transactions.GetAsync(TransactionId).ConfigureAwait(false);
            if (tx == null)
                return false;
            try
            {
                var color = await tx.GetColoredTransactionAsync(repository).ConfigureAwait(false);
                if (color == null)
                    return false;
                ColoredTransaction = color;
                this.UpdateToColoredCoins();
                return true;
            }
            catch (TransactionNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Get the quantity of asset in this balance change
        /// </summary>
        /// <param name="assetId">The asset id, if null, returns uncolored satoshi</param>
        /// <returns></returns>
        public IMoney GetAssetAmount(BitcoinAssetId assetId)
        {
            return assetId == null ? Amount : GetAssetAmount(assetId.AssetId);
        }

        /// <summary>
        /// Get the quantity of asset in this balance change
        /// </summary>
        /// <param name="assetId">The asset id, if null, returns uncolored satoshi</param>
        /// <returns></returns>
        public IMoney GetAssetAmount(AssetId assetId)
        {
            if (assetId == null)
                return Amount;
            var amount = ReceivedCoins.WhereColored(assetId)
                .Select(c => c.Amount).Sum(assetId) - SpentCoins.WhereColored(assetId).Select(c => c.Amount).Sum(assetId);
            return amount;
        }

        internal DynamicTableEntity ToEntity()
        {
            var entity = new DynamicTableEntity();
            entity.ETag = "*";
            entity.PartitionKey = PartitionKey;

            var locator = CreateBalanceLocator();
            entity.RowKey = $"{BalanceId}-{locator.ToString(true)}";

            entity.Properties.Add("s", new EntityProperty(SeenUtc));
            Helper.SetEntityProperty(entity, "ss", Helper.SerializeList(SpentIndices.Select(e => new IntCompactVarInt(e))));

            Helper.SetEntityProperty(entity, "a", Helper.SerializeList(SpentOutpoints));
            if (SpentCoins != null)
                Helper.SetEntityProperty(entity, "b", Helper.SerializeList(SpentCoins.Select(c => new Spendable(c.Outpoint, c.TxOut))));
            Helper.SetEntityProperty(entity, "c", Helper.SerializeList(ReceivedCoins.Select(e => new IntCompactVarInt(e.Outpoint.N))));
            Helper.SetEntityProperty(entity, "d", Helper.SerializeList(ReceivedCoins.Select(e => e.TxOut)));
            var flags = (HasOpReturn ? "o" : "n") + (IsCoinbase ? "o" : "n");
            entity.Properties.AddOrReplace("e", new EntityProperty(flags));
            entity.Properties.AddOrReplace("f", new EntityProperty(Helper.Serialize(MatchedRules)));
            if (ColoredTransaction != null)
            {
                entity.Properties.AddOrReplace("g", new EntityProperty(ColoredTransaction.ToBytes()));
            }
            if (ScriptPubKey != null && !BalanceId.ContainsScript)
            {
                var bytes = ScriptPubKey.ToBytes(true);
                if (bytes.Length < 63000)
                    entity.Properties.Add("h", new EntityProperty(bytes));
            }
            if (CustomData != null)
            {
                Helper.SetEntityProperty(entity, "cu", Encoding.UTF8.GetBytes(CustomData));
            }
            return entity;
        }

        internal OrderedBalanceChange(uint256 txId, string walletId, Script scriptPubKey, uint256 blockId, BlockHeader blockHeader, int height)
            : this()
        {
            Init(txId, new BalanceId(walletId), blockId, blockHeader, height);
            _scriptPubKey = scriptPubKey;
        }

        internal OrderedBalanceChange(string walletId, OrderedBalanceChange source)
            : this(source.TransactionId, walletId, source.ScriptPubKey, source.BlockId, null, source.Height)
        {
            SeenUtc = source.SeenUtc;
            IsCoinbase = source.IsCoinbase;
            HasOpReturn = source.HasOpReturn;
        }

        internal class IntCompactVarInt : CompactVarInt
        {
            public IntCompactVarInt(uint value)
                : base(value, 4)
            {
            }
            public IntCompactVarInt()
                : base(4)
            {

            }
        }

        internal OrderedBalanceChange(uint256 txId, Script scriptPubKey, uint256 blockId, BlockHeader blockHeader, int height)
            : this()
        {
            var balanceId = new BalanceId(scriptPubKey);
            Init(txId, balanceId, blockId, blockHeader, height);
            if (!balanceId.ContainsScript)
            {
                _scriptPubKey = scriptPubKey;
            }
        }
        internal Task<bool> EnsureSpentCoinsLoadedAsync(uint256[] parentIds, Transaction[] transactions)
        {
            var repo = new NoSqlTransactionRepository();
            for (var i = 0; i < parentIds.Length; i++)
            {
                if (transactions[i] == null)
                    return Task.FromResult(false);
                repo.Put(parentIds[i], transactions[i]);
            }
            return EnsureSpentCoinsLoadedAsync(repo);
        }

        internal void Merge(OrderedBalanceChange other, WalletRule walletRule)
        {
            if (other.ReceivedCoins.Count != 0)
            {
                ReceivedCoins.AddRange(other.ReceivedCoins);
                ReceivedCoins = new CoinCollection(ReceivedCoins.Distinct<ICoin, OutPoint>(c => c.Outpoint));
                if (walletRule != null)
                    foreach (var c in other.ReceivedCoins)
                    {
                        this.MatchedRules.Add(new MatchedRule()
                        {
                            Index = c.Outpoint.N,
                            Rule = walletRule,
                            MatchType = MatchLocation.Output
                        });
                    }
            }

            if (other.SpentIndices.Count != 0)
            {
                SpentIndices.AddRange(other.SpentIndices);
                SpentIndices = SpentIndices.Distinct().ToList();

                SpentOutpoints.AddRange(other.SpentOutpoints);
                SpentOutpoints = SpentOutpoints.Distinct().ToList();

                //Remove cached value, no longer correct
                UpdateToUncoloredCoins();
                SpentCoins = null;

                if (walletRule != null)
                    foreach (var c in other.SpentIndices)
                    {
                        this.MatchedRules.Add(new MatchedRule()
                        {
                            Index = c,
                            Rule = walletRule,
                            MatchType = MatchLocation.Input
                        });
                    }
            }
        }

        private static void UpdateToUncoloredCoins(CoinCollection collection)
        {
            if (collection == null)
            {
                return;
            }

            for (var i = 0; i < collection.Count; i++)
            {
                var coin = collection[i] as ColoredCoin;
                if (coin != null)
                    collection[i] = coin.Bearer;
            }
        }

        private Script GetScriptPubkey(int i)
        {
            if (this.MatchedRules.Count == 0)
                return ScriptPubKey;
            return ((ScriptRule)(this.MatchedRules.First(r => r.MatchType == MatchLocation.Input && r.Index == SpentIndices[i]).Rule)).ScriptPubKey;
        }

        private void Init(uint256 txId, BalanceId balanceId, uint256 blockId, BlockHeader blockHeader, int height)
        {
            BlockId = blockId;
            SeenUtc = blockHeader?.BlockTime.UtcDateTime ?? DateTime.UtcNow;
            Height = blockId == null ? UnconfirmedBalanceLocator.UnconfHeight : height;
            TransactionId = txId;
            BalanceId = balanceId;
        }

        private void UpdateToColoredCoins(CoinCollection collection, bool input)
        {
            if (collection == null)
                return;
            for (var i = 0; i < collection.Count; i++)
            {
                var coin = collection[i] as Coin;
                if (coin != null)
                {
                    if (input)
                    {
                        var txinIndex = SpentIndices[i];
                        var asset = ColoredTransaction
                            .Inputs
                            .Where(_ => _.Index == (uint)txinIndex)
                            .Select(_ => _.Asset)
                            .FirstOrDefault();
                        if (asset != null)
                            collection[i] = coin.ToColoredCoin(asset);
                    }
                    else
                    {
                        var asset = ColoredTransaction.GetColoredEntry(coin.Outpoint.N);
                        if (asset != null)
                            collection[i] = coin.ToColoredCoin(asset.Asset);
                    }
                }
            }
        }

        internal Script ScriptPubKey
        {
            get
            {
                if (_scriptPubKey == null)
                    _scriptPubKey = BalanceId.ExtractScript();
                return _scriptPubKey;
            }
        }

        internal bool IsEmpty => SpentCoins.Count == 0 && ReceivedCoins.Count == 0;

        public List<MatchedRule> MatchedRules { get; internal set; } = new List<MatchedRule>();

        public bool MempoolEntry => BlockId == null;

        public string CustomData { get; set; }

        public BalanceId BalanceId { get; internal set; }

        public string PartitionKey => BalanceId.PartitionKey;

        public int Height { get; set; }

        public uint256 BlockId { get; set; }

        public uint256 TransactionId { get; set; }

        public bool HasOpReturn { get; set; }

        public bool IsCoinbase { get; set; }

        public DateTime SeenUtc { get; set; }

        public List<uint> SpentIndices { get; private set; }

        public List<OutPoint> SpentOutpoints { get; private set; }

        public CoinCollection ReceivedCoins { get; private set; }

        /// <summary>
        /// Might be null if parent transactions have not yet been indexed
        /// </summary>
        public CoinCollection SpentCoins { get; internal set; }

        public Money Amount
        {
            get
            {
                if (_amount == null && SpentCoins != null)
                {
                    _amount = ReceivedCoins.WhereUncolored().Select(c => c.Amount).Sum() - SpentCoins.WhereUncolored().Select(c => c.Amount).Sum();
                }
                return _amount;
            }
        }
    }
}
