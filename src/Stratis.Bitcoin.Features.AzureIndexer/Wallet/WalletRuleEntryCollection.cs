﻿using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;

namespace Stratis.Bitcoin.Features.AzureIndexer.Wallet
{
    public class WalletRuleEntryCollection : IEnumerable<WalletRuleEntry>
    {
        private readonly List<WalletRuleEntry> _walletRules;
        private readonly HashSet<Tuple<string, string>> _walletsIds = new HashSet<Tuple<string, string>>();

        private readonly MultiValueDictionary<string, WalletRuleEntry> _entriesByWallet;
        private readonly ILookup<string, WalletRuleEntry> _entriesByWalletLookup;

        private readonly MultiValueDictionary<Script, WalletRuleEntry> _entriesByAddress;
        private readonly ILookup<Script, WalletRuleEntry> _entriesByAddressLookup;

        internal WalletRuleEntryCollection(IEnumerable<WalletRuleEntry> walletRules)
        {
            if (walletRules == null)
                throw new ArgumentNullException("walletRules");

            _walletRules = new List<WalletRuleEntry>();
            _entriesByWallet = new MultiValueDictionary<string, WalletRuleEntry>();
            _entriesByWalletLookup = _entriesByWallet.AsLookup();

            _entriesByAddress = new MultiValueDictionary<Script, WalletRuleEntry>();
            _entriesByAddressLookup = _entriesByAddress.AsLookup();
            foreach (var rule in walletRules)
            {
                Add(rule);
            }
        }

        public bool Add(WalletRuleEntry entry)
        {
            if (!_walletsIds.Add(GetId(entry)))
            {
                return false;
            }

            _walletRules.Add(entry);
            _entriesByWallet.Add(entry.WalletId, entry);

            if (entry.Rule is ScriptRule rule)
            {
                _entriesByAddress.Add(rule.ScriptPubKey, entry);
            }

            return true;
        }

        public void AddRange(IEnumerable<WalletRuleEntry> entries)
        {
            foreach (var entry in entries)
                Add(entry);
        }

        public IEnumerable<WalletRuleEntry> GetRulesForWallet(string walletName)
        {
            return _entriesByWalletLookup[walletName];
        }
        
        public IEnumerable<WalletRuleEntry> GetRulesFor(IDestination destination)
        {
            return GetRulesFor(destination.ScriptPubKey);
        }

        public IEnumerable<WalletRuleEntry> GetRulesFor(Script script)
        {
            return _entriesByAddressLookup[script];
        }

        public IEnumerator<WalletRuleEntry> GetEnumerator()
        {
            return _walletRules.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static Tuple<string, string> GetId(WalletRuleEntry entry)
        {
            return Tuple.Create(entry.WalletId, entry.Rule.Id);
        }

        public int Count => _walletRules.Count;
    }
}
