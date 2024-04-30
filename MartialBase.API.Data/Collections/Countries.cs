// <copyright file="Countries.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Countries;

namespace MartialBase.API.Data.Collections
{
    internal static class Countries
    {
        private static readonly List<Country> CountryList = InitialiseCountries();

        internal static bool Exists(string countryCode) => CountryList.Exists(c => c.Code == countryCode);

        internal static List<CountryDTO> GetAllCountryDTOs(long? populationLimit = null)
        {
            var returnCountries = new List<CountryDTO>();

            IEnumerable<Country> countries = populationLimit == null
                ? CountryList
                : CountryList.Where(c => c.Population >= populationLimit);

            foreach (Country country in countries.OrderBy(c => c.Name))
            {
                returnCountries.Add(ModelMapper.GetCountryDTO(country));
            }

            return returnCountries;
        }

        internal static CountryDTO GetCountryDTO(string countryCode)
        {
            Country country = CountryList.First(c => c.Code == countryCode);

            return ModelMapper.GetCountryDTO(country);
        }

        internal static List<Country> GetAllCountries(long? populationLimit = null)
        {
            IEnumerable<Country> countries = populationLimit == null
                ? CountryList
                : CountryList.Where(c => c.Population >= populationLimit);

            return countries.OrderBy(c => c.Name).ToList();
        }

