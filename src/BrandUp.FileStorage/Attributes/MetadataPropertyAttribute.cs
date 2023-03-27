namespace BrandUp.FileStorage.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MetadataPropertyAttribute : Attribute
    {
        public string Name { get; set; }
    }
}