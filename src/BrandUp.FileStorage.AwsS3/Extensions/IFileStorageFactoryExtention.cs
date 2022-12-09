namespace BrandUp.FileStorage.AwsS3
{
    /// <summary>
    /// 
    /// </summary>
    public static class IFileStorageFactoryExtention
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFile"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IFileStorage<TFile> CreateAwsStorage<TFile>(this IFileStorageFactory factory) where TFile : class, IFileMetadata, new()
            => factory.Create<TFile, Configuration.AwsS3Configuration>();

    }
}
