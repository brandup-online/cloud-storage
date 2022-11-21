using BrandUp.CloudStorage.Client;
using BrandUp.CloudStorage.Models.Interfaces;
using BrandUp.CloudStorage.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.CloudStorage.AwsS3
{
    public class AwsS3CloudStorage : ICloudStorage
    {
        readonly IServiceProvider serviceProvider;

        public AwsS3CloudStorage(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ICloudClient<TFileType> CreateClient<TFileType>() where TFileType : class, IFileMetadata, new()
            => serviceProvider.GetRequiredService<ICloudClient<TFileType>>();
    }
}
