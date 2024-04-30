// <copyright file="GetCountriesTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Controllers;
using MartialBase.API.ControllerUnitTests.TestControllerInstances;
using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Countries;
using MartialBase.API.TestTools.TestResources;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

namespace MartialBase.API.ControllerUnitTests.CountriesControllerTests
{
    public class GetCountriesTests
    {
        private ICountriesRepository _countriesRepository;
        private IMartialBaseUserHelper _martialBaseUserHelper;
        private IAzureUserHelper _azureUserHelper;
        private CountriesControllerInstance _countriesControllerInstance;
        private MartialBaseUser _testUser;

        private CountriesController CountriesController => _countriesControllerInstance.Instance;

        [SetUp]
        public void Setup()
        {
            _countriesRepository = Substitute.For<ICountriesRepository>();
            _martialBaseUserHelper = Substitute.For<IMartialBaseUserHelper>();
            _azureUserHelper = Substitute.For<IAzureUserHelper>();

            _testUser =
                DataGenerator.MartialBaseUsers.GenerateMartialBaseUserObject();

            _countriesControllerInstance =
                new CountriesControllerInstance(
                    _countriesRepository,
                    _martialBaseUserHelper,
                    _azureUserHelper,
                    "Live",
                    _testUser);

            _countriesControllerInstance.SetTestUserRole(UserRoles.User);

            CountryResources.ClearUsedCountries();
        }

        [Test]
        public void CanGetCountries()
        {
            // Arrange
            var testCountries = new List<CountryDTO>();

            for (int i = 0; i < 100; i++)
            {
                testCountries.Add(DataGenerator.Countries.GenerateCountryDTO());
            }

            _countriesRepository
                .GetAll()
                .ReturnsForAnyArgs(testCountries);

            // Act
            IActionResult result = CountriesController.GetCountries();
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<List<CountryDTO>>());

            var retrievedCountries = (List<CountryDTO>)objectResult.Value;

            CountryResources.AssertEqual(testCountries, retrievedCountries);
        }

        [Test]
        public void CanGetCountriesWithPopulationLimit()
        {
            // Arrange
            var testCountriesUnlimited = new List<CountryDTO>();
            var testCountriesLimited = new List<CountryDTO>();

            for (int i = 0; i < 100; i++)
            {
                testCountriesUnlimited.Add(DataGenerator.Countries.GenerateCountryDTO());
            }

            for (int i = 0; i < 100; i++)
            {
                testCountriesLimited.Add(DataGenerator.Countries.GenerateCountryDTO());
            }

            _countriesRepository
                .GetAll()
                .Returns(testCountriesUnlimited);

            _countriesRepository
                .GetAll(1000000)
                .Returns(testCountriesLimited);

            // Act
            IActionResult result = CountriesController.GetCountries();
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<List<CountryDTO>>());

            var retrievedCountries = (List<CountryDTO>)objectResult.Value;

            CountryResources.AssertEqual(testCountriesLimited, retrievedCountries);
        }
    }
}
