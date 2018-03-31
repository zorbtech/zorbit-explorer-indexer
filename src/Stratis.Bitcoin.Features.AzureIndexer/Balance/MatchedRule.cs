
using Stratis.Bitcoin.Features.AzureIndexer.Wallet;

namespace Stratis.Bitcoin.Features.AzureIndexer.Balance
{
    public enum MatchLocation
    {
        Output,
        Input,
    }
    public class MatchedRule
    {
        public uint Index
        {
            get;
            set;
        }

        public WalletRule Rule
        {
            get;
            set;
        }

        public MatchLocation MatchType
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"{this.Index}-{this.MatchType}";
        }
    }
}
