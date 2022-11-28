namespace BrandUp.FileStorage
{
    public interface ICloudClientFactory
    {
        public ICloudClient<TFileType> CreateClient<TFileType>() where TFileType : class, new();
    }
}