using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BrandUp.FileStorage
{
    internal class ConfigurationCache
    {
        readonly Type storageType;
        readonly Type configurationType;
        readonly Dictionary<Type, ConstructorInfo> storageConstructors = new();
        readonly IDictionary<string, object> configs;

        public ConfigurationCache(Type storageType, Type configurationType, object defaultConfiguration)
        {
            this.storageType = storageType ?? throw new ArgumentNullException(nameof(storageType));
            this.configurationType = configurationType ?? throw new ArgumentNullException(nameof(configurationType));

            configs = new Dictionary<string, object>() { { "Default", defaultConfiguration } };
        }

        internal void Add(Type fileType, object configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (configuration.GetType() != configurationType)
                throw new ArgumentException("Passed object does not match with the configuration type");

            var constructor = storageType.MakeGenericType(fileType).GetConstructors().Where(c => c.IsPublic
            && c.GetParameters().Select(p => p.ParameterType).Contains(configurationType)).FirstOrDefault();

            if (constructor == null)
                throw new ArgumentException($"Type does not have suitable constructors (constructor must be public and have parameter of type {configurationType.Name})");

            if (!storageConstructors.TryAdd(fileType, constructor))
                throw new ArgumentException($"Constructor for {fileType.Name} already exist");

            if (!configs.TryAdd(fileType.Name, configuration))
                throw new ArgumentException($"Configuration for {fileType.Name} already exist");
        }

        internal IFileStorage<TFileType> CreateInstanse<TFileType>(IServiceProvider serviceProvider) where TFileType : class, IFileMetadata, new()
        {
            if (storageConstructors.TryGetValue(typeof(TFileType), out var constructor))
            {
                var @params = new List<object>();

                foreach (var param in constructor.GetParameters())
                {
                    if (param.ParameterType == configurationType)
                    {
                        var config = configurationType.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
                        foreach (var prop in configurationType.GetProperties())
                        {
                            if (prop.GetValue(configs[typeof(TFileType).Name]) != null)
                            {
                                var value = prop.GetValue(configs[typeof(TFileType).Name]);
                                prop.SetValue(config, value);
                            }
                            else
                            {
                                var value = prop.GetValue(configs["Default"]);
                                prop.SetValue(config, value);
                            }
                        }
                        @params.Add(config);
                    }
                    else
                    {
                        var service = serviceProvider.GetRequiredService(param.ParameterType);
                        @params.Add(service);
                    }
                }

                return (IFileStorage<TFileType>)constructor.Invoke(@params.ToArray());
            }
            else throw new ArgumentException($"For type {typeof(TFileType)} did not stored constructors");
        }
    }
}