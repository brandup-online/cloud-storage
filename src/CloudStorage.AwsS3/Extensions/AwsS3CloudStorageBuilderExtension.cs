using BrandUp.FileStorage.AwsS3.Configuration;
using BrandUp.FileStorage.AwsS3.Context;
using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.AwsS3
{
    public static class IServiceCollectionExtension
    {
        public static AwsS3CloudStorageBuilder AddAwsS3Builder(this IServiceCollection services)
        {
            return new AwsS3CloudStorageBuilder(services);
        }

        public static ICloudStorageBuilder ConfigureAws(this ICloudStorageBuilder builder, IConfiguration configuration)
        {
            var services = builder.Services;

            services.Configure<AwsS3Config>(options => configuration.GetRequiredSection("Default").Bind(options));

            var context = new AwsS3CloudContext(configuration);

            services.AddSingleton<IAwsS3StorageContext>(context);

            return builder;
        }

        public static ICloudStorageBuilder StoreFileByAws<TFileMetadata>(this ICloudStorageBuilder builder)
        {
            return builder;
        }
    }
}