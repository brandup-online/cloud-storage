using CloudStorage.Client;
using CloudStorage.Models.Interfaces;

namespace CloudStorage.Storage
{
    public interface ICloudStorage
    {
        public ICloudClient<TFileType> CreateClient<TFileType>() where TFileType : class, IFileMetadata, new();
    }
}
