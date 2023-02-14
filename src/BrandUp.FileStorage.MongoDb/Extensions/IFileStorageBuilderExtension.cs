using BrandUp.FileStorage.Builder;
using BrandUp.MongoDB;

namespace BrandUp.FileStorage.MongoDb
{
    public static class IFileStorageBuilderExtension
    {
        public static IFileStorageBuilder AddMongoProvider<TMongoContext>(this IFileStorageBuilder builder, string configurationName, Action<MongoStorageOptions> configureOptions)
            where TMongoContext : MongoDbContext
        {
            builder.AddStorageProvider<MongoStorageProvider<TMongoContext>, MongoStorageOptions>(configurationName, configureOptions);

            return builder;
        }
    }
}