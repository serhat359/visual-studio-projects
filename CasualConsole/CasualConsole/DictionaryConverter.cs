using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CasualConsole
{
    public class DictionaryConverter : JsonConverter
    {
        public override bool CanConvert(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var genericParameters = objectType.GetGenericArguments();
            var basePairType = typeof(KeyValuePair<,>).MakeGenericType(genericParameters);
            var keyProp = basePairType.GetProperty("Key");
            var valueProp = basePairType.GetProperty("Value");

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(basePairType);

            var deserialized = serializer.Deserialize(reader, enumerableType);

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(genericParameters);

            var dictionaryInstance = dictionaryType.GetConstructor(new Type[] { }).Invoke(null);
            var addMethod = dictionaryType.GetMethod("Add");

            foreach (var item in deserialized as IEnumerable)
            {
                var key = keyProp.GetValue(item);
                var value = valueProp.GetValue(item);
                addMethod.Invoke(dictionaryInstance, new object[] { key, value });
            }

            return dictionaryInstance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var item in value as IEnumerable)
            {
                serializer.Serialize(writer, item);
            }
            writer.WriteEndArray();
        }
    }
}
