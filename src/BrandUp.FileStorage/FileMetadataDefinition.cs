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

        private IFileStorageConfiguration storageConfiguration; // Configuration for storage of file.
        private IFileMetadataConfiguration fileConfiguration; // Custom configuration for file.

        public Type MetadataFileType { get; }

        public FileMetadataDefinition(Type metadataFileType, Type storageType)
        {
            MetadataFileType = metadataFileType;

            GeneratePropertyCollection(metadataFileType);
            constructor = GetStorageConstructor(storageType);
        }

        public void AddConfiguration(IFileStorageConfiguration storageConfiguration, IFileMetadataConfiguration fileConfiguration)
        {
            this.storageConfiguration = storageConfiguration;
            this.fileConfiguration = fileConfiguration;
        }

        #region IStorageInstanceCreator members

        public IFileStorage<T> CreateStorageInstance<T>(IServiceProvider serviceProvider) where T : class, IFileMetadata, new()
        {
            var constructorParameters = new List<object>();
            foreach (var parameter in constructor.GetParameters())
            {
                if (parameter.ParameterType == typeof(IFileStorageConfiguration))
                {
                    constructorParameters.Add(storageConfiguration);
                }
                else if (parameter.ParameterType == typeof(IFileMetadataConfiguration))
                {
                    constructorParameters.Add(fileConfiguration);
                }
                else
                {
                    var service = serviceProvider.GetRequiredService(parameter.ParameterType);
                    constructorParameters.Add(service);
                }
            }

            return (IFileStorage<T>)constructor.Invoke(constructorParameters.ToArray());
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

            //.Where(c => c.IsPublic && c.GetParameters().Select(p => p.ParameterType).Contains(configurationType))
            //.FirstOrDefault();
            ConstructorInfo storageConstructor = null;
            foreach (var constructor in storageType.MakeGenericType(MetadataFileType).GetConstructors())
            {
                if (constructor.IsPublic)
                {
                    var parameters = constructor.GetParameters();

                    // Finding IFileStorageConfiguration parameter. This parameter is required
                    bool storageConfigFlag = false;
                    foreach (var parameter in parameters)
                    {
                        if (parameter.ParameterType == typeof(IFileStorageConfiguration))
                        {
                            storageConfigFlag = true;
                            break;
                        }
                    }

                    // Finding IFileMetadataConfiguration parameter.
                    // Storage can not suppose configuration for certain file, so this parameter is optional.
                    if (storageConfigFlag)
                    {
                        bool fileConfigFlag = false;
                        foreach (var parameter in parameters)
                        {
                            if (parameter.ParameterType == typeof(IFileMetadataConfiguration))
                            {
                                fileConfigFlag = true;
                                break;
                            }
                        }

                        storageConstructor = constructor;

                        //If constructor with both configs was found, finding is break
                        if (fileConfigFlag)
                        {
                            break;
                        }
                    }
                }
            }

            if (storageConstructor == null)
                throw new ArgumentException($"Type does not have suitable constructors (constructor must be public and have parameter of type {typeof(IFileStorageConfiguration)})");

            return storageConstructor;
        }
        //void Add(Type fileType, object configuration)
        //{
        //    if (configuration == null)
        //        throw new ArgumentNullException(nameof(configuration));

        //    if (configuration.GetType() != configurationType)
        //        throw new ArgumentException("Passed object does not match with the configuration type");

        //    var constructor = storageType.MakeGenericType(fileType).GetConstructors().Where(c => c.IsPublic
        //    && c.GetParameters().Select(p => p.ParameterType).Contains(configurationType)).FirstOrDefault();

        //    if (constructor == null)
        //        throw new ArgumentException($"Type does not have suitable constructors (constructor must be public and have parameter of type {configurationType.Name})");

        //    if (!storageConstructors.TryAdd(fileType, constructor))
        //        throw new ArgumentException($"Constructor for {fileType.Name} already exist");

        //    if (!configs.TryAdd(fileType.Name, configuration))
        //        throw new ArgumentException($"Configuration for {fileType.Name} already exist");
        //}

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
