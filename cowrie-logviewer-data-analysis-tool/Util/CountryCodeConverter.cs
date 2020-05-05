using System;
using System.Globalization;
using System.Linq;

namespace cowrie_logviewer_data_analysis_tool.Util
{
    public static class CountryCodeConverter
    {
        public static string ConvertThreeLetterNameToTwoLetterName(string twoLetterCountryCode)
        {
            if (twoLetterCountryCode == null || twoLetterCountryCode.Length != 2)
            {
                throw new ArgumentException("name must be three letters.");
            }

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            foreach (CultureInfo culture in cultures)
            {
                RegionInfo region = new RegionInfo(culture.LCID);
                if (region.TwoLetterISORegionName.ToUpper() == twoLetterCountryCode.ToUpper())
                {
                    return region.ThreeLetterISORegionName;
                }
            }

            throw new ArgumentException("Could not get country code");
        }

        public static CultureInfo FromISOName(string name)
        {
            return CultureInfo
                .GetCultures(CultureTypes.NeutralCultures)
                .FirstOrDefault(c => c.ThreeLetterISOLanguageName == name);
        }
    }
}
