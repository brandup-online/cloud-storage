namespace BrandUp.FileStorage.Abstract
{
    public interface IFileDefinitionsDictionary
    {
        bool TryGetConstructor(Type type, out IStorageInstanceCreator value);
        bool TryGetProperties(Type type, out IEnumerable<IPropertyCache> value);
    }

}
