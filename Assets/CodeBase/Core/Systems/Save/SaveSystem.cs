using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace CodeBase.Core.Systems.Save
{
	//TODO Dispose бы не помешал, раз мы в конструкторе подписываемся на ивенты
	public class SaveSystem : IStartable
	{
		private readonly List<ISerializableDataSystem> _serializableDataSystems = new();
		private readonly SerializableDataFileLoader _serializableDataFileLoader = new();

		private SerializableDataContainer _serializableDataContainer;
		private bool _isLoaded;

		public SaveSystem(IAppEventService universalAppEventsService) => 
			universalAppEventsService.OnApplicationFocusEvent += SaveDataOnApplicationUnfocus;

		public void Start() => Initialize().Forget();

		private async UniTaskVoid Initialize()
		{
			_serializableDataContainer = await _serializableDataFileLoader.Read();
			if (_serializableDataContainer == null) 
				_serializableDataContainer = new SerializableDataContainer();

			foreach(var settingsSystem in _serializableDataSystems) 
				settingsSystem.LoadData(_serializableDataContainer);

			_isLoaded = true;
		}

		public void AddSystem(ISerializableDataSystem serializableDataSystem) 
		{
			_serializableDataSystems.Add(serializableDataSystem);
			if (_isLoaded) 
				serializableDataSystem.LoadData(_serializableDataContainer);
		}

		public async UniTaskVoid SaveData()
		{
			foreach(var serializableDataSystem in _serializableDataSystems) 
				serializableDataSystem.SaveData(_serializableDataContainer);

			await _serializableDataFileLoader.Write(_serializableDataContainer);
		}

		private void SaveDataOnApplicationUnfocus(bool isFocused)
		{
			if(!isFocused) 
				SaveData().Forget();
		}
	}
}