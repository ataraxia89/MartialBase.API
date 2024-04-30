// <copyright file="GetAllTests.cs" company="Martialtech®">
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
    public class GetAllTests : BaseTestClass
    {
        [Test]
        public void CanGetAllCountries()
        {
            List<Country> countries = Countries.GetAllCountries();

            List<CountryDTO> retrievedCountries = CountriesRepository.GetAll();

            CountryResources.AssertEqual(countries, retrievedCountries);
        }

        [Test]
        public void CanGetAllCountriesWithPopulationLimit()
        {
            long populationLimit = 1000000;

            List<Country> countries = Countries.GetAllCountries(populationLimit);

            List<CountryDTO> retrievedCountries = CountriesRepository.GetAll(populationLimit);

            CountryResources.AssertEqual(countries, retrievedCountries);
        }
    }
}
