// <copyright file="CountryResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Countries;
using MartialBase.API.Tools;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class CountryResources
    {
        private static readonly List<Country> CountryList;
        private static List<string> usedCountryCodes;

        static CountryResources()
        {
            CountryList = Countries.GetAllCountries();
            usedCountryCodes = new List<string>();
        }

        internal static void ClearUsedCountries() => usedCountryCodes = new List<string>();

        internal static void AssertDoesNotExist(string countryCode) => Assert.False(Countries.Exists(countryCode));

        internal static Country GetRandomCountry()
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

        internal static void AssertEqual(Country expected, CountryDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Code, actual.Code);
            Assert.Equal(expected.Name, actual.Name);
        }

        internal static void AssertEqual(CountryDTO expected, CountryDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Code, actual.Code);
            Assert.Equal(expected.Name, actual.Name);
        }

        internal static void AssertEqual(List<Country> expected, List<CountryDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (Country expectedCountry in expected)
            {
                CountryDTO actualCountry = actual.FirstOrDefault(c => c.Code == expectedCountry.Code);

                AssertEqual(expectedCountry, actualCountry);
            }
        }

        internal static void AssertEqual(List<CountryDTO> expected, List<CountryDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (CountryDTO expectedCountry in expected)
            {
                CountryDTO actualCountry = actual.FirstOrDefault(c => c.Code == expectedCountry.Code);

                AssertEqual(expectedCountry, actualCountry);
            }
        }
    }
}