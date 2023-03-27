namespace BrandUp.FileStorage
{
    public class File<TMetadata>
        where TMetadata : class, new()
    {
        public File(FileInfo fileInfo, TMetadata metadata)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            Id = fileInfo.Id;
            Size = fileInfo.Size;
            Metadata = metadata;
        }

        public Guid Id { get; init; }
        public long Size { get; init; }
        public TMetadata Metadata { get; init; }
    }
}