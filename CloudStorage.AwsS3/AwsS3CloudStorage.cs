using CloudStorage.Client;
using CloudStorage.Models.Interfaces;
using CloudStorage.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CloudStorage.AwsS3
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
