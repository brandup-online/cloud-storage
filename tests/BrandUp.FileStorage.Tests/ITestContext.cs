namespace BrandUp.FileStorage.Tests
{
    public interface ITestContext
    {
        public IFileCollection<TestFile> FileStorageTestFiles { get; }
    }
}
