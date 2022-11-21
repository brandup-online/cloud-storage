using BrandUp.CloudStorage.Client;
using BrandUp.CloudStorage.Models.Interfaces;

namespace BrandUp.CloudStorage.Storage
{
    public interface ICloudStorage
    {
        public ICloudClient<TFileType> CreateClient<TFileType>() where TFileType : class, IFileMetadata, new();
    }
}
