using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace OptionsLocalization.JsonLocalization
{
    /// <summary>
    /// 本地化配置源
    /// </summary>
    sealed partial class CultureJsonConfigurationSource : JsonConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new CultureJsonConfigurationProvider(this);
        }
    }
}
