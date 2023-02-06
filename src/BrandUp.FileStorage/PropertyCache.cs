using System.Collections;
using System.Reflection;

namespace BrandUp.FileStorage
{
    /// <summary>
    /// Collections of metadata property informations 
    /// </summary>
    public class PropertyCacheCollection : IEnumerable<PropertyCache>
    {
        private readonly IList<PropertyCache> caches = new List<PropertyCache>();
        /// <summary>
        /// Adds to collections new cache object
        /// </summary>
        /// <param name="fullName">Name of simple type property(if property is nested properties separated by dot)</param>
        /// <param name="item">Property info object of this property</param>
        public void Add(string fullName, PropertyInfo item)
        {
            caches.Add(new() { FullPropertyName = fullName, Property = item });
        }

        private IEnumerable<PropertyCache> GetValues()
        {
            foreach (var s in caches)
            {
                yield return s;
            }
        }

        #region IEnumerable implementation

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<PropertyCache> GetEnumerator()
        {
            return GetValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return caches.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// metadata property information 
    /// </summary>
    public class PropertyCache
    {
        /// <summary>
        /// Name of simple type property(if property is nested properties separated by dot)
        /// </summary>
        public string FullPropertyName { get; set; }
        /// <summary>
        /// Property info object of this property
        /// </summary>
        public PropertyInfo Property { get; set; }
    }
}
