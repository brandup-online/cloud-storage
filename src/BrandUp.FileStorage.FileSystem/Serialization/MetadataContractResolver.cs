using BrandUp.FileStorage.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BrandUp.FileStorage.FileSystem.Serialization
{
    internal class MetadataContractResolver : DefaultContractResolver
    {
        public static readonly MetadataContractResolver Instance = new MetadataContractResolver();
        public MetadataContractResolver() { }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            var keyAttribute = member.GetCustomAttribute<MetadataPropertyAttribute>();

            if (keyAttribute != null)
            {
                property.PropertyName = keyAttribute.Name; 
            }

            return property;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            var propsToIgnore = type.GetProperties().Where(p => p.PropertyType.GetCustomAttribute<MetadataIgnoreAttribute>() != null).Select(p => p.Name).ToList();

            properties =
                properties.Where(p => !propsToIgnore.Contains(p.PropertyName)).ToList();

            return properties;
        }
    }
}
