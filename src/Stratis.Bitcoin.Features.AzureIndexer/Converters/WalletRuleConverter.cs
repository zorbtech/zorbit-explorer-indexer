﻿using Newtonsoft.Json;
using System;

namespace Stratis.Bitcoin.Features.AzureIndexer.Converters
{
    public class WalletRuleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(WalletRule);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, typeof(ScriptRule));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value, typeof(ScriptRule));
        }
    }
}