using BrandUp.FileStorage.AwsS3.Configuration;
using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.AwsS3
{
    public static class ICloudStorageBuilderExtension
    {
        public static IFileStorageBuilder AddAwsS3Storage(this IFileStorageBuilder builder, IConfiguration configuration)
        {
            builder.AddConfiguration<AwsS3Configuration>(typeof(AwsS3FileStorage<>), configuration);

            builder.Services.AddScoped(typeof(IMetadataSerializer<>), typeof(MetadataSerializer<>));

            return builder;
        }
    }
}