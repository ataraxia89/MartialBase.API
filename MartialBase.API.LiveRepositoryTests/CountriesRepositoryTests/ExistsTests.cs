// <copyright file="ExistsTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;

using NUnit.Framework;

namespace MartialBase.API.LiveRepositoryTests.CountriesRepositoryTests
{
    public class ExistsTests : BaseTestClass
    {
        [Test]
        public void CanCheckCountryExists()
        {
            List<Country> countries = Countries.GetAllCountries();

            foreach (Country country in countries)
            {
                Assert.IsTrue(CountriesRepository.Exists(country.Code));
            }
        }
    }
}
