using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Globalization;

namespace OptionsLocalization
{
    sealed class OptionsLocalizerBuilder : IOptionsLocalizerBuilder
    {
        public required CultureInfo DefaultCulture;
        public required CultureInfo[] ExpectedCultures;
        public required IServiceCollection Services;
        public required IConfiguration Configuration;

        public IOptionsLocalizerBuilder Configure<TOptions>() where TOptions : class, new()
        {
            var configurator = new OptionsConfigurator<TOptions>(this);
            configurator.BindCultureSections();
            configurator.AddOptionsLocalizer();
            return this;
        }

        private class OptionsConfigurator<TOptions> where TOptions : class, new()
        {
            private readonly CultureInfo defaultCulture;
            private readonly CultureInfo[] expectedCultures;
            private readonly IServiceCollection services;
            private readonly IConfigurationSection optionsSection;

            public OptionsConfigurator(OptionsLocalizerBuilder builder)
            {
                this.defaultCulture = builder.DefaultCulture;
                this.expectedCultures = builder.ExpectedCultures;
                this.services = builder.Services;
                this.optionsSection = builder.Configuration.GetSection($"{nameof(OptionsLocalization)}:{typeof(TOptions).Name}");
            }

            /// <summary>
            /// 绑定 Options 到各个文化信息的配置节点
            /// </summary>
            public void BindCultureSections()
            {
                var targetCultures = new HashSet<CultureInfo>();
                foreach (var cultureSection in this.optionsSection.GetChildren())
                {
                    if (OptionsLocalizer.TryGetCultureInfo(cultureSection.Key, out var culture))
                    {
                        if (targetCultures.Add(culture))
                        {
                            BindCultureSection(culture);
                        }
                    }
                }

                foreach (var culture in this.expectedCultures)
                {
                    if (targetCultures.Add(culture))
                    {
                        BindCultureSection(culture);
                    }
                }
            }

            /// <summary>
            /// 绑定 Options 到指定文化信息的节点
            /// </summary>
            /// <param name="culture"></param>
            private void BindCultureSection(CultureInfo culture)
            {
                var name = this.defaultCulture.Equals(culture) ? Options.DefaultName : culture.Name;
                var cultureSection = this.optionsSection.GetSection(culture.Name);
                this.services.Configure<TOptions>(name, cultureSection);
            }

            /// <summary>
            /// 注册 Options的 IOptionsLocalizer 服务
            /// </summary>
            public void AddOptionsLocalizer()
            {
                this.services.Configure<OptionsLocalizerOptions<TOptions>>(options =>
                {
                    options.DefaultCulture = this.defaultCulture;
                    options.ExpectedCultures = this.expectedCultures;
                });

                this.services.TryAddTransient<IOptionsFactory<TOptions>, CultureOptionsFactory<TOptions>>();
                this.services.TryAddSingleton<OptionsLocalizer<TOptions>>();
                this.services.TryAddSingleton<IOptionsLocalizer<TOptions>>(s => s.GetRequiredService<OptionsLocalizer<TOptions>>());
                this.services.TryAddTransient(s => s.GetRequiredService<OptionsLocalizer<TOptions>>().CurrentValue);
                this.services.AddSingleton<IOptionsLocalizer>(s => s.GetRequiredService<OptionsLocalizer<TOptions>>());
            }
        }
    }
}