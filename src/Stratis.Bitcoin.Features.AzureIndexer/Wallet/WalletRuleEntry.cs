using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin.DataEncoders;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing;

namespace Stratis.Bitcoin.Features.AzureIndexer.Wallet
{
    public class WalletRuleEntry
    {
        public WalletRuleEntry()
        {
        }

        public WalletRuleEntry(DynamicTableEntity entity, IndexerClient client)
        {
            WalletId = Encoding.UTF8.GetString(Encoders.Hex.DecodeData(entity.PartitionKey));

            Rule = Helper.DeserializeObject<WalletRule>(!entity.Properties.ContainsKey("a0") ?
                Encoding.UTF8.GetString(Encoders.Hex.DecodeData(entity.RowKey)) :
                Encoding.UTF8.GetString(Helper.GetEntityProperty(entity, "a")));
        }

        public WalletRuleEntry(string walletId, WalletRule rule)
        {
            WalletId = walletId;
            Rule = rule;
        }

        public DynamicTableEntity CreateTableEntity()
        {
            var entity = new DynamicTableEntity
            {
                ETag = "*",
                PartitionKey = Encoders.Hex.EncodeData(Encoding.UTF8.GetBytes(WalletId))
            };

            if (Rule == null)
            {
                return entity;
            }

            entity.RowKey = Rule.Id;
            Helper.SetEntityProperty(entity, "a", Encoding.UTF8.GetBytes(Helper.Serialize(Rule)));
            return entity;
        }

        public string WalletId { get; set; }

        public WalletRule Rule { get; set; }

    }
}
