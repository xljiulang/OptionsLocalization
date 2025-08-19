using Microsoft.Extensions.Configuration.Json;
using System;
using System.IO;
using System.Linq;

namespace OptionsLocalization.JsonLocalization
{
    sealed class CultureJsonConfigurationProvider(CultureJsonConfigurationSource source) : JsonConfigurationProvider(source)
    {
        public override void Load(Stream stream)
        {
            // 如果是空文件，则不读取数据
            if (stream is FileStream fileStream && fileStream.Length == 0L)
            {
                return;
            }

            base.Load(stream);

            var filePath = Source.Path;
            if (File.Exists(filePath))
            {
                // json配置文件的结构为Options类型结构，需要追加OptionsLocalization:{optionsName}:{culture}前缀
                var culture = Path.GetFileNameWithoutExtension(filePath);
                var optionsPath = Path.GetDirectoryName(filePath);
                var optionsName = Path.GetFileName(optionsPath);

                var keyPrefix = $"{nameof(OptionsLocalization)}:{optionsName}:{culture}";
                this.Data = this.Data.ToDictionary(kv => $"{keyPrefix}:{kv.Key}", kv => kv.Value, StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
