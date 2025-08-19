using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OptionsLocalization.JsonLocalization
{
    sealed class JsonLocalizationHostedService : BackgroundService
    {
        private readonly string localizationRoot;
        private readonly IOptionsLocalizer[] optionsLocalizers;
        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public JsonLocalizationHostedService(
            IOptions<JsonLocalizationOptions> options,
            IEnumerable<IOptionsLocalizer> optionsLocalizers)
        {
            this.localizationRoot = options.Value.LocalizationRoot;
            this.optionsLocalizers = optionsLocalizers.Distinct().ToArray();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            foreach (var localizer in optionsLocalizers)
            {
                // 选项变化时，自动写入到json文件
                localizer.OnChange(this.OnChange);

                var optionsPath = Path.Combine(this.localizationRoot, localizer.OptionsType.Name);
                if (Directory.Exists(optionsPath))
                {
                    foreach (var jsonFilePath in Directory.GetFiles(optionsPath, "*.json"))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(jsonFilePath);
                        if (OptionsLocalizer.TryGetCultureInfo(fileName, out var culture))
                        {
                            var optionsValue = localizer.Get(culture);
                            WriteToValueFile(optionsValue, culture, optionsPath);
                        }
                    } 
                }
            }
        }


        private void OnChange(object optionsValue, CultureInfo culture)
        {
            var optionsPath = Path.Combine(this.localizationRoot, optionsValue.GetType().Name);
            var jsonFilePath = Path.Combine(optionsPath, $"{culture.Name}.json");
            if (File.Exists(jsonFilePath))
            {
                WriteToValueFile(optionsValue, culture, optionsPath);
            }
        }

        private static void WriteToValueFile(object optionsValue, CultureInfo culture, string optionsPath)
        {
            try
            {
                var valueFilePath = Path.Combine(optionsPath, $"{culture.Name}.json.value");
                var valueJson = JsonSerializer.SerializeToUtf8Bytes(optionsValue, optionsValue.GetType(), jsonSerializerOptions);

                using var valueFileStream = File.Create(valueFilePath);
                valueFileStream.Write("// 这是自动生成的完整语言文件内容，删除或修改此文件对应用没有影响\r\n"u8);
                valueFileStream.Write(valueJson);
            }
            catch (Exception)
            {
            }
        }
    }
}
