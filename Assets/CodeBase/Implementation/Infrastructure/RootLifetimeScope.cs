using CodeBase.Core.Systems;
using CodeBase.Core.Systems.Save;
using CodeBase.Services;
using CodeBase.Services.EventMediator;
using CodeBase.Services.Input;
using CodeBase.Services.LongInitializationServices;
using CodeBase.Services.SceneInstallerService;
using CodeBase.Systems;
using CodeBase.Systems.InputSystem;
using Microsoft.Extensions.Configuration;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CodeBase.Implementation.Infrastructure
{
    //RootLifeTimeScope where all the dependencies needed for the whole project are registered
    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private UniversalAppEventsService universalAppEventsService;
        [SerializeField] private AudioSystem audioSystem;
        
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterServices(builder);

            RegisterSystems(builder);
                
            builder.Register<ModuleTypeMapper>(Lifetime.Singleton);

            builder.Register<ModuleStateMachine>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            
            RegisterConfiguration(builder);
        }

        private static void RegisterConfiguration(IContainerBuilder builder)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Application.dataPath + "/Configuration")
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
            builder.RegisterInstance(configuration);
        }

        private void RegisterSystems(IContainerBuilder builder)
        {
            builder.Register<SaveSystem>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterInstance(universalAppEventsService)
                .As<IAppEventService>();
            
            builder.RegisterInstance(audioSystem)
                .AsImplementedInterfaces()
                .AsSelf();
        }
        
        private void RegisterServices(IContainerBuilder builder)
        {
            RegisterLongInitializationService(builder);

            builder.Register<EventMediator>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            
            builder.Register<InputSystemService>(Lifetime.Singleton)
                .As<IStartable>()
                .AsSelf();
            
            builder.Register<AudioListenerService>(Lifetime.Singleton)
                .AsSelf();
            
            builder.Register<SceneService>(Lifetime.Singleton);
            builder.Register<SceneInstallerService>(Lifetime.Singleton);
            builder.Register<LoadingServiceProvider>(Lifetime.Singleton);
        }

        private static void RegisterLongInitializationService(IContainerBuilder builder)
        {
            builder.Register<FirstLongInitializationService>(Lifetime.Singleton);
            builder.Register<SecondLongInitializationService>(Lifetime.Singleton);
            builder.Register<ThirdLongInitializationService>(Lifetime.Singleton);
        }
    }
}