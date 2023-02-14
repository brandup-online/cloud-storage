namespace BrandUp.FileStorage.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MetadataRequiredAttribute : Attribute { }
}