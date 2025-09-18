using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CodeBase.Core.Systems.Save
{
    public class TypedDictionaryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                && typeof(IDictionary).IsAssignableFrom(objectType.GetGenericArguments()[1]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var dictionary = (IDictionary)Activator.CreateInstance(objectType);
            foreach(var property in jsonObject.Properties())
            {
                var keyToken = property.Value["key"];
                var valueToken = property.Value["value"];
                var keyTypeToken = property.Value["keyType"];
                var valueTypeToken = property.Value["valueType"];
                if(keyToken == null || valueToken == null || keyTypeToken == null || valueTypeToken == null)
                {
                    throw new JsonSerializationException(
                        $"Invalid format for key '{property.Name}'. Expected 'key', 'value', 'keyType', and 'valueType' properties.");
                }

                var keyTypeName = keyTypeToken.Value<string>();
                var valueTypeName = valueTypeToken.Value<string>();
                var keyPropertyType = Type.GetType(keyTypeName);
                var valuePropertyType = Type.GetType(valueTypeName);
                if(keyPropertyType == null)
                {
                    throw new JsonSerializationException($"Key type '{keyTypeName}' not found.");
                }

                if(valuePropertyType == null)
                {
                    throw new JsonSerializationException($"Value type '{valueTypeName}' not found.");
                }
                
                var key = keyToken.ToObject(keyPropertyType, serializer);
                var value = valueToken.ToObject(valuePropertyType, serializer);
                dictionary.Add(key, value);
            }

            return dictionary;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dictionary = (IDictionary)value;
            var jsonObject = new JObject();
            foreach(var key in dictionary.Keys)
            {
                var dictionaryValue = dictionary[key];
                var keyToken = JToken.FromObject(key, serializer);
                var valueToken = JToken.FromObject(dictionaryValue, serializer);
                var keyTypeToken = new JValue(key.GetType().AssemblyQualifiedName);
                var valueTypeToken = new JValue(dictionaryValue.GetType().AssemblyQualifiedName);
                var propertyObject = new JObject
                {
                    ["key"] = keyToken,
                    ["value"] = valueToken,
                    ["keyType"] = keyTypeToken,
                    ["valueType"] = valueTypeToken
                };

                jsonObject[key.ToString()] = propertyObject;
            }

            jsonObject.WriteTo(writer);
        }
    }
}