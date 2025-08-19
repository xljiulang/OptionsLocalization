using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;

namespace OptionsLocalization
{
    /// <summary>
    /// 本地化选项工具
    /// </summary>
    public static class OptionsLocalizer
    {
        /// <summary>
        /// 设置当前线程的语言区域为指定值
        /// </summary>
        /// <param name="culture">语言区域</param>
        /// <param name="fallback">回退的语言区域</param>
        public static void SetCurrentThreadCulture(string? culture, string fallback)
        {
            SetCurrentThreadCulture(culture, CultureInfo.GetCultureInfo(fallback));
        }

        /// <summary>
        /// 设置当前线程的语言区域为指定值
        /// </summary>
        /// <param name="culture">语言区域</param>
        /// <param name="fallback">回退的语言区域</param>
        public static void SetCurrentThreadCulture(string? culture, CultureInfo fallback)
        {
            if (TryGetCultureInfo(culture, out var currentCulture) == false)
            {
                currentCulture = fallback;
            }

            Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentCulture;
        }

        /// <summary>
        /// 尝试获取指定名称的 CultureInfo
        /// </summary>
        /// <param name="culture">CultureInfo的名称</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetCultureInfo(string? culture, [MaybeNullWhen(false)] out CultureInfo value)
        {
            if (string.IsNullOrEmpty(culture))
            {
                value = null;
                return false;
            }

            try
            {
                value = CultureInfo.GetCultureInfo(culture);
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }
    }
}