        internal static Country GetCountry(string countryCode) => CountryList.First(c => c.Code == countryCode);

#pragma warning disable S109 // Magic numbers should not be used (These aren't magic numbers!)
        private static List<Country> InitialiseCountries() => new List<Country>
            {
                new Country { Code = "AFG", Name = "Afghanistan", Population = 38928346 },
                new Country { Code = "ALA", Name = "Åland Islands", Population = 28007 },
                new Country { Code = "ALB", Name = "Albania", Population = 2877797 },
                new Country { Code = "DZA", Name = "Algeria", Population = 43851044 },
                new Country { Code = "ASM", Name = "American Samoa", Population = 55191 },
                new Country { Code = "AND", Name = "Andorra", Population = 77265 },
                new Country { Code = "AGO", Name = "Angola", Population = 32866272 },
                new Country { Code = "AIA", Name = "Anguilla", Population = 15003 },
                new Country { Code = "ATA", Name = "Antarctica", Population = 1106 },
                new Country { Code = "ATG", Name = "Antigua and Barbuda", Population = 97929 },
                new Country { Code = "ARG", Name = "Argentina", Population = 45195774 },
                new Country { Code = "ARM", Name = "Armenia", Population = 2963243 },
                new Country { Code = "ABW", Name = "Aruba", Population = 106766 },
                new Country { Code = "AUS", Name = "Australia", Population = 25499884 },
                new Country { Code = "AUT", Name = "Austria", Population = 9006398 },
                new Country { Code = "AZE", Name = "Azerbaijan", Population = 10139177 },
                new Country { Code = "BHS", Name = "The Bahamas", Population = 393244 },
                new Country { Code = "BHR", Name = "Bahrain", Population = 1701575 },
                new Country { Code = "BGD", Name = "Bangladesh", Population = 164689383 },
                new Country { Code = "BRB", Name = "Barbados", Population = 287375 },
                new Country { Code = "BLR", Name = "Belarus", Population = 9449323 },
                new Country { Code = "BEL", Name = "Belgium", Population = 11589623 },
                new Country { Code = "BLZ", Name = "Belize", Population = 397628 },
                new Country { Code = "BEN", Name = "Benin", Population = 12123200 },
                new Country { Code = "BMU", Name = "Bermuda", Population = 62278 },
                new Country { Code = "BTN", Name = "Bhutan", Population = 771608 },
                new Country { Code = "BOL", Name = "Bolivia", Population = 11673021 },
                new Country { Code = "BIH", Name = "Bosnia and Herzegovina", Population = 3280819 },
                new Country { Code = "BWA", Name = "Botswana", Population = 2351627 },
                new Country { Code = "BRA", Name = "Brazil", Population = 212559417 },
                new Country { Code = "BRN", Name = "Brunei", Population = 437479 },
                new Country { Code = "BGR", Name = "Bulgaria", Population = 6948445 },
                new Country { Code = "BFA", Name = "Burkina Faso", Population = 20903273 },
                new Country { Code = "BDI", Name = "Burundi", Population = 11890784 },
                new Country { Code = "CPV", Name = "Cabo Verde", Population = 555987 },
                new Country { Code = "KHM", Name = "Cambodia", Population = 16718965 },
                new Country { Code = "CMR", Name = "Cameroon", Population = 26545863 },
                new Country { Code = "CAN", Name = "Canada", Population = 37742154 },
                new Country { Code = "CYM", Name = "Cayman Islands", Population = 65722 },
                new Country { Code = "CAF", Name = "Central African Republic", Population = 4829767 },
                new Country { Code = "TCD", Name = "Chad", Population = 16425864 },
                new Country { Code = "CHL", Name = "Chile", Population = 19116201 },
                new Country { Code = "CHN", Name = "China", Population = 1439323776 },
                new Country { Code = "CXR", Name = "Christmas Island", Population = 1402 },
                new Country { Code = "CCK", Name = "The Cocos (Keeling) Islands", Population = 596 },
                new Country { Code = "COL", Name = "Colombia", Population = 50882891 },
                new Country { Code = "COM", Name = "Comoros", Population = 869601 },
                new Country { Code = "COD", Name = "Democratic Republic of the Congo", Population = 89433965 },
                new Country { Code = "COG", Name = "The Congo", Population = 5511778 },
                new Country { Code = "COK", Name = "Cook Islands", Population = 17564 },
                new Country { Code = "CRI", Name = "Costa Rica", Population = 5094118 },
                new Country { Code = "CIV", Name = "Côte d'Ivoire", Population = 26378274 },
                new Country { Code = "HRV", Name = "Croatia", Population = 4105267 },
                new Country { Code = "CUB", Name = "Cuba", Population = 11326616 },
                new Country { Code = "CUW", Name = "Curaçao", Population = 164093 },
                new Country { Code = "CYP", Name = "Cyprus", Population = 1207359 },
                new Country { Code = "CZE", Name = "Czech Republic", Population = 10708981 },
                new Country { Code = "DNK", Name = "Denmark", Population = 5792202 },
                new Country { Code = "DJI", Name = "Djibouti", Population = 988000 },
                new Country { Code = "DMA", Name = "Dominica", Population = 71986 },
                new Country { Code = "ECU", Name = "Ecuador", Population = 17643054 },
                new Country { Code = "EGY", Name = "Egypt", Population = 102334404 },
                new Country { Code = "SLV", Name = "El Salvador", Population = 6486205 },
                new Country { Code = "GNQ", Name = "Equatorial Guinea", Population = 1402985 },
                new Country { Code = "ERI", Name = "Eritrea", Population = 3546421 },
                new Country { Code = "EST", Name = "Estonia", Population = 1326535 },
                new Country { Code = "SWZ", Name = "Eswatini", Population = 1160164 },
                new Country { Code = "ETH", Name = "Ethiopia", Population = 114963588 },
                new Country { Code = "FLK", Name = "Falkland Islands", Population = 3480 },
                new Country { Code = "FRO", Name = "Faroe Islands", Population = 48863 },
                new Country { Code = "FJI", Name = "Fiji", Population = 896445 },
                new Country { Code = "FIN", Name = "Finland", Population = 5540720 },
                new Country { Code = "FRA", Name = "France", Population = 65273511 },
                new Country { Code = "GUF", Name = "French Guiana", Population = 298682 },
                new Country { Code = "PYF", Name = "French Polynesia", Population = 280908 },
                new Country { Code = "GAB", Name = "Gabon", Population = 2225734 },
                new Country { Code = "GMB", Name = "Gambia", Population = 2416668 },
                new Country { Code = "GEO", Name = "Georgia", Population = 3989167 },
                new Country { Code = "DEU", Name = "Germany", Population = 83783942 },
                new Country { Code = "GHA", Name = "Ghana", Population = 31072940 },
                new Country { Code = "GIB", Name = "Gibraltar", Population = 33691 },
                new Country { Code = "GRC", Name = "Greece", Population = 10423054 },
                new Country { Code = "GRL", Name = "Greenland", Population = 56770 },
                new Country { Code = "GRD", Name = "Grenada", Population = 112523 },
                new Country { Code = "GLP", Name = "Guadeloupe", Population = 400124 },
                new Country { Code = "GUM", Name = "Guam", Population = 168775 },
                new Country { Code = "GTM", Name = "Guatemala", Population = 17915568 },
                new Country { Code = "GGY", Name = "Guernsey", Population = 67052 },
                new Country { Code = "GIN", Name = "Guinea", Population = 13132795 },
                new Country { Code = "GNB", Name = "Guinea-Bissau", Population = 1968001 },
                new Country { Code = "GUY", Name = "Guyana", Population = 786552 },
                new Country { Code = "HTI", Name = "Haiti", Population = 11402528 },
                new Country { Code = "VAT", Name = "Holy See", Population = 801 },
                new Country { Code = "HND", Name = "Honduras", Population = 9904607 },
                new Country { Code = "HKG", Name = "Hong Kong", Population = 7496981 },
                new Country { Code = "HUN", Name = "Hungary", Population = 9660351 },
                new Country { Code = "ISL", Name = "Iceland", Population = 341243 },
                new Country { Code = "IND", Name = "India", Population = 1380004385 },
                new Country { Code = "IDN", Name = "Indonesia", Population = 273523615 },
                new Country { Code = "IRN", Name = "Iran", Population = 83992949 },
                new Country { Code = "IRQ", Name = "Iraq", Population = 40222493 },
                new Country { Code = "IRL", Name = "Ireland", Population = 4937786 },
                new Country { Code = "IMN", Name = "Isle of Man", Population = 85033 },
                new Country { Code = "ISR", Name = "Israel", Population = 8655535 },
                new Country { Code = "ITA", Name = "Italy", Population = 60461826 },
                new Country { Code = "JAM", Name = "Jamaica", Population = 2961167 },
                new Country { Code = "JPN", Name = "Japan", Population = 126476461 },
                new Country { Code = "JEY", Name = "Jersey", Population = 97857 },
                new Country { Code = "JOR", Name = "Jordan", Population = 10203134 },
                new Country { Code = "KAZ", Name = "Kazakhstan", Population = 18776707 },
                new Country { Code = "KEN", Name = "Kenya", Population = 53771296 },
                new Country { Code = "KIR", Name = "Kiribati", Population = 119449 },
                new Country { Code = "PRK", Name = "North Korea", Population = 25778816 },
                new Country { Code = "KOR", Name = "South Korea", Population = 51269185 },
                new Country { Code = "KWT", Name = "Kuwait", Population = 4270571 },
                new Country { Code = "KGZ", Name = "Kyrgyzstan", Population = 6524195 },
                new Country { Code = "LVA", Name = "Latvia", Population = 1886198 },
                new Country { Code = "LBN", Name = "Lebanon", Population = 6825445 },
                new Country { Code = "LSO", Name = "Lesotho", Population = 2142249 },
                new Country { Code = "LBR", Name = "Liberia", Population = 5057681 },
                new Country { Code = "LBY", Name = "Libya", Population = 6871292 },
                new Country { Code = "LIE", Name = "Liechtenstein", Population = 38128 },
                new Country { Code = "LTU", Name = "Lithuania", Population = 2722289 },
                new Country { Code = "LUX", Name = "Luxembourg", Population = 625978 },
                new Country { Code = "MAC", Name = "Macao", Population = 649335 },
                new Country { Code = "MKD", Name = "North Macedonia", Population = 2083374 },
                new Country { Code = "MDG", Name = "Madagascar", Population = 27691018 },
                new Country { Code = "MWI", Name = "Malawi", Population = 19129952 },
                new Country { Code = "MYS", Name = "Malaysia", Population = 32365999 },
                new Country { Code = "MDV", Name = "Maldives", Population = 540544 },
                new Country { Code = "MLI", Name = "Mali", Population = 20250833 },
                new Country { Code = "MLT", Name = "Malta", Population = 441543 },
                new Country { Code = "MHL", Name = "Marshall Islands", Population = 59190 },
                new Country { Code = "MTQ", Name = "Martinique", Population = 375265 },
                new Country { Code = "MRT", Name = "Mauritania", Population = 4649658 },
                new Country { Code = "MUS", Name = "Mauritius", Population = 1271768 },
                new Country { Code = "MYT", Name = "Mayotte", Population = 272815 },
                new Country { Code = "MEX", Name = "Mexico", Population = 128932753 },
                new Country { Code = "FSM", Name = "Micronesia", Population = 115023 },
                new Country { Code = "MDA", Name = "Moldova", Population = 4033963 },
                new Country { Code = "MCO", Name = "Monaco", Population = 39242 },
                new Country { Code = "MNG", Name = "Mongolia", Population = 3278290 },
                new Country { Code = "MNE", Name = "Montenegro", Population = 628066 },
                new Country { Code = "MSR", Name = "Montserrat", Population = 4992 },
                new Country { Code = "MAR", Name = "Morocco", Population = 36910560 },
                new Country { Code = "MOZ", Name = "Mozambique", Population = 31255435 },
                new Country { Code = "MMR", Name = "Myanmar", Population = 54409800 },
                new Country { Code = "NAM", Name = "Namibia", Population = 2540905 },
                new Country { Code = "NRU", Name = "Nauru", Population = 10824 },
                new Country { Code = "NPL", Name = "Nepal", Population = 29136808 },
                new Country { Code = "NLD", Name = "Netherlands", Population = 17134872 },
                new Country { Code = "NCL", Name = "New Caledonia", Population = 285498 },
                new Country { Code = "NZL", Name = "New Zealand", Population = 4822233 },
                new Country { Code = "NIC", Name = "Nicaragua", Population = 6624554 },
                new Country { Code = "NER", Name = "Niger", Population = 24206644 },
                new Country { Code = "NGA", Name = "Nigeria", Population = 206139589 },
                new Country { Code = "NIU", Name = "Niue", Population = 1626 },
                new Country { Code = "NFK", Name = "Norfolk Island", Population = 2169 },
                new Country { Code = "MNP", Name = "Northern Mariana Islands", Population = 57559 },
                new Country { Code = "NOR", Name = "Norway", Population = 5421241 },
                new Country { Code = "OMN", Name = "Oman", Population = 5106626 },
                new Country { Code = "PAK", Name = "Pakistan", Population = 220892340 },
                new Country { Code = "PLW", Name = "Palau", Population = 18094 },
                new Country { Code = "PSE", Name = "Palestine", Population = 5101414 },
                new Country { Code = "PAN", Name = "Panama", Population = 4314767 },
                new Country { Code = "PNG", Name = "Papua New Guinea", Population = 8947024 },
                new Country { Code = "PRY", Name = "Paraguay", Population = 7132538 },
                new Country { Code = "PER", Name = "Peru", Population = 32971854 },
                new Country { Code = "PHL", Name = "The Philippines", Population = 109581078 },
                new Country { Code = "PCN", Name = "Pitcairn", Population = 67 },
                new Country { Code = "POL", Name = "Poland", Population = 37846611 },
                new Country { Code = "PRT", Name = "Portugal", Population = 10196709 },
                new Country { Code = "PRI", Name = "Puerto Rico", Population = 2860853 },
                new Country { Code = "QAT", Name = "Qatar", Population = 2881053 },
                new Country { Code = "REU", Name = "Réunion", Population = 895312 },
                new Country { Code = "ROU", Name = "Romania", Population = 19237691 },
                new Country { Code = "RUS", Name = "Russia", Population = 145934462 },
                new Country { Code = "RWA", Name = "Rwanda", Population = 12952218 },
                new Country { Code = "BLM", Name = "Saint Barthélemy", Population = 9131 },
                new Country { Code = "KNA", Name = "Saint Kitts and Nevis", Population = 52441 },
                new Country { Code = "LCA", Name = "Saint Lucia", Population = 183627 },
                new Country { Code = "MAF", Name = "Saint Martin", Population = 38666 },
                new Country { Code = "SPM", Name = "Saint Pierre and Miquelon", Population = 5888 },
                new Country { Code = "VCT", Name = "St. Vincent & Grenadines", Population = 110940 },
                new Country { Code = "WSM", Name = "Samoa", Population = 198414 },
                new Country { Code = "SMR", Name = "San Marino", Population = 33931 },
                new Country { Code = "STP", Name = "Sao Tome & Principe", Population = 219159 },
                new Country { Code = "SAU", Name = "Saudi Arabia", Population = 34813871 },
                new Country { Code = "SEN", Name = "Senegal", Population = 16743927 },
                new Country { Code = "SRB", Name = "Serbia", Population = 8737371 },
                new Country { Code = "SYC", Name = "Seychelles", Population = 98347 },
                new Country { Code = "SLE", Name = "Sierra Leone", Population = 7976983 },
                new Country { Code = "SGP", Name = "Singapore", Population = 5850342 },
                new Country { Code = "SXM", Name = "Sint Maarten", Population = 42876 },
                new Country { Code = "SVK", Name = "Slovakia", Population = 5459642 },
                new Country { Code = "SVN", Name = "Slovenia", Population = 2078938 },
                new Country { Code = "SLB", Name = "Solomon Islands", Population = 686884 },
                new Country { Code = "SOM", Name = "Somalia", Population = 15893222 },
                new Country { Code = "ZAF", Name = "South Africa", Population = 59308690 },
                new Country { Code = "SSD", Name = "South Sudan", Population = 11193725 },
                new Country { Code = "ESP", Name = "Spain", Population = 46754778 },
                new Country { Code = "LKA", Name = "Sri Lanka", Population = 21413249 },
                new Country { Code = "SDN", Name = "Sudan", Population = 43849260 },
                new Country { Code = "SUR", Name = "Suriname", Population = 586632 },
                new Country { Code = "SWE", Name = "Sweden", Population = 10099265 },
                new Country { Code = "CHE", Name = "Switzerland", Population = 8654622 },
                new Country { Code = "SYR", Name = "Syria", Population = 17500658 },
                new Country { Code = "TWN", Name = "Taiwan", Population = 23816775 },
                new Country { Code = "TJK", Name = "Tajikistan", Population = 9537645 },
                new Country { Code = "TZA", Name = "Tanzania", Population = 59734218 },
                new Country { Code = "THA", Name = "Thailand", Population = 69799978 },
                new Country { Code = "TLS", Name = "Timor-Leste", Population = 1318445 },
                new Country { Code = "TGO", Name = "Togo", Population = 8278724 },
                new Country { Code = "TKL", Name = "Tokelau", Population = 1357 },
                new Country { Code = "TON", Name = "Tonga", Population = 105695 },
                new Country { Code = "TTO", Name = "Trinidad and Tobago", Population = 1399488 },
                new Country { Code = "TUN", Name = "Tunisia", Population = 11818619 },
                new Country { Code = "TUR", Name = "Turkey", Population = 84339067 },
                new Country { Code = "TKM", Name = "Turkmenistan", Population = 6031200 },
                new Country { Code = "TCA", Name = "Turks and Caicos Islands", Population = 38717 },
                new Country { Code = "TUV", Name = "Tuvalu", Population = 11792 },
                new Country { Code = "UGA", Name = "Uganda", Population = 45741007 },
                new Country { Code = "UKR", Name = "Ukraine", Population = 43733762 },
                new Country { Code = "ARE", Name = "United Arab Emirates", Population = 9890402 },
                new Country { Code = "GBR", Name = "United Kingdom", Population = 67886011 },
                new Country { Code = "UMI", Name = "United States Minor Outlying Islands", Population = 300 },
                new Country { Code = "USA", Name = "United States of America", Population = 331002651 },
                new Country { Code = "URY", Name = "Uruguay", Population = 3473730 },
                new Country { Code = "UZB", Name = "Uzbekistan", Population = 33469203 },
                new Country { Code = "VUT", Name = "Vanuatu", Population = 307145 },
                new Country { Code = "VEN", Name = "Venezuela", Population = 28435940 },
                new Country { Code = "VNM", Name = "Vietnam", Population = 97338579 },
                new Country { Code = "VGB", Name = "British Virgin Islands", Population = 30231 },
                new Country { Code = "VIR", Name = "U.S. Virgin Islands", Population = 106977 },
                new Country { Code = "WLF", Name = "Wallis & Futuna", Population = 11239 },
                new Country { Code = "ESH", Name = "Western Sahara", Population = 597339 },
                new Country { Code = "YEM", Name = "Yemen", Population = 29825964 },
                new Country { Code = "ZMB", Name = "Zambia", Population = 18383955 },
                new Country { Code = "ZWE", Name = "Zimbabwe", Population = 14862924 }
            };
#pragma warning restore S109 // Magic numbers should not be used
    }
}
