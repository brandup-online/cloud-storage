using BrandUp.FileStorage.Abstract;

namespace BrandUp.FileStorage.Tests.Storages.Aws
{
    public class FakeFile : IFileMetadata
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public int FakeInt { get; set; }
        public DateTime FakeDateTime { get; set; }
        public TimeSpan FakeTimeSpan { get; set; }
        public FakeInnnerClass FakeInner { get; set; }
        public class FakeInnnerClass
        {
            public bool FakeBool { get; set; }
            public Guid FakeGuid { get; set; }
        }
    }

    public class AwsClientTest : AwsClientTestBase<FakeFile>
    {
        #region FileStorageTestBase

        internal override FakeFile CreateMetadataValue()
        {
            return new FakeFile
            {
                FakeInner = new() { FakeGuid = TestGuid, FakeBool = true },
                FileName = "string",
                Extension = "png",
                FakeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),
                FakeInt = 21332,
                FakeTimeSpan = TimeSpan.FromSeconds(127)
            };
        }

        #endregion
    }
}
