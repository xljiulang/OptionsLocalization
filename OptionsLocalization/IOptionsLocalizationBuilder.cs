using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace OptionsLocalization
{
    /// <summary>
    /// 选项地本化构建器
    /// </summary>
    public interface IOptionsLocalizationBuilder
    {
        /// <summary>
        /// 获取服务集合
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// 获取配置管理器
        /// </summary>
        IConfigurationBuilder Configuration { get; }

        /// <summary>
        /// 获取默认语言区域
        /// </summary>
        CultureInfo DefaultCulture { get; }

        /// <summary>
        /// 获取选项期望支持的语言区域集合
        /// 这些语言的文件在程序运行后创建也得到跟踪
        /// </summary>
        CultureInfo[] ExpectedCultures { get; }
    }
}
