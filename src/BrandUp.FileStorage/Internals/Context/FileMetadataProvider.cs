using System.Reflection;

namespace BrandUp.FileStorage.Internals.Context
{
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
