using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OptionsLocalization.JsonLocalization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OptionsLocalization
{
    /// <summary>
    /// IOptionsLocalizationBuilder 扩展
    /// </summary>
    public static class OptionsLocalizationBuilderExtensions
    {
        private record LocalizationRoot(string Value);

        /// <summary>
        /// 添加 JSON 本地化支持
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="localizationRoot">本地化选项的 JSON 资源文件根目录</param>
        /// <returns></returns>
        public static IOptionsLocalizationBuilder AddJsonLocalization(this IOptionsLocalizationBuilder builder, string localizationRoot = "localizations")
        {
            builder.CheckLocalizationRoot(localizationRoot);

            foreach (var optionsPath in Directory.GetDirectories(localizationRoot))
            {
                var targetCultures = new HashSet<CultureInfo>();
                foreach (var jsonFilePath in Directory.GetFiles(optionsPath, "*.json"))
                {
                    var fileName = Path.GetFileNameWithoutExtension(jsonFilePath);
                    if (OptionsLocalizer.TryGetCultureInfo(fileName, out var culture))
                    {
                        if (targetCultures.Add(culture))
                        {
                            builder.AddJsonFile(jsonFilePath);
                        }
                    }
                }

                foreach (var culture in builder.ExpectedCultures)
                {
                    if (targetCultures.Add(culture))
                    {
                        var jsonFilePath = Path.Combine(optionsPath, $"{culture.Name}.json");
                        builder.AddJsonFile(jsonFilePath);
                    }
                }
            }

            builder.Services.AddHostedService<JsonLocalizationHostedService>();
            builder.Services.Configure<JsonLocalizationOptions>(options => options.LocalizationRoot = localizationRoot);
            return builder;
        }

        private static void CheckLocalizationRoot(this IOptionsLocalizationBuilder builder, string localizationRoot)
        {
            ArgumentException.ThrowIfNullOrEmpty(localizationRoot);

            if (localizationRoot.StartsWith('.') ||
                Path.IsPathRooted(localizationRoot) ||
                Path.GetDirectoryName(localizationRoot.AsSpan()).Length > 0)
            {
                throw new ArgumentException("Localization root must be a directory name.", nameof(localizationRoot));
            }

            var descriptor = builder.Services.FirstOrDefault(i => i.ServiceType == typeof(LocalizationRoot));
            if (descriptor != null && descriptor.ImplementationInstance is LocalizationRoot root)
            {
                throw new InvalidOperationException($"Localization root has already been set to '{root.Value}'.");
            }

            builder.Services.AddSingleton(new LocalizationRoot(localizationRoot));
            Directory.CreateDirectory(localizationRoot);
        }

        private static void AddJsonFile(this IOptionsLocalizationBuilder builder, string jsonFilePath)
        {
            builder.Configuration.Add<CultureJsonConfigurationSource>(s =>
            {
                s.Path = jsonFilePath;
                s.Optional = true;
                s.ReloadOnChange = true;
                s.ResolveFileProvider();
            });
        }
    }
}
