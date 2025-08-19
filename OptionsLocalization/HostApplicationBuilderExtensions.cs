using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OptionsLocalization;
using System;
using System.Globalization;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// IHostApplicationBuilder扩展
    /// </summary>
    public static class HostApplicationBuilderExtensions
    {
        /// <summary>
        /// 添加选项地本化工具
        /// 默认添加了 JSON 本地化支持
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="defaultCulture">默认语言区域</param>
        /// <param name="expectedCultures">期望支持的语言区域集合，这些语言的文件在程序运行后创建也得到跟踪</param>
        /// <returns></returns>
        public static IOptionsLocalizerBuilder AddOptionsLocalizer(
            this IHostApplicationBuilder builder,
            string defaultCulture,
            string[] expectedCultures)
        {
            var defaultCultureInfo = CultureInfo.GetCultureInfo(defaultCulture);
            var expectedCultureInfos = Array.ConvertAll(expectedCultures, CultureInfo.GetCultureInfo);
            return builder.AddOptionsLocalizer(defaultCultureInfo, expectedCultureInfos);
        }

        /// <summary>
        /// 添加选项地本化工具
        /// 默认添加了 JSON 本地化支持
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="defaultCulture">默认语言区域</param>
        /// <param name="expectedCultures">期望支持的语言区域集合，这些语言的文件在程序运行后创建也得到跟踪</param>
        /// <returns></returns>
        public static IOptionsLocalizerBuilder AddOptionsLocalizer(
           this IHostApplicationBuilder builder,
           CultureInfo defaultCulture,
           CultureInfo[] expectedCultures)
        {
            return builder.AddOptionsLocalizer(defaultCulture, expectedCultures, c => c.AddJsonLocalization());
        }

        /// <summary>
        /// 添加选项地本化工具
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="defaultCulture">默认语言区域</param>
        /// <param name="expectedCultures">期望支持的语言区域集合</param>
        /// <param name="localizationBuilderConfig">选项本地化的配置构建者委托</param>
        /// <returns></returns>
        public static IOptionsLocalizerBuilder AddOptionsLocalizer(
            this IHostApplicationBuilder builder,
            CultureInfo defaultCulture,
            CultureInfo[] expectedCultures,
            Action<IOptionsLocalizationBuilder> localizationBuilderConfig)
        {
            var localizationBuilder = new OptionsLocalizationBuilder
            {
                Services = builder.Services,
                Configuration = builder.Configuration,
                DefaultCulture = defaultCulture,
                ExpectedCultures = expectedCultures
            };

            localizationBuilderConfig.Invoke(localizationBuilder);

            return new OptionsLocalizerBuilder
            {
                Services = builder.Services,
                Configuration = builder.Configuration,
                DefaultCulture = defaultCulture,
                ExpectedCultures = expectedCultures
            };
        }


        private class OptionsLocalizationBuilder : IOptionsLocalizationBuilder
        {
            public required IServiceCollection Services { get; init; }

            public required IConfigurationBuilder Configuration { get; init; }

            public required CultureInfo DefaultCulture { get; init; }

            public required CultureInfo[] ExpectedCultures { get; init; }
        }
    }
}