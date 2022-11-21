using CloudStorage.AwsS3.Configuration;
using CloudStorage.AwsS3.Context;
using CloudStorage.Client;
using CloudStorage.Extensions;
using CloudStorage.Models.Interfaces;
using CloudStorage.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CloudStorage.AwsS3
{
    public class AwsS3CloudStorageBuilder : ICloudStorageBuilder
    {
        public IServiceCollection Services { get; set; }
        private AwsS3CloudContext context;

        public AwsS3CloudStorageBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public AwsS3CloudStorageBuilder AddAwsCloudStorage(IConfiguration configuration)
        {
            Services.Configure<AwsS3Config>(options => configuration.GetRequiredSection("Default").Bind(options));

            context = new AwsS3CloudContext(configuration);

            Services.AddSingleton<IAwsS3StorageContext>(context);

            Services.AddSingleton<ICloudStorage, AwsS3CloudStorage>();

            return this;
        }

        public AwsS3CloudStorageBuilder AddClient<T>() where T : class, IFileMetadata, new()
        {

            context.AddClientType<T>();

            Services.AddTransient<ICloudClient<T>, AwsS3CloudClient<T>>();

            return this;
        }
    }
}
