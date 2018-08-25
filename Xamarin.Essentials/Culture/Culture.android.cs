﻿using System;
using System.Globalization;
using System.Threading;

namespace Xamarin.Essentials
{
    public static partial class Culture
    {
        static string PlatformInstalledUICulture =>
            Java.Util.Locale.Default.ToString();

        static void PlatformSetCurrentUICulture(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        static CultureInfo PlatformGetCurrentUICulture(Func<string, CultureInfo> mappingOverride)
        {
            var netLanguage = ToDotnetLanguage(InstalledUICulture.Replace("_", "-"));

            // this gets called a lot - try/catch can be expensive so consider caching or something
            CultureInfo ci = null;
            try
            {
                ci = new CultureInfo(netLanguage);
            }
            catch (CultureNotFoundException)
            {
                if (mappingOverride != null)
                {
                    return mappingOverride(InstalledUICulture);
                }

                // locale not valid .NET culture (eg. "en-ES" : English in Spain)
                // fallback to first characters, in this case "en"
                try
                {
                    var fallback = ToDotnetFallbackLanguage(new InternalCulture(netLanguage));
                    ci = new CultureInfo(fallback);
                }
                catch (CultureNotFoundException)
                {
                    // language not valid .NET culture, falling back to English
                    ci = new CultureInfo("en");
                }
            }
            return ci;
        }

        static string ToDotnetLanguage(string androidLanguage)
        {
            var netLanguage = androidLanguage;

            // certain languages need to be converted to CultureInfo equivalent
            switch (androidLanguage)
            {
                case "ms-BN": // "Malaysian (Brunei)" not supported .NET culture
                case "ms-MY": // "Malaysian (Malaysia)" not supported .NET culture
                case "ms-SG": // "Malaysian (Singapore)" not supported .NET culture
                    netLanguage = "ms"; // closest supported
                    break;
                case "in-ID": // "Indonesian (Indonesia)" has different code in  .NET
                    netLanguage = "id-ID"; // correct code for .NET
                    break;
                case "gsw-CH": // "Schwiizertüütsch (Swiss German)" not supported .NET culture
                    netLanguage = "de-CH"; // closest supported
                    break;
                case "iw-IL": // Hebrew
                    netLanguage = "he-IL";
                    break;

                    // add more application-specific cases here (if required)
                    // ONLY use cultures that have been tested and known to work
            }
            return netLanguage;
        }

        static string ToDotnetFallbackLanguage(InternalCulture platCulture)
        {
            var netLanguage = platCulture.LanguageCode; // use the first part of the identifier (two chars, usually);
            switch (platCulture.LanguageCode)
            {
                case "gsw":
                    netLanguage = "de-CH"; // equivalent to German (Switzerland) for this app
                    break;
                case "iw":
                    netLanguage = "he"; // Hebrew
                    break;

                    // add more application-specific cases here (if required)
                    // ONLY use cultures that have been tested and known to work
            }
            return netLanguage;
        }
    }
}
