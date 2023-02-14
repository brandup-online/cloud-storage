using System.Reflection;

namespace BrandUp.FileStorage.Internals
{
    internal interface IStorageProviderConfiguration
    {
        ProviderConfiguration Resolve(string configurationName);
    }

    internal class ProviderConfiguration
    {
        public Type ProviderType { get; }
        public object ProviderOptions { get; }

        public ProviderConfiguration(Type provderType, object providerOptions)
        {
            ProviderType = provderType;
            ProviderOptions = providerOptions;
        }

        public IStorageProvider Create(IServiceProvider serviceProvider)
        {
            var providerConstructors = ProviderType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (providerConstructors.Length == 0)
                throw new InvalidOperationException();
            else if (providerConstructors.Length > 1)
                throw new InvalidOperationException();

            var optionsType = ProviderOptions.GetType();
            var providerConstructor = providerConstructors[0];
            var constructorParameters = providerConstructor.GetParameters();
            var constructorValues = new object[constructorParameters.Length];
            for (var i = 0; i < constructorParameters.Length; i++)
            {
                var parameterInfo = constructorParameters[i];

                if (parameterInfo.ParameterType == optionsType)
                    constructorValues[i] = ProviderOptions;
                else
                {
                    var constructorValue = serviceProvider.GetService(parameterInfo.ParameterType);
                    if (constructorValue == null)
                        throw new InvalidOperationException();
                    constructorValues[i] = constructorValue;
                }
            }

            return (IStorageProvider)providerConstructor.Invoke(constructorValues);
        }
    }
}