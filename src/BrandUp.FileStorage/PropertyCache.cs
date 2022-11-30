using System.Collections;
using System.Reflection;

namespace BrandUp.FileStorage
{
    public class PropertyCacheCollection : IEnumerable<PropertyCache>
    {
        private readonly IList<PropertyCache> caches = new List<PropertyCache>();

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

    public class PropertyCache
    {
        public string FullPropertyName { get; set; }
        public PropertyInfo Property { get; set; }
    }
}
