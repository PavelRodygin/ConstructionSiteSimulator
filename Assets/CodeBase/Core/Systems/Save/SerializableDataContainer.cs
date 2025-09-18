using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CodeBase.Core.Systems.Save
{
	public class SerializableDataContainer
	{
		[JsonConverter(typeof(TypedDictionaryConverter))]
		[JsonProperty]
		private readonly Dictionary<Type, IDictionary> _data = new();
		[JsonProperty]
		private readonly Dictionary<string, Type> _dataTypes = new();

		public bool TryGet<T>(string dataName, out T dataValue)
		{
			if(_dataTypes.TryGetValue(dataName, out var cachedType) && typeof(T) != cachedType)
			{
				throw new InvalidCastException();
			}
			
			if(!_data.TryGetValue(typeof(T), out var rawDictionary))
			{
				dataValue = default(T);
				return false;
			}

			var dictionary = (Dictionary<string, T>)rawDictionary;
			return dictionary.TryGetValue(dataName, out dataValue);
		}

		public void SetData<T>(string dataName, T dataValue)
		{
			var type = typeof(T);
			if(_dataTypes.TryGetValue(dataName, out var cachedType) && cachedType != type)
			{
				throw new InvalidCastException();
			}

			if(!_data.TryGetValue(type, out var rawDictionary))
			{
				rawDictionary = new Dictionary<string, T>(1);
				_data.Add(type, rawDictionary);
			}

			var dictionary = (Dictionary<string, T>)rawDictionary;
			if(dictionary.TryAdd(dataName, dataValue))
			{
				_dataTypes.Add(dataName, type);
			}
			else
			{
				dictionary[dataName] = dataValue;
			}
		}

		public void Reset(string dataName)
		{
			if(!_dataTypes.TryGetValue(dataName, out var type))
			{
				return;
			}
			
			var dictionary = _data[type];
			dictionary.Remove(dataName);
		}

		public void ResetAll()
		{
			_data.Clear();
			_dataTypes.Clear();
		}
	}
}
