using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrazyStorm_Player
{
    public sealed class LocaleHelper
    {
        [DllImport("kernel32.dll")]
        private static extern ushort GetUserDefaultUILanguage();

        public static CultureInfo GetSystemCulture()
        {
            var languageId = GetUserDefaultUILanguage();
            if (languageId == 0) return CultureInfo.CurrentUICulture;
            return CultureInfo.GetCultureInfo(languageId);
        }
    }
}
