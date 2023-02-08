using BrandUp.FileStorage.Attributes;
using System.Collections;
using System.Reflection;

namespace BrandUp.FileStorage
{
    public class FileMetadataDefinition : IEnumerable<PropertyCache>
    {
        readonly List<PropertyCache> caches = new();
        readonly ConstructorInfo constructor;
        readonly List<PropertyInfo> serviceProperties = new List<PropertyInfo>();

        public Type MetadataFileType { get; }

        public FileMetadataDefinition(Type metadataFileType)
        {
            MetadataFileType = metadataFileType;

            GeneratePropertyCollection(metadataFileType);
        }

        #region Helpers 

        void GeneratePropertyCollection(Type type, string parentName = default)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
            foreach (var property in properties)
            {
                var propertyName = property.GetCustomAttribute<MetadataKeyAttribute>()?.Name ?? property.Name;
                var metadataName = parentName == null ? propertyName : parentName + "." + propertyName;

                if (!property.PropertyType.IsSerializable)
                    GeneratePropertyCollection(property.PropertyType, metadataName);
                else
                    AddProperty(metadataName, property);
            }
        }

        #endregion

        public PropertyCache AddProperty(string fullName, PropertyInfo item)
        {
            var propertyMetadata = new PropertyCache() { FullPropertyName = fullName, Property = item };
            caches.Add(propertyMetadata);
            return propertyMetadata;
        }

        public object CreateStorageInstance(IServiceProvider serviceProvider)
        {
            constructor.Invoke()
        }

        #region IEnumerable implementation

        public IEnumerator<PropertyCache> GetEnumerator()
        {
            return caches.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return caches.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// metadata property information 
    /// </summary>
    public class PropertyCache
    {
        /// <summary>
        /// Name of simple type property(if property is nested properties separated by dot)
        /// </summary>
        public string FullPropertyName { get; set; }
        /// <summary>
        /// Property info object of this property
        /// </summary>
        public PropertyInfo Property { get; set; }
    }
}
