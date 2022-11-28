namespace BrandUp.CloudStorage
{
    public class FileInfo<T> where T : class, new()
    {
        public Guid FileId { get; init; }
        public long Size { get; init; }
        public T Metadata { get; init; }
    }
}