using System;
using System.Threading.Tasks;

namespace CodeBase.Services.LongInitializationServices
{
    public abstract class LongInitializationService
    {
        private bool _isInitialized;
        protected static int DelayTime = 1;

        public Task Init()   
        {
            if (!_isInitialized)
                return InitializeAsync();

            Console.WriteLine("LongInitializationService is already initialized.");
            return Task.CompletedTask;
        }

        private async Task InitializeAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(DelayTime));
            _isInitialized = true;
        }
    }
}