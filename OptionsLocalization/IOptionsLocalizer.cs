using System;
using System.Globalization;

namespace OptionsLocalization
{
    /// <summary>
    /// 本地化选项工具接口
    /// </summary>
    public interface IOptionsLocalizer
    {
        /// <summary>
        /// 获取选项类型
        /// </summary>
        Type OptionsType { get; }

        /// <summary>
        /// 获取选项期望支持的语言区域集合
        /// 这些语言的文件在程序运行后创建也得到跟踪
        /// </summary>
        CultureInfo[] ExpectedCultures { get; }

        /// <summary>
        /// 获取选项已支持的语言区域
        /// </summary>
        CultureInfo[] SupportedCultures { get; }

        /// <summary>
        /// 获取指定语言区域的本地化选项
        /// </summary>
        /// <param name="culture">指定语言区域</param>
        /// <returns></returns>
        object Get(CultureInfo culture);

        /// <summary>
        /// 监听选项变化
        /// </summary>
        /// <param name="listener">监听器</param>
        /// <returns></returns>
        IDisposable? OnChange(Action<object, CultureInfo> listener);
    }
}
