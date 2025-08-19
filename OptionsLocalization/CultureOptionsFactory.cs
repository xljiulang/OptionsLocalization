using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OptionsLocalization
{
    sealed class CultureOptionsFactory<TOptions> : IOptionsFactory<TOptions>
         where TOptions : class, new()
    {
        private readonly IConfigureOptions<TOptions>[] _setups;
        private readonly IPostConfigureOptions<TOptions>[] _postConfigures;
        private readonly IValidateOptions<TOptions>[] _validations;

        public CultureOptionsFactory(IEnumerable<IConfigureOptions<TOptions>> setups, IEnumerable<IPostConfigureOptions<TOptions>> postConfigures, IEnumerable<IValidateOptions<TOptions>> validations)
        {
            _setups = setups as IConfigureOptions<TOptions>[] ?? setups.ToArray();
            _postConfigures = postConfigures as IPostConfigureOptions<TOptions>[] ?? postConfigures.ToArray();
            _validations = validations as IValidateOptions<TOptions>[] ?? validations.ToArray();
        }


        public TOptions Create(string name)
        {
            var defaultOptions = this.CreateOptions(Options.DefaultName, default);
            if (string.IsNullOrEmpty(name))
            {
                return defaultOptions;
            }

            var culture = CultureInfo.GetCultureInfo(name);
            var cultureStack = new Stack<CultureInfo>();

            cultureStack.Push(culture);
            while (culture.Parent.Name.AsSpan().Length > 0)
            {
                culture = culture.Parent;
                cultureStack.Push(culture);
            }

            var options = defaultOptions;
            while (cultureStack.TryPop(out var next))
            {
                options = this.CreateOptions(next.Name, options);
            }

            return options;
        }


        private TOptions CreateOptions(string name, TOptions? options)
        {
            if (options == null)
            {
                options = new TOptions();
            }

            foreach (var setup in _setups)
            {
                if (setup is IConfigureNamedOptions<TOptions> namedSetup)
                {
                    namedSetup.Configure(name, options);
                }
                else if (name == Options.DefaultName)
                {
                    setup.Configure(options);
                }
            }

            foreach (var post in _postConfigures)
            {
                post.PostConfigure(name, options);
            }

            if (_validations != null)
            {
                var failures = new List<string>();
                foreach (var validate in _validations)
                {
                    var result = validate.Validate(name, options);
                    if (result != null && result.Failed)
                    {
                        failures.AddRange(result.Failures);
                    }
                }
                if (failures.Count > 0)
                {
                    throw new OptionsValidationException(name, typeof(TOptions), failures);
                }
            }

            return options;
        }
    }
}