using System;

namespace TDriver {
    internal class Region {
        public String Name;
        public String[] ZIPCODE_PREFIXES;
    }

    internal static class Regions {
        /// <summary>
        ///     Upstate New York Region
        /// </summary>
        /// <remarks>TODO Make this private?</remarks>
        public static Region UPSTATE = new Region() {
            Name = "UPSTATE",
            ZIPCODE_PREFIXES = new string[] {"12", "13", "14"}
        };

        /// <summary>
        ///     Finds the region by name.
        /// </summary>
        /// <param name="regionName"></param>
        /// <returns>Specified Region object or null.</returns>
        public static Region Region(String regionName) {
            switch (regionName) {
                case "UPSTATE":
                    return UPSTATE;
                default:
                    return null;
            }
        }
    }
}