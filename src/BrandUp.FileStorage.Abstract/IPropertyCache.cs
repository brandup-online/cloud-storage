using System.Reflection;

namespace BrandUp.FileStorage.Abstract
{
    public interface IPropertyCache
    {
        /// <summary>
        /// Name of simple type property(if property is nested, properties separated by dot)
        /// </summary>
        string FullPropertyName { get; }
        /// <summary>
        /// Property info object of this property
        /// </summary>
        PropertyInfo Property { get; }
    }
}
