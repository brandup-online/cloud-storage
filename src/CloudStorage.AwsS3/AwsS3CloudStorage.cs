using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.CloudStorage.AwsS3
{
    public class AwsS3CloudStorage : ICloudClientFactory
    {
        readonly IServiceProvider serviceProvider;

        public AwsS3CloudStorage(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ICloudClient<TFileType> CreateClient<TFileType>() where TFileType : class, new()
            => serviceProvider.GetRequiredService<ICloudClient<TFileType>>();
    }
}