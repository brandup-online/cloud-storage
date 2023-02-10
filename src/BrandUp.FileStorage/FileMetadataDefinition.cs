using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Abstract.Configuration;
using BrandUp.FileStorage.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Reflection;

namespace BrandUp.FileStorage
{
    public class FileMetadataDefinition : IEnumerable<IPropertyCache>, IStorageInstanceCreator
    {
        readonly List<PropertyCache> caches = new();
        readonly ConstructorInfo constructor;

        private IStorageConfiguration defaultConfiguration; // Configuration for storage of file.
        private IStorageConfiguration fileConfiguration; // Custom configuration for file.

        public Type MetadataFileType { get; }

        public FileMetadataDefinition(Type metadataFileType, Type storageType)
        {
            MetadataFileType = metadataFileType;

            GeneratePropertyCollection(metadataFileType);
            constructor = GetStorageConstructor(storageType);
        }

        public void AddConfiguration(IStorageConfiguration storageConfiguration, IStorageConfiguration fileConfiguration)
        {
            this.defaultConfiguration = storageConfiguration;
            this.fileConfiguration = fileConfiguration;
        }

        #region IStorageInstanceCreator members

        public IFileStorage<T> CreateStorageInstance<T>(IServiceProvider serviceProvider) where T : class, IFileMetadata, new()
        {
            var constructorParameters = new List<object>();
            foreach (var parameter in constructor.GetParameters())
            {
                if (parameter.ParameterType.IsAssignableTo(typeof(IStorageConfiguration)))
                {
                    var joinedConfiguration = JoinConfiguration(parameter.ParameterType, defaultConfiguration, fileConfiguration);
                    constructorParameters.Add(joinedConfiguration);
                }
                else
                {
                    var service = serviceProvider.GetRequiredService(parameter.ParameterType);
                    constructorParameters.Add(service);
                }
            }

            return (IFileStorage<T>)constructor.Invoke(constructorParameters.ToArray());
        }

        private IStorageConfiguration JoinConfiguration(Type configType, IStorageConfiguration defaultConfiguration, IStorageConfiguration fileConfiguration)
        {
            if (configType == null)
                throw new ArgumentNullException(nameof(configType));
            if (defaultConfiguration == null)
                throw new ArgumentNullException(nameof(defaultConfiguration));
            if (fileConfiguration == null)
                return defaultConfiguration;

            var defaultConfigType = defaultConfiguration.GetType();
            var fileConfigType = fileConfiguration?.GetType();

            if (defaultConfigType != configType && !defaultConfigType.IsAssignableTo(configType))
                throw new ArgumentException();

            if (fileConfigType != configType && !fileConfigType.IsAssignableTo(configType))
                throw new ArgumentException();

            // Implied that file configuration can be heir of default configuration.
            if (fileConfigType.IsAbstract)
                throw new ArgumentException();
            var constructor = fileConfigType.GetConstructor(Type.EmptyTypes);
            var newInstance = constructor.Invoke(Type.EmptyTypes);

            foreach (var property in fileConfigType.GetProperties())
            {
                var value = property.GetValue(fileConfiguration, null);
                if (value == null)
                {
                    var defaultValue = defaultConfigType.GetProperty(property.Name).GetValue(defaultConfiguration, null);
                    property.SetValue(newInstance, defaultValue);
                }
                else
                {
                    property.SetValue(newInstance, value);
                }
            }
            return (IStorageConfiguration)newInstance;

        }

        #endregion

        #region IEnumerable members

        public IEnumerator<IPropertyCache> GetEnumerator()
        {
            return caches.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return caches.GetEnumerator();
        }

        #endregion

        #region Helpers 

        void GeneratePropertyCollection(Type type, string parentName = default)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
            foreach (var property in properties)
            {
                var ignore = property.GetCustomAttribute<MetadataIgnoreAttribute>() != null ? true : false;
                if (!ignore)
                {
                    var propertyName = property.GetCustomAttribute<MetadataKeyAttribute>()?.Name ?? property.Name;
                    var metadataName = parentName == null ? propertyName : parentName + "." + propertyName;

                    if (!property.PropertyType.IsSerializable)
                        GeneratePropertyCollection(property.PropertyType, metadataName);
                    else
                        AddProperty(metadataName, property);
                }
            }
        }

        void AddProperty(string fullName, PropertyInfo item)
        {
            var propertyMetadata = new PropertyCache() { FullPropertyName = fullName, Property = item };
            caches.Add(propertyMetadata);
        }

        ConstructorInfo GetStorageConstructor(Type storageType)
        {
            foreach (var constructor in storageType.MakeGenericType(MetadataFileType).GetConstructors())
            {
                if (constructor.IsPublic)
                {
                    var parameters = constructor.GetParameters();

                    foreach (var param in parameters)
                    {
                        if (param.ParameterType.IsAssignableTo(typeof(IStorageConfiguration)))
                            return constructor;
                    }
                }
            }
            throw new ArgumentException($"Type does not have suitable constructors (constructor must be public and have parameter of type {typeof(IStorageConfiguration)})");
        }

        #endregion
    }

    /// <summary>
    /// metadata property information 
    /// </summary>
    public class PropertyCache : IPropertyCache
    {
        /// <summary>
        /// Name of simple type property(if property is nested, properties separated by dot)
        /// </summary>
        public string FullPropertyName { get; set; }
        /// <summary>
        /// Property info object of this property
        /// </summary>
        public PropertyInfo Property { get; set; }
    }
}
