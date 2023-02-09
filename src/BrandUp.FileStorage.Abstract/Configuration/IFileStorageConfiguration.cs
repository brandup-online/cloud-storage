namespace BrandUp.FileStorage.Abstract.Configuration
{
    public interface IFileStorageConfiguration
    {
        public IDictionary<string, IFileMetadataConfiguration> InnerConfiguration { get; }
    }

    public interface IFileMetadataConfiguration
    {

    }
}
