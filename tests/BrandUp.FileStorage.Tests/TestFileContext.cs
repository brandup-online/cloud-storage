using BrandUp.FileStorage.Tests;

namespace BrandUp.FileStorage
{
    public class TestFileContext : FileStorageContext, ITestContext
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
}
