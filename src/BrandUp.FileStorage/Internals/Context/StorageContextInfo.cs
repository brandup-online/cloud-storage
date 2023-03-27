using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BrandUp.FileStorage.Internals.Context
{
    internal class StorageContextInfo
    {
        readonly static Type FileStoreBaseType = typeof(IFileCollection<>);
        readonly Dictionary<Type, FileMetadataProvider> metadata = new();
        Type contextType;
        ConstructorInfo constructor;
        ParameterInfo[] constructorParameters;
        int providerParameterIndex;

        private StorageContextInfo() { }

        public static StorageContextInfo Create<TContext>()
            where TContext : FileStorageContext
        {
            var contextType = typeof(TContext);
            var contextConstructors = contextType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (contextConstructors.Length == 0)
                throw new InvalidOperationException();
            else if (contextConstructors.Length > 1)
                throw new InvalidOperationException();

            var contextConstructor = contextConstructors[0];
            var constructorParameters = contextConstructor.GetParameters();
            int providerParameterIndex = -1;
            for (var i = 0; i < constructorParameters.Length; i++)
            {
                var constructorParameter = constructorParameters[i];
                if (constructorParameter.ParameterType == typeof(IStorageProvider))
                    providerParameterIndex = i;
            }

            var contextInfo = new StorageContextInfo
            {
                contextType = contextType,
                constructor = contextConstructor,
                constructorParameters = constructorParameters,
                providerParameterIndex = providerParameterIndex
            };

            contextInfo.BuildMetadata();

            return contextInfo;
        }

        void BuildMetadata()
        {
            var properties = contextType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;
                if (!propertyType.IsConstructedGenericType)
                    continue;

                if (!propertyType.IsAssignableToGenericType(FileStoreBaseType))
                    continue;

                var metadataType = propertyType.GenericTypeArguments[0];
                if (metadata.ContainsKey(metadataType))
                    continue;

                var metadataInfo = FileMetadataProvider.Create(metadataType);
                metadata.Add(metadataType, metadataInfo);
            }
        }

        public FileMetadataProvider GetMetadataProvider<TMetadata>()
            where TMetadata : class, new()
        {
            var metadataType = typeof(TMetadata);
            if (!metadata.TryGetValue(metadataType, out var metadataProvider))
                throw new InvalidOperationException();
            return metadataProvider;
        }

        public TContext CreateInstance<TContext>(IServiceProvider serviceProvider, IStorageProvider storageProvider)
            where TContext : FileStorageContext
        {
            var constructorValues = new object[constructorParameters.Length];
            for (var i = 0; i < constructorParameters.Length; i++)
            {
                var constructorParameter = constructorParameters[i];

                if (i == providerParameterIndex)
                    constructorValues[i] = storageProvider;
                else
                {
                    var service = serviceProvider.GetRequiredService(constructorParameter.ParameterType);
                    constructorValues[i] = service;
                }
            }

            return (TContext)constructor.Invoke(constructorValues);
        }
    }
}
