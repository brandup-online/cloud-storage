using CloudStorage.Models.Interfaces;

namespace CloudStorage.Models
{
    public class FileInfo<T> where T : class, IFileMetadata, new()
    {
        public Guid FileId { get; set; }
        public long Size { get; set; }

        public T Metadata { get; set; }

    }
}
