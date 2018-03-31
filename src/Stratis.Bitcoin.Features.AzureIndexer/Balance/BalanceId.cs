using System;
using System.Text;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Balance.DamienG.Security.Cryptography;

namespace Stratis.Bitcoin.Features.AzureIndexer.Balance
{
    public enum BalanceType
    {
        Wallet,
        Address
    }

    public class BalanceId
    {
        internal const int MaxScriptSize = 512;

        private const string WalletPrefix = "w$";
        private const string HashPrefix = "h$";

        private string _partitionKey;
        private string _internal;

        private BalanceId()
        {
        }

        public BalanceId(string walletId)
        {
            _internal = WalletPrefix + FastEncoder.Instance.EncodeData(Encoding.UTF8.GetBytes(walletId));
        }

        public BalanceId(Script scriptPubKey)
        {
            var pubKey = scriptPubKey.ToBytes(true);
            if (pubKey.Length > MaxScriptSize)
                _internal = HashPrefix + FastEncoder.Instance.EncodeData(scriptPubKey.Hash.ToBytes(true));
            else
                _internal = FastEncoder.Instance.EncodeData(scriptPubKey.ToBytes(true));
        }

        public BalanceId(IDestination destination)
            : this(destination.ScriptPubKey)
        {
        }

        public static BalanceId Parse(string balanceId)
        {
            return new BalanceId()
            {
                _internal = balanceId
            };
        }
        
        public string GetWalletId()
        {
            if (!_internal.StartsWith(WalletPrefix))
                throw new InvalidOperationException("This balance id does not represent a wallet");
            return Encoding.UTF8.GetString(FastEncoder.Instance.DecodeData(_internal.Substring(WalletPrefix.Length)));
        }

        public Script ExtractScript()
        {
            return !ContainsScript ? null : Script.FromBytesUnsafe(FastEncoder.Instance.DecodeData(_internal));
        }

        public override string ToString()
        {
            return _internal;
        }

        public bool ContainsScript => _internal.Length >= 2 && _internal[1] != '$';

        public BalanceType Type => _internal.StartsWith(WalletPrefix) ? BalanceType.Wallet : BalanceType.Address;

        public string PartitionKey => _partitionKey ?? (_partitionKey = Helper.GetPartitionKey(10, Crc32.Compute(_internal)));

    }
}
