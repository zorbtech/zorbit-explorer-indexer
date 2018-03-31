using Newtonsoft.Json;

namespace Stratis.Bitcoin.Features.AzureIndexer.Wallet
{
    public abstract class WalletRule
    {
        public override string ToString()
        {
            return Helper.Serialize(this);
        }

        [JsonIgnore]
        public abstract string Id { get; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CustomData { get; set; }
    }
}
