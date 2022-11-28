using BrandUp.FileStorage.AwsS3.Configuration;

namespace BrandUp.FileStorage.AwsS3
{
    public static class IFileStorageFactoryExtention
    {
        public static IFileStorage<TFile> CreateAwsStorage<TFile>(this IFileStorageFactory factory) where TFile : class, new()
        {
            return factory.Create<TFile, AwsS3Configuration>();
        }
    }
}
