using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Internals
{
    internal static class StorageContextTypes
    {
        readonly static Dictionary<Type, StorageContextInfo> contextsInfo = new();

        public static void RegisterContextType<TContext>()
            where TContext : FileStorageContext
        {
            var contextType = typeof(TContext);
            if (contextsInfo.ContainsKey(contextType))
                return;

            contextsInfo.Add(contextType, StorageContextInfo.Create<TContext>());
        }

        public static StorageContextInfo GetContextInfo<TContext>()
            where TContext : FileStorageContext
        {
            var contextType = typeof(TContext);
            if (!contextsInfo.TryGetValue(contextType, out var storageContextTypeMetadata))
                throw new ArgumentException();

            return storageContextTypeMetadata;
        }
    }

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

    internal class FileMetadataProvider
    {
        List<MetadataProperty> metadataProperties;

        public Type Type { get; init; }

        private FileMetadataProvider() { }

        public static FileMetadataProvider Create(Type metadataType)
        {
            var metadataProperties = new List<MetadataProperty>();
            var properties = metadataType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
            foreach (var property in properties)
                metadataProperties.Add(new MetadataProperty(property));

            var metadataProvider = new FileMetadataProvider
            {
                Type = metadataType,
                metadataProperties = metadataProperties
            };

            return metadataProvider;
        }

        public TMetadata Deserialize<TMetadata>(IDictionary<string, string> input)
            where TMetadata : class, new()
        {
            var metadata = new TMetadata();

            foreach (var property in metadataProperties)
            {
                //property.SetValue(metadata, null);
            }

            return metadata;
        }

        public void Serialize<TMetadata>(TMetadata metadata, IDictionary<string, string> output)
        {

        }

        class MetadataProperty
        {
            readonly PropertyInfo property;
            readonly string name;

            public string Name => name;

            public MetadataProperty(PropertyInfo property)
            {
                this.property = property;

                name = property.Name;
            }

            public void SetValue(object metadataObject, object value)
            {
                property.SetValue(metadataObject, value);
            }
        }
    }
}