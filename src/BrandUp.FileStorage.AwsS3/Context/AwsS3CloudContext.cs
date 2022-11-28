using BrandUp.FileStorage.AwsS3.Configuration;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace BrandUp.FileStorage.AwsS3.Context
{
    public class AwsS3CloudContext : IAwsS3StorageContext
    {
        readonly Dictionary<Type, AwsS3Config> bucketConfigs = new();
        readonly Dictionary<Type, PropertyInfo[]> typeMetaData = new();
        readonly List<Type> bucketTypes = new();
        readonly IConfiguration configuration;

        public AwsS3CloudContext(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        #region IAwsS3CloudContext members

        public IDictionary<Type, AwsS3Config> BucketConfigs => bucketConfigs;

        public IDictionary<Type, PropertyInfo[]> TypeMetaData => typeMetaData;

        public IList<Type> BucketTypes => bucketTypes;

        public void AddClientType<T>() where T : class
        {
            bucketTypes.Add(typeof(T));
            var properties = typeof(T).GetProperties();
            typeMetaData.Add(typeof(T), properties.ToArray());

            var options = configuration.GetSection(typeof(T).Name).Get<AwsS3Config>();
            if (options != null)
            {
                if (!bucketConfigs.TryAdd(typeof(T), options))
                    throw new ArgumentException($"Для типа {typeof(T)} уже сущесвует конфигурация");
            }
        }

        #endregion
    }

    public interface IAwsS3StorageContext
    {
        IDictionary<Type, AwsS3Config> BucketConfigs { get; }
        IDictionary<Type, PropertyInfo[]> TypeMetaData { get; }

        IList<Type> BucketTypes { get; }

        void AddClientType<T>() where T : class;
    }
}