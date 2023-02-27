using BrandUp.FileStorage.Attributes;

namespace BrandUp.FileStorage
{
    public class TestFileContext : FileStorageContext
    {
        public IFileCollection<TestFile> FileStorageTestFiles => GetCollection<TestFile>("TempFiles");

    }
    public class TestFile
    {
        public string FileName { get; set; }
        public Guid Id { get; set; }
        public int Size { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AttributedsTestFileContext : FileStorageContext
    {
        public IFileCollection<AttributedTestFile> AttributedTestFiles => GetCollection<AttributedTestFile>("TempFiles");
    }

    public class AttributedTestFile
    {
        [MetadataRequired]
        public string FileName { get; set; }
        [MetadataProperty(Name = "Id")]
        public Guid MailingId { get; set; }
        public int Size { get; set; }
        public DateTime CreatedDate { get; set; }

        [MetadataIgnore]
        public string Ignore { get; set; }
    }
}
