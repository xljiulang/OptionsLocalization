using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace OptionsLocalization
{
    sealed class OptionsLocalizer<TOptions> : IOptionsLocalizer, IOptionsLocalizer<TOptions> where TOptions : class, new()
    {
        private readonly IOptionsMonitor<OptionsLocalizerOptions<TOptions>> options;
        private readonly IOptionsMonitor<TOptions> optionsMonitor;

        public Type OptionsType => typeof(TOptions);

        public TOptions CurrentValue => this.Get(Thread.CurrentThread.CurrentCulture);

        public CultureInfo[] SupportedCultures => this.options.CurrentValue.SupportedCultures.ToArray();

        public CultureInfo[] ExpectedCultures => this.options.CurrentValue.ExpectedCultures;

        public OptionsLocalizer(
            IOptionsMonitor<OptionsLocalizerOptions<TOptions>> options,
            IOptionsMonitor<TOptions> optionsMonitor)
        {
            this.options = options;
            this.optionsMonitor = optionsMonitor;
        }

        public TOptions Get(string culture)
        {
            return this.Get(CultureInfo.GetCultureInfo(culture));
        }

        public TOptions Get(CultureInfo culture)
        {
            if (string.IsNullOrEmpty(culture.Name) ||
                this.options.CurrentValue.DefaultCulture.Equals(culture))
            {
                return this.optionsMonitor.Get(Options.DefaultName);
            }

            if (this.options.CurrentValue.SupportedCultures.Contains(culture))
            {
                return this.optionsMonitor.Get(culture.Name);
            }

            return Get(culture.Parent);
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
                    culture = this.options.CurrentValue.DefaultCulture;
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