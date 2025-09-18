using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnityEngine;
using VContainer;

namespace CodeBase.Core
{
    public class VContainerServiceProvider : IServiceProvider
    {
        private readonly IObjectResolver _resolver;

        public VContainerServiceProvider(IObjectResolver resolver) => 
            _resolver = resolver;

        public object GetService(Type serviceType)
        {
            /*if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = serviceType.GetGenericArguments()[0];
                var method = typeof(IObjectResolver).GetMethod("ResolveAll")?.MakeGenericMethod(elementType);
                var result = method?.Invoke(_resolver, null);
                return result ?? Array.CreateInstance(elementType, 0);
            }*/

            try { return _resolver.Resolve(serviceType); }
            catch (VContainerException ex)
            {
                Debug.LogError($"VContainer could not resolve {serviceType}: {ex.Message}");
                return null;
            }
        }
    }

    public static class MediatRExtensions
    {
        private static readonly Dictionary<Assembly, Type[]> HandlerTypesCache = new();

        public static void AddMediatR(this IContainerBuilder builder, params Assembly[] assemblies)
        {
            RegisterMediatRLicense();

            builder.Register<VContainerServiceProvider>(Lifetime.Singleton).As<IServiceProvider>();
            builder.Register(resolver => 
                    new Mediator(resolver.Resolve<IServiceProvider>()), Lifetime.Singleton)
                .As<IMediator>();

            foreach (var assembly in assemblies) 
                RegisterMediatRHandlers(builder, assembly);
            
            return;

            void RegisterMediatRLicense()
            {
                builder.Register(resolver =>
                {
                    var configuration = resolver.Resolve<IConfiguration>();
                    const string licenseKeySection = "MediatR:LicenseKey";
                    var licenseKey = configuration.GetValue<string>(licenseKeySection)
                                     ?? throw new InvalidOperationException($"License key not found in section '{licenseKeySection}'");
                    return new MediatRServiceConfiguration { LicenseKey = licenseKey };
                }, Lifetime.Singleton);
                
                var loggerFactory = new LoggerFactory();
                builder.RegisterInstance<ILoggerFactory>(loggerFactory);
                
                var mediatRAssembly = typeof(Mediator).Assembly;
                var licenseAccessorType = mediatRAssembly.GetType("MediatR.Licensing.LicenseAccessor")
                                          ?? throw new InvalidOperationException("The LicenseAccessor type was not found in MediatR");
                builder.Register(licenseAccessorType, Lifetime.Singleton);
            
                var licenseValidatorType = mediatRAssembly.GetType("MediatR.Licensing.LicenseValidator") 
                                           ?? throw new InvalidOperationException("The LicenseValidator type was not found in MediatR");
                builder.Register(licenseValidatorType, Lifetime.Singleton);
            }
        }

        private static void RegisterMediatRHandlers(IContainerBuilder builder, Assembly assembly)
        {
            if (!HandlerTypesCache.TryGetValue(assembly, out var handlerTypesArray))
            {
                handlerTypesArray = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface &&
                                t.GetInterfaces().Any(IsMediatRHandlerInterface))
                    .ToArray();
                HandlerTypesCache[assembly] = handlerTypesArray;
            }

            foreach (var handlerType in handlerTypesArray)
            {
                var interfaces = handlerType.GetInterfaces()
                    .Where(IsMediatRHandlerInterface)
                    .ToArray();

                foreach (var handlerInterface in interfaces) 
                    builder.Register(handlerType, Lifetime.Transient).As(handlerInterface);
            }

            return;

            static bool IsMediatRHandlerInterface(Type type)
            {
                return type.IsGenericType &&
                       (type.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                        type.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                        type.GetGenericTypeDefinition() == typeof(INotificationHandler<>));
            }
        }
    }
}