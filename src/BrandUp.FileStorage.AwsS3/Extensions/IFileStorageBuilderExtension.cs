using BrandUp.FileStorage.AwsS3.Configuration;
using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.AwsS3
{
    public static class IFileStorageBuilderExtension
    {
        public static IFileStorageBuilder AddAwsS3Storage(this IFileStorageBuilder builder, IConfiguration configuration)
        {
            AwsS3Configuration config = new();

            configuration.Bind(config);
            builder.AddConfiguration(typeof(AwsS3FileStorage<>), config);

            builder.Services.AddScoped(typeof(IMetadataSerializer<>), typeof(MetadataSerializer<>));

            return builder;
        }

        public static IFileStorageBuilder AddAwsS3Bucket<TFile>(this IFileStorageBuilder builder, Action<AwsS3Configuration> configureAction) where TFile : class, new()
        {
            var options = new AwsS3Configuration();
            configureAction(options);

            builder.AddFileToStorage<TFile>(options);

            return builder;
        }
    }
}