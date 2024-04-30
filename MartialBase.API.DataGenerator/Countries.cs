// <copyright file="CountryResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Countries;
using MartialBase.API.Tools;

namespace MartialBase.API.DataGenerator
{
    public static class Countries
    {
        private static readonly List<Country> CountryList;
        private static readonly List<string> usedCountryCodes;

        static Countries()
        {
            CountryList = Data.Collections.Countries.GetAllCountries();
            usedCountryCodes = new List<string>();
        }

        public static Country GetRandomCountry()
        {
            // In case you run out of countries, remove the first one used
            if (usedCountryCodes.Count == CountryList.Count)
            {
                usedCountryCodes.RemoveAt(0);
            }

            var countries = CountryList.Where(c => !usedCountryCodes.Contains(c.Code)).ToList();

            int randomCountryNo = RandomData.GetRandomNumber(0, countries.Count - 1);

            Country country = countries[randomCountryNo];

            usedCountryCodes.Add(country.Code);

            return country;
        }

        public static CountryDTO GenerateCountryDTO(Country? country = null)
        {
            country ??= GetRandomCountry();

            return new CountryDTO
            {
                Code = country.Code,
                Name = country.Name
            };
        }
    }
}
