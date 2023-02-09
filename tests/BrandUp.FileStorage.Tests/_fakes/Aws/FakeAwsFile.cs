using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Attributes;

namespace BrandUp.FileStorage.Tests._fakes.Aws
{
    public class FakeAwsFile : IFileMetadata
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public int FakeInt { get; set; }
        public DateTime FakeDateTime { get; set; }
        public TimeSpan FakeTimeSpan { get; set; }
        public FakeInnnerClass FakeInner { get; set; }
    }

    public class AttributedFakeFile : IFileMetadata
    {
        public string FileName { get; set; }
        [MetadataKey("Extension")]
        public string Ext { get; set; }
        [MetadataKey("fake-int")]
        public int Int { get; set; }
        [MetadataKey("fake date time")]
        public DateTime DateTime { get; set; }
        [MetadataKey("fake_time_Span")]
        public TimeSpan FakeSpan { get; set; }
        [MetadataKey("Fake_INNer")]
        public AttributtedInnerClass Inner { get; set; }

    }

    public class FakeInnnerClass
    {
        public bool FakeBool { get; set; }
        public Guid FakeGuid { get; set; }
    }

    public class AttributtedInnerClass
    {
        [MetadataKey("FakeBool")]
        public bool Bool { get; set; }
        [MetadataKey("Fake      - guid")]
        public Guid InnerGuid { get; set; }
    }

    public class FakeMetadataOld : IFileMetadata
    {
        public string FileName { get; set; }

        public Guid SomeGuid { get; set; }

        public DateTime SomeDateTime { get; set; }
    }

    public class FakeMetadataNew : IFileMetadata
    {
        public string FileName { get; set; }

        public Guid SomeGuid { get; set; }

        public DateTime SomeDateTime { get; set; }

        public int SomeInt { get; set; }
    }
}
