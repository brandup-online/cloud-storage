namespace BrandUp.FileStorage.Attributes
{
    /// <summary>
    /// Sets custom metadata key for property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MetadataKeyAttribute : Attribute
    {
        /// <summary>
        /// Custom key
        /// </summary>
        public string MetadataKey { get; internal set; }

        /// <summary>
        /// Attribute constructor.
        /// </summary>
        /// <param name="key"> Custom key</param>
        /// <exception cref="ArgumentNullException"></exception>
        public MetadataKeyAttribute(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            MetadataKey = key.ToPascalCase();
        }
    }
}
