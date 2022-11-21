using CloudStorage.Models.Interfaces;

namespace CloudStorage.Files
{
    public class ArchiveMetadata : IFileMetadata
    {
        public string FileName { get; set; }
        public Guid MailingId { get; set; }
    }
}
