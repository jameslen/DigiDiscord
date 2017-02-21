using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigiDiscord.Utilities
{
    public class JsonListToDictionaryById<T> : JsonConverter where T: class
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var data = serializer.Deserialize<List<T>>(reader);
            var result = data.ToDictionary(e => (string)((dynamic)e).Id, e => e);
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class JsonListToDictionaryByUserId<T> : JsonConverter where T : class
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var data = serializer.Deserialize<List<T>>(reader);
            var result = data.ToDictionary(e => (string)((dynamic)e).User.Id, e => e);
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
