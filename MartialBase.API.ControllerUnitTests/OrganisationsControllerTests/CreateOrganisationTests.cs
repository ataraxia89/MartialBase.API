// <copyright file="CreateOrganisationTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Controllers;
using MartialBase.API.ControllerUnitTests.TestControllerInstances;
using MartialBase.API.Data.Exceptions;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Organisations;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.TestResources;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.ControllerUnitTests.OrganisationsControllerTests
{
    public class CreateOrganisationTests
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
        private CreateOrganisationDTO _testCreateOrganisationDTO;
        private CreateOrganisationInternalDTO _testCreateOrganisationInternalDTO;

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

            _testCreateOrganisationDTO = DataGenerator.Organisations.GenerateCreateOrganisationDTOObject();
            _testCreateOrganisationInternalDTO =
                DataGenerator.Organisations.GenerateCreateOrganisationInternalDTOObject();
        }

        [Test]
        public async Task CreateOrganisationWithInvalidDTOReturnsInternalServerErrorWithDetails()
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
            IActionResult result = await OrganisationsController.CreateOrganisationAsync(_testCreateOrganisationDTO);
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
            Assert.That(errorResult.Value, Is.TypeOf<Dictionary<string, string[]>>());

            DictionaryResources.AssertEqual(expectedModelStateErrors, (Dictionary<string, string[]>)errorResult.Value);
        }

        [Test]
        public async Task CreateOrganisationUnderNonExistentParentReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;
            var invalidParentId = Guid.NewGuid();

            _testCreateOrganisationDTO.ParentId = invalidParentId.ToString();

            _organisationsControllerInstance.OrganisationAdminAccessThrowsNotFoundException(invalidParentId);

            // Act
            try
            {
                await OrganisationsController.CreateOrganisationAsync(_testCreateOrganisationDTO);
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Organisation ID '{invalidParentId}' not found.", caughtException.Message);
        }

        [Test]
        public async Task CreateOrganisationUnderParentForWhichRequestingUserIsNotAdminReturnsForbidden()
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            var testParentId = Guid.NewGuid();

            _testCreateOrganisationDTO.ParentId = testParentId.ToString();

            _organisationsControllerInstance
                .OrganisationAdminAccessReturnsForbidden(testParentId, ErrorResponseCode.NotOrganisationAdmin);

            // Act
            try
            {
                await OrganisationsController.CreateOrganisationAsync(_testCreateOrganisationDTO);
            }
            catch (ErrorResponseCodeException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual(ErrorResponseCode.NotOrganisationAdmin, caughtException.ErrorResponseCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, caughtException.StatusCode);
        }

        [Test]
        public async Task CreateOrganisationWithNonExistentAddressCountryReturnsNotFound()
        {
            // Arrange
            EntityNotFoundException caughtException = null;
            string invalidCountryCode = "XXX";

            _testCreateOrganisationDTO.Address.CountryCode = invalidCountryCode;

            _countriesRepository
                .Exists(invalidCountryCode)
                .Returns(false);

            // Act
            try
            {
                await OrganisationsController.CreateOrganisationAsync(_testCreateOrganisationDTO);
            }
            catch (EntityNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Country code '{invalidCountryCode}' not found.", caughtException.Message);
        }

        [Test]
        public async Task BasicUserCanCreateOrganisationWithNoParent()
        {
            // Arrange
            OrganisationDTO testOrganisation = DataGenerator.Organisations.GenerateOrganisationDTO();

            _countriesRepository
                .Exists(_testCreateOrganisationInternalDTO.Address.CountryCode)
                .Returns(true);

            _organisationsRepository
                .CreateAsync(_testCreateOrganisationInternalDTO)
                .ReturnsForAnyArgs(testOrganisation);

            _organisationsRepository
                .SaveChangesAsync()
                .Returns(true);

            // Act
            // TODO: THIS SHOULDN'T BE USING AN INTERNAL DTO
            IActionResult result = await OrganisationsController.CreateOrganisationAsync(_testCreateOrganisationInternalDTO);
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<OrganisationDTO>());

            var createdOrganisation = (OrganisationDTO)objectResult.Value;

            OrganisationResources.AssertEqual(testOrganisation, createdOrganisation);
        }

        [Test]
        public async Task BasicUserCanCreateOrganisationWithParentForWhichTheyAreAdmin()
        {
            // Arrange
            var testParentId = Guid.NewGuid();

            OrganisationDTO testOrganisation =
                DataGenerator.Organisations.GenerateOrganisationDTO(testParentId.ToString());

            _countriesRepository
                .Exists(_testCreateOrganisationInternalDTO.Address.CountryCode)
                .Returns(true);

            _organisationsRepository
                .CreateAsync(_testCreateOrganisationInternalDTO)
                .ReturnsForAnyArgs(testOrganisation);

            _organisationsRepository
                .SaveChangesAsync()
                .Returns(true);

            // Act
            // TODO: THIS SHOULDN'T BE USING AN INTERNAL DTO
            IActionResult result = await OrganisationsController.CreateOrganisationAsync(_testCreateOrganisationInternalDTO);
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<OrganisationDTO>());

            var createdOrganisation = (OrganisationDTO)objectResult.Value;

            OrganisationResources.AssertEqual(testOrganisation, createdOrganisation);
        }
    }
}
