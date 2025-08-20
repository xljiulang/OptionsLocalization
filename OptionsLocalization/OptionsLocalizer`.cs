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
    sealed class OptionsLocalizer<TOptions> : IOptionsLocalizer, IOptionsLocalizer<TOptions>, IDisposable where TOptions : class, new()
    {
        private readonly OptionsLocalizerOptions<TOptions> options;
        private readonly IOptionsMonitor<TOptions> optionsMonitor;
        private readonly IDisposable listener;
        private CultureInfo[] supportedCultures;

        public Type OptionsType => typeof(TOptions);

        public TOptions CurrentValue => this.Get(Thread.CurrentThread.CurrentCulture);

        public CultureInfo[] SupportedCultures => this.supportedCultures;

        public CultureInfo[] ExpectedCultures => this.options.ExpectedCultures;

        public OptionsLocalizer(
            IConfiguration configuration,
            IOptions<OptionsLocalizerOptions<TOptions>> options,
            IOptionsMonitor<TOptions> optionsMonitor,
            IOptionsMonitorCache<TOptions> optionsMonitorCache)
        {
            this.options = options.Value;
            this.optionsMonitor = optionsMonitor;

            var optionsSection = configuration.GetSection($"{nameof(OptionsLocalization)}:{typeof(TOptions).Name}");
            this.supportedCultures = GetSupportedCultures(optionsSection).ToArray();

            optionsMonitorCache.Clear();
            this.listener = ChangeToken.OnChange(optionsSection.GetReloadToken, OnOptionsChange);

            void OnOptionsChange()
            {
                optionsMonitorCache.Clear();
                this.supportedCultures = GetSupportedCultures(optionsSection).ToArray();
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
            return this.optionsMonitor.Get(culture);
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
                    culture = this.options.DefaultCulture;
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

        public void Dispose()
        {
            this.listener.Dispose();
        }
    }
}