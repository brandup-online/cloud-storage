namespace BrandUp.FileStorage
{
    public class TestFileContext : FileStorageContext
    {
        public IFileCollection<TestFile> TempFiles => GetCollection<TestFile>("temp-files");
    }

    public class TestFile
    {
        public string FileName { get; set; }
    }
}
