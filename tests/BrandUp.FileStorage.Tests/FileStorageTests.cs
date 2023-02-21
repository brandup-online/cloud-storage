using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage
{
    public abstract class FileStorageTests : FileStorageTestBase
    {
        readonly protected byte[] image = Tests.Properties.Resources.Image;
        readonly protected TestFileContext testFileContext;

        public FileStorageTests()
        {
            testFileContext = Services.GetRequiredService<TestFileContext>();
        }

        #region Tests 

        [Fact]

        public void Init_Success()
        {
            Assert.NotNull(testFileContext);
            Assert.NotNull(testFileContext.StorageProvider);

            var tempFiles = testFileContext.FileStorageTestFiles;
            Assert.NotNull(tempFiles);
        }

        [Fact]
        public async Task CRUD_Success()
        {
            TestFile file = new()
            {
                FileName = "Test",
                Size = 100,
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.Date,
            };

            using MemoryStream ms = new(image);

            await CRUDAsync(file, ms);
        }

        #endregion

        #region Helpers

        protected async Task CRUDAsync(TestFile file, Stream stream)
        {
            var fileinfo = await TestUploadAsync(file, stream);

            var getFileinfo = await TestGetAsync(fileinfo.Id);

            Equals(fileinfo.Metadata, getFileinfo.Metadata);

            await TestReadAsync(fileinfo.Id, stream);

            await TestDeleteAsync(fileinfo.Id);
        }

        protected async Task<File<TestFile>> TestUploadAsync(TestFile file, Stream stream)
        {
            var id = Guid.NewGuid();
            var fileinfo = await testFileContext.FileStorageTestFiles.UploadFileAsync(id, file, stream, CancellationToken.None);
            Assert.NotNull(fileinfo);
            Assert.Equal(fileinfo.Id, id);
            Assert.Equal(fileinfo.Size, stream.Length);
            Equals(file, fileinfo.Metadata);

            return fileinfo;
        }

        protected async Task<File<TestFile>> TestGetAsync(Guid id)
        {
            var getFileinfo = await testFileContext.FileStorageTestFiles.FindFileAsync(id, CancellationToken.None);
            Assert.NotNull(getFileinfo);
            Assert.Equal(getFileinfo.Id, id);
            Assert.True(getFileinfo.Size > 0);

            return getFileinfo;
        }

        protected async Task TestReadAsync(Guid id, Stream stream)
        {
            using var downlodadedStream = await testFileContext.FileStorageTestFiles.ReadFileAsync(id, CancellationToken.None);
            Assert.NotNull(downlodadedStream);
            CompareStreams(stream, downlodadedStream);
        }

        protected async Task TestDeleteAsync(Guid id)
        {
            var isDeleted = await testFileContext.FileStorageTestFiles.DeleteFileAsync(id, CancellationToken.None);
            Assert.True(isDeleted);

            Assert.Null(await testFileContext.FileStorageTestFiles.FindFileAsync(id, CancellationToken.None));
        }

        #endregion

        #region Utils helpers

        void Equals(TestFile first, TestFile second)
        {
            Assert.NotNull(first);
            Assert.NotNull(second);

            Assert.Equal(first.FileName, second.FileName);
            Assert.Equal(first.Id, second.Id);
            Assert.Equal(first.Size, second.Size);
            Assert.Equal(first.CreatedDate, second.CreatedDate);
        }

        void CompareStreams(Stream expected, Stream actual)
        {
            Stream assertStream = null;
            using var ms = new MemoryStream();
            try
            {
                actual.Seek(0, SeekOrigin.Begin);
                assertStream = actual;
            }
            catch
            {
                actual.CopyTo(ms);
                assertStream = ms;
                assertStream.Seek(0, SeekOrigin.Begin);
            }
            expected.Seek(0, SeekOrigin.Begin);

            Assert.Equal(expected.Length, actual.Length);

            var bytesToRead = sizeof(long);

            byte[] one = new byte[bytesToRead];
            byte[] two = new byte[bytesToRead];

            int iterations = (int)Math.Ceiling((double)expected.Length / bytesToRead);

            for (int i = 0; i < iterations; i++)
            {
                expected.Read(one, 0, bytesToRead);
                assertStream.Read(two, 0, bytesToRead);

                Assert.Equal(BitConverter.ToInt64(one, 0), BitConverter.ToInt64(two, 0));
            }

        }

        #endregion
    }
}
