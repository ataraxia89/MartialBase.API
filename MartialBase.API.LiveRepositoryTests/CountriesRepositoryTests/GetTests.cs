// <copyright file="GetTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Countries;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

namespace MartialBase.API.LiveRepositoryTests.CountriesRepositoryTests
{
    public class GetTests : BaseTestClass
    {
        [Test]
        public void CanGetCountry()
        {
            List<Country> countries = Countries.GetAllCountries();

            foreach (Country country in countries)
            {
                CountryDTO retrievedCountry = CountriesRepository.Get(country.Code);

                CountryResources.AssertEqual(country, retrievedCountry);
            }
        }
    }
}
