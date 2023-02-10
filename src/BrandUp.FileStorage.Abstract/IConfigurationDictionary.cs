namespace BrandUp.FileStorage.Abstract
{
    public interface IFileDefinitionsContext
    {
        bool TryGetProperties(Type type, out IEnumerable<IPropertyCache> value);
    }

}
