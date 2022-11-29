using BrandUp.FileStorage.Builder;

namespace BrandUp.FileStorage
{
    public class FileStorageFactory : IFileStorageFactory
    {
        readonly IFileStorageBuilder builder;
        readonly IServiceProvider provider;
        public FileStorageFactory(IFileStorageBuilder builder, IServiceProvider provider)
        {
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public IFileStorage<TFileType> Create<TFileType, TConfigType>() where TFileType : class, new()
                                                                        where TConfigType : class, new()
           => builder.ConfigurationCache[typeof(TConfigType)].CreateInstanse<TFileType>(provider);

    }

    public interface IFileStorageFactory
    {
        public IFileStorage<TFileType> Create<TFileType, TConfigType>() where TFileType : class, new()
        where TConfigType : class, new();
    }
}