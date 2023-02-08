namespace BrandUp.FileStorage.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MetadataKeyAttribute : Attribute
    {
        public string Name { get; }

        public MetadataKeyAttribute(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Name = name.ToPascalCase();
        }
    }
}