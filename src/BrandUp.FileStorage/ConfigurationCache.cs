using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BrandUp.FileStorage
{
    public class ConfigurationCache
    {
        readonly IDictionary<Type, ConstructorInfo> storageConstructors;
        readonly Type storageType;
        readonly IDictionary<string, object> configs;
        readonly Type configurationType;

        public ConfigurationCache(Type storageType, IEnumerable<Type> fileTypes, Type configurationType, IConfiguration configuration)
        {
            this.storageType = storageType ?? throw new ArgumentNullException(nameof(storageType));

            var @default = configuration.GetSection("Default").Get(configurationType);
            if (@default == null)
                throw new ArgumentNullException(nameof(@default));

            configs = new Dictionary<string, object>() { { "Default", @default } };

            storageConstructors = new Dictionary<Type, ConstructorInfo>();

            foreach (var fileType in fileTypes)
            {
                var constructors = this.storageType.MakeGenericType(fileType).GetConstructors();
                if (!constructors.Any())
                    throw new ArgumentNullException(nameof(constructors));

                if (constructors.Length > 1)
                    throw new ArgumentException("У провайдера не может быть больше одного коструктора");
                var constructor = constructors.First();
                if (constructor.IsPublic == false)
                    throw new ArgumentException("Конструктор провайдера должен быть публичным");

                storageConstructors.Add(fileType, constructor);

                var options = configuration.GetSection(fileType.Name).Get(configurationType);
                if (options != null)
                {
                    if (!configs.TryAdd(fileType.Name, options))
                        throw new ArgumentException($"Для типа {fileType.Name} уже сущесвует конфигурация");
                }
            }

            this.configurationType = configurationType ?? throw new ArgumentNullException(nameof(configurationType));
        }

        internal IFileStorage<TFileType> CreateInstanse<TFileType>(IServiceProvider serviceProvider) where TFileType : class, new()
        {
            if (storageConstructors.TryGetValue(typeof(TFileType), out var constructor))
            {
                var @params = new List<object>();

                foreach (var param in constructor.GetParameters())
                {
                    if (param.ParameterType == configurationType)
                    {
                        var config = configurationType.GetConstructor(new Type[] { }).Invoke(new object[] { });
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
