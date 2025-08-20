using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace OptionsLocalization
{
    sealed class OptionsLocalizer<TOptions> : IOptionsLocalizer, IOptionsLocalizer<TOptions> where TOptions : class, new()
    {
        private readonly IOptions<OptionsLocalizerOptions<TOptions>> options;
        private readonly IOptionsMonitor<TOptions> optionsMonitor;

        public Type OptionsType => typeof(TOptions);

        public TOptions CurrentValue => this.Get(Thread.CurrentThread.CurrentCulture);

        public CultureInfo[] SupportedCultures { get; private set; }

        public CultureInfo[] ExpectedCultures => this.options.Value.ExpectedCultures;

        public OptionsLocalizer(
            IConfiguration configuration,
            IOptions<OptionsLocalizerOptions<TOptions>> options,
            IOptionsMonitor<TOptions> optionsMonitor,
            IOptionsMonitorCache<TOptions> optionsMonitorCache)
        {
            this.options = options;
            this.optionsMonitor = optionsMonitor;

            var optionsSection = configuration.GetSection($"{nameof(OptionsLocalization)}:{typeof(TOptions).Name}");
            this.SupportedCultures = GetSupportedCultures(optionsSection).ToArray();

            ChangeToken.OnChange(optionsSection.GetReloadToken, OnOptionsChange);
            void OnOptionsChange()
            {
                optionsMonitorCache.Clear();
                this.SupportedCultures = GetSupportedCultures(optionsSection).ToArray();
            }
        }

        private static IEnumerable<CultureInfo> GetSupportedCultures(IConfigurationSection optionsSection)
        {
            foreach (var cultureSection in optionsSection.GetChildren())
            {
                if (OptionsLocalizer.TryGetCultureInfo(cultureSection.Key, out var culture))
                {
                    yield return culture;
                }
            }
        }

        public TOptions Get(string culture)
        {
            return this.Get(CultureInfo.GetCultureInfo(culture));
        }

        public TOptions Get(CultureInfo culture)
        {
            return this.optionsMonitor.Get(culture.Name);
        }

        /// <summary>
        /// 监听选项变化
        /// </summary>
        /// <param name="listener"></param>
        /// <returns></returns>
        public IDisposable? OnChange(Action<TOptions, CultureInfo> listener)
        {
            return this.optionsMonitor.OnChange(OnChange);

            void OnChange(TOptions optionsValue, string? name)
            {
                if (OptionsLocalizer.TryGetCultureInfo(name, out var culture) == false)
                {
                    culture = this.options.Value.DefaultCulture;
                }
                listener(optionsValue, culture);
            }
        }

        object IOptionsLocalizer.Get(CultureInfo culture)
        {
            return this.Get(culture);
        }

        IDisposable? IOptionsLocalizer.OnChange(Action<object, CultureInfo> listener)
        {
            return this.OnChange(OnChange);

            void OnChange(TOptions optionsValue, CultureInfo culture)
            {
                listener(optionsValue, culture);
            }
        }
    }
}