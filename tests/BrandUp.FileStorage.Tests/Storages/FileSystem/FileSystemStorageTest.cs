using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.FileSystem;
using BrandUp.FileStorage.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Storages.FileSystem
{
    public class FakeFile : IFileMetadata
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public int FakeInt { get; set; }
        public DateTime FakeDateTime { get; set; }
        public TimeSpan FakeTimeSpan { get; set; }
        public FakeInnnerClass FakeInner { get; set; }
    }

    public class FakeInnnerClass
    {
        public bool FakeBool { get; set; }
        public Guid FakeGuid { get; set; }
    }

    public class FileSystemStorageTest : FileStorageTest<FakeFile>
    {
        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddFolderStorage(Configuration.GetSection("TestFolderStorage"))
                        .AddFolderFor<FakeFile>("FakeAwsFile");
            //.AddFolderFor<_fakes.AttributedFakeFile>("FakeAwsFile")
            //.AddFolderFor<_fakes.FakeMetadataOld>("FakeAwsFile")
            //.AddFolderFor<_fakes.FakeMetadataNew>("FakeAwsFile");
            base.OnConfigure(services, builder);
        }

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
    }
}