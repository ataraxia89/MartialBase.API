// <copyright file="ChangeOrganisationAddressTests.cs" company="Martialtech®">
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
using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Exceptions;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.TestResources;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.ControllerUnitTests.OrganisationsControllerTests
{
    public class ChangeOrganisationAddressTests
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
        private Guid _testOrganisationId;
        private CreateAddressDTO _testCreateAddressDTO;

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

            _testOrganisationId = Guid.NewGuid();
            _testCreateAddressDTO = DataGenerator.Addresses.GenerateCreateAddressDTO();
        }

        [Test]
        public async Task ChangeOrganisationAddressWithInvalidDTOReturnsInternalServerErrorWithDetails()
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
                await OrganisationsController.ChangeOrganisationAddressAsync(
                    _testOrganisationId.ToString(),
                    _testCreateAddressDTO);
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
            Assert.That(errorResult.Value, Is.TypeOf<Dictionary<string, string[]>>());

            DictionaryResources.AssertEqual(expectedModelStateErrors, (Dictionary<string, string[]>)errorResult.Value);
        }

        [TestCase(ErrorResponseCode.InsufficientUserRole)]
        [TestCase(ErrorResponseCode.AzureUserNotRegistered)]
        [TestCase(ErrorResponseCode.NotOrganisationAdmin)]
        public async Task ChangeOrganisationAddressReturnsForbiddenWithErrorResponseCode(ErrorResponseCode errorResponseCode)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _organisationsControllerInstance
                .OrganisationAdminAccessReturnsForbidden(_testOrganisationId, errorResponseCode);

            // Act
            try
            {
                await OrganisationsController.ChangeOrganisationAddressAsync(
                    _testOrganisationId.ToString(),
                    _testCreateAddressDTO);
            }
            catch (ErrorResponseCodeException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual(errorResponseCode, caughtException.ErrorResponseCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, caughtException.StatusCode);
        }

        [Test]
        public async Task ChangeOrganisationAddressForNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _organisationsControllerInstance
                .OrganisationAdminAccessThrowsNotFoundException(_testOrganisationId);

            // Act
            try
            {
                await OrganisationsController.ChangeOrganisationAddressAsync(
                    _testOrganisationId.ToString(),
                    _testCreateAddressDTO);
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Organisation ID '{_testOrganisationId}' not found.", caughtException.Message);
        }

        [Test]
        public async Task ChangeOrganisationAddressWithNonExistentAddressCountryReturnsNotFound()
        {
            // Arrange
            EntityNotFoundException caughtException = null;

            _countriesRepository
                .Exists(_testCreateAddressDTO.CountryCode)
                .Returns(false);

            // Act
            try
            {
                await OrganisationsController.ChangeOrganisationAddressAsync(
                    _testOrganisationId.ToString(),
                    _testCreateAddressDTO);
            }
            catch (EntityNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Country code '{_testCreateAddressDTO.CountryCode}' not found.", caughtException.Message);
        }

        [Test]
        public async Task OrganisationAdminUserCanChangeOrganisationAddress()
        {
            // Arrange
            AddressDTO testAddress = DataGenerator.Addresses.GenerateAddressDTO();

            _countriesRepository
                .Exists(_testCreateAddressDTO.CountryCode)
                .Returns(true);

            _organisationsRepository
                .ChangeOrganisationAddressAsync(_testOrganisationId, _testCreateAddressDTO)
                .Returns(testAddress);

            _organisationsRepository
                .SaveChangesAsync()
                .Returns(true);

            _organisationsControllerInstance.SetTestUserRole(UserRoles.OrganisationAdmin);

            // Act
            IActionResult result =
                await OrganisationsController.ChangeOrganisationAddressAsync(
                    _testOrganisationId.ToString(),
                    _testCreateAddressDTO);
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<AddressDTO>());

            var createdAddress = (AddressDTO)objectResult.Value;

            AddressResources.AssertEqual(testAddress, createdAddress);
        }
    }
}
