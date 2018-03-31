﻿using NBitcoin;
using Newtonsoft.Json;
using Stratis.Bitcoin.Features.AzureIndexer.Wallet;

namespace Stratis.Bitcoin.Features.AzureIndexer
{
    public class ScriptRule : WalletRule
    {
        public ScriptRule()
        {
        }

        public ScriptRule(Script destination, Script redeemScript = null)
        {
            this.ScriptPubKey = destination;
            this.RedeemScript = redeemScript;
        }

        public ScriptRule(IDestination destination, Script redeemScript = null)
            : this(destination.ScriptPubKey, redeemScript)
        {
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Script ScriptPubKey { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Script RedeemScript { get; set; }

        public override string Id => this.ScriptPubKey.Hash.ToString();
    }
}
