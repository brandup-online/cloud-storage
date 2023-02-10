using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Abstract.Configuration;
using Microsoft.Extensions.Configuration;

namespace BrandUp.FileStorage
{
    public static class IFileStorageBuilderExtension
    {
        public static IFileStorageBuilder AddStorageWithConfiguration(this IFileStorageBuilder builder, Type storageType, Type configurationType, IConfiguration configuration)
        {
            if (!storageType.IsAssignableToGenericType(typeof(IFileStorage<>)))
                throw new ArgumentException($"{storageType} Must be assignable to IFileStorage.");
            if (!configurationType.IsAssignableTo(typeof(IStorageConfiguration)))
                throw new ArgumentException($"{configurationType} Must be assignable to {nameof(IStorageConfiguration)}.");

            Dictionary<string, IStorageConfiguration> options = new();

            foreach (var config in configuration.GetChildren())
            {
                var inner = configurationType.GetConstructor(Type.EmptyTypes).Invoke(Type.EmptyTypes);
                config.Bind(inner);
                if (!options.TryAdd(config.Key, (IStorageConfiguration)inner))
                    throw new Exception("Configuration keys mest be unique.");
            }

            builder.AddStorage(storageType, options);

            return builder;
        }

    }
}
