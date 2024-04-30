// <copyright file="CreateOrganisationWithNewPersonTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Controllers;
using MartialBase.API.ControllerUnitTests.TestControllerInstances;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.TestTools.TestResources;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.ControllerUnitTests.OrganisationsControllerTests
{
    public class CreateOrganisationWithNewPersonTests
    {
        private ICountriesRepository _countriesRepository;
        private IDocumentTypesRepository _documentTypesRepository;
        private IMartialBaseUserHelper _martialBaseUserHelper;
        private IMartialBaseUserRolesRepository _martialBaseUserRolesRepository;
        private IOrganisationsRepository _organisationsRepository;
        private IPeopleRepository _peopleRepository;
        private IAzureUserHelper _azureUserHelper;
        private OrganisationsControllerInstance _organisationsControllerInstance;
        private MartialBaseUser _testUser;
        private CreatePersonOrganisationDTO _testCreatePersonOrganisationDTO;

        private OrganisationsController OrganisationsController => _organisationsControllerInstance.Instance;

        [SetUp]
        public void Setup()
        {
            _countriesRepository = Substitute.For<ICountriesRepository>();
            _documentTypesRepository = Substitute.For<IDocumentTypesRepository>();
            _martialBaseUserHelper = Substitute.For<IMartialBaseUserHelper>();
            _martialBaseUserRolesRepository = Substitute.For<IMartialBaseUserRolesRepository>();
            _organisationsRepository = Substitute.For<IOrganisationsRepository>();
            _peopleRepository = Substitute.For<IPeopleRepository>();
            _azureUserHelper = Substitute.For<IAzureUserHelper>();

            _testUser =
                DataGenerator.MartialBaseUsers.GenerateMartialBaseUserObject();

            _organisationsControllerInstance = new OrganisationsControllerInstance(
                _countriesRepository,
                _documentTypesRepository,
                _martialBaseUserHelper,
                _martialBaseUserRolesRepository,
                _organisationsRepository,
                _peopleRepository,
                _azureUserHelper,
                "Live",
                _testUser);

            _testCreatePersonOrganisationDTO = DataGenerator.Organisations.GenerateCreatePersonOrganisationDTOObject();
        }

        [Test]
        public async Task CreatePersonOrganisationWithInvalidDTOReturnsInternalServerErrorWithDetails()
        {
            // Arrange
            var testErrors = new Dictionary<string, string>();
            var expectedModelStateErrors = new Dictionary<string, string[]>();

            for (int i = 0; i < 10; i++)
            {
                expectedModelStateErrors.Add(Guid.NewGuid().ToString(), new[] { Guid.NewGuid().ToString() });
            }

            foreach (KeyValuePair<string, string[]> expectedModelStateError in expectedModelStateErrors)
            {
                testErrors.Add(expectedModelStateError.Key, expectedModelStateError.Value[0]);
            }

            foreach (KeyValuePair<string, string> testError in testErrors)
            {
                OrganisationsController.ModelState.AddModelError(testError.Key, testError.Value);
            }

            // Act
            IActionResult result =
                await OrganisationsController.CreateOrganisationWithNewPersonAsync(_testCreatePersonOrganisationDTO);
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
            Assert.That(errorResult.Value, Is.TypeOf<Dictionary<string, string[]>>());

            DictionaryResources.AssertEqual(expectedModelStateErrors, (Dictionary<string, string[]>)errorResult.Value);
        }

        [Test]
        public async Task CreatePersonOrganisationWithNonExistentOrganisationAddressCountryReturnsNotFound()
        {
            // Arrange
            EntityNotFoundException caughtException = null;

            _countriesRepository
                .Exists(_testCreatePersonOrganisationDTO.Person.Address.CountryCode)
                .Returns(true);

            _countriesRepository
                .Exists(_testCreatePersonOrganisationDTO.Organisation.Address.CountryCode)
                .Returns(false);

            // Act
            try
            {
                await OrganisationsController.CreateOrganisationWithNewPersonAsync(_testCreatePersonOrganisationDTO);
            }
            catch (EntityNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual(
                $"Country code '{_testCreatePersonOrganisationDTO.Organisation.Address.CountryCode}' not found.",
                caughtException.Message);
        }

        [Test]
        public async Task CreatePersonOrganisationWithNonExistentPersonAddressCountryReturnsNotFound()
        {
            // Arrange
            EntityNotFoundException caughtException = null;

            _countriesRepository
                .Exists(_testCreatePersonOrganisationDTO.Person.Address.CountryCode)
                .Returns(false);

            _countriesRepository
                .Exists(_testCreatePersonOrganisationDTO.Organisation.Address.CountryCode)
                .Returns(true);

            // Act
            try
            {
                await OrganisationsController.CreateOrganisationWithNewPersonAsync(_testCreatePersonOrganisationDTO);
            }
            catch (EntityNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual(
                $"Country code '{_testCreatePersonOrganisationDTO.Person.Address.CountryCode}' not found.",
                caughtException.Message);
        }
    }
}
