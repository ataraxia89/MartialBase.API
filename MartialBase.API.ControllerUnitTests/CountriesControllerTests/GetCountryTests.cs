// <copyright file="GetCountryTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Controllers;
using MartialBase.API.ControllerUnitTests.TestControllerInstances;
using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Exceptions.EntityFramework;
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
    public class GetCountryTests
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
        public void CanGetCountry()
        {
            // Arrange
            CountryDTO testCountry = DataGenerator.Countries.GenerateCountryDTO();

            _countriesRepository
                .Exists(testCountry.Code)
                .Returns(true);

            _countriesRepository
                .Get(testCountry.Code)
                .Returns(testCountry);

            // Act
            IActionResult result = CountriesController.GetCountry(testCountry.Code);
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<CountryDTO>());

            var retrievedCountry = (CountryDTO)objectResult.Value;

            CountryResources.AssertEqual(testCountry, retrievedCountry);
        }

        [Test]
        public void GetNonExistentCountryReturnsNotFoundResult()
        {
            // Arrange
            EntityNotFoundException caughtException = null;

            string invalidCountryCode = "XXX";

            _countriesRepository
                .Exists(invalidCountryCode)
                .Returns(false);

            // Act
            try
            {
                CountriesController.GetCountry(invalidCountryCode);
            }
            catch (EntityNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Country code '{invalidCountryCode}' not found.", caughtException.Message);
        }
    }
}
