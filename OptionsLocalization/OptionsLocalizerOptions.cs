using System.Collections.Generic;
using System.Globalization;

namespace OptionsLocalization
{
    sealed class OptionsLocalizerOptions<TOptions>
    {
        public CultureInfo DefaultCulture { get; set; } = CultureInfo.CurrentCulture;
        public CultureInfo[] ExpectedCultures { get; set; } = [];
        public HashSet<CultureInfo> SupportedCultures { get; set; } = [];
    }
}
