using BrandUp.CloudStorage.Models.Interfaces;

namespace BrandUp.CloudStorage.Files
{
    public class ArchiveMetadata : IFileMetadata
    {
        public string FileName { get; set; }
        public Guid MailingId { get; set; }
    }
}
