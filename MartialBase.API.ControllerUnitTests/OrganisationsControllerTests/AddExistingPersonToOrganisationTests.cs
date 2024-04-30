// <copyright file="AddExistingPersonToOrganisationTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Net;

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Controllers;
using MartialBase.API.ControllerUnitTests.TestControllerInstances;
using MartialBase.API.Data.Exceptions;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.TestResources;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.ControllerUnitTests.OrganisationsControllerTests
{
    public class AddExistingPersonToOrganisationTests
    {
        private ICountriesRepository _countriesRepository;
        private IDocumentTypesRepository _documentTypesRepository;
        private IMartialBaseUserRolesRepository _martialBaseUserRolesRepository;
        private IMartialBaseUserHelper _martialBaseUserHelper;
        private IOrganisationsRepository _organisationsRepository;
        private IPeopleRepository _peopleRepository;
        private IAzureUserHelper _azureUserHelper;
        private OrganisationsControllerInstance _organisationsControllerInstance;
        private MartialBaseUser _testUser;
        private Guid _testOrganisationId;
        private Guid _testPersonId;

        private OrganisationsController OrganisationsController => _organisationsControllerInstance.Instance;

        [SetUp]
        public void Setup()
        {
            _countriesRepository = Substitute.For<ICountriesRepository>();
            _documentTypesRepository = Substitute.For<IDocumentTypesRepository>();
            _martialBaseUserRolesRepository = Substitute.For<IMartialBaseUserRolesRepository>();
            _martialBaseUserHelper = Substitute.For<IMartialBaseUserHelper>();
            _organisationsRepository = Substitute.For<IOrganisationsRepository>();
            _peopleRepository = Substitute.For<IPeopleRepository>();
            _azureUserHelper = Substitute.For<IAzureUserHelper>();

            _testUser =
                DataGenerator.MartialBaseUsers.GenerateMartialBaseUserObject();

            _organisationsControllerInstance =
                new OrganisationsControllerInstance(
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
            _testPersonId = Guid.NewGuid();
        }

        [TestCase(ErrorResponseCode.InsufficientUserRole)]
        [TestCase(ErrorResponseCode.AzureUserNotRegistered)]
        [TestCase(ErrorResponseCode.NotOrganisationAdmin)]
        public async Task AddExistingPersonToOrganisationReturnsForbiddenWithErrorResponseCode(ErrorResponseCode errorResponseCode)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _organisationsControllerInstance
                .OrganisationAdminAccessReturnsForbidden(_testOrganisationId, errorResponseCode);

            // Act
            try
            {
                await OrganisationsController.AddExistingPersonToOrganisationAsync(
                    _testOrganisationId.ToString(),
                    _testPersonId.ToString(),
                    "false");
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
        public async Task AddExistingPersonToOrganisationWithNoPersonIdProvidedReturnsBadRequest()
        {
            // Arrange
            // Act
            IActionResult result = await OrganisationsController
                .AddExistingPersonToOrganisationAsync(_testOrganisationId.ToString(), null, "false");
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, errorResult.StatusCode);
            Assert.AreEqual("No person ID parameter specified.", errorResult.Value);
        }

        [Test]
        public async Task AddExistingPersonToOrganisationWithInvalidIsAdminValueReturnsBadRequest()
        {
            // Arrange
            // Act
            IActionResult result = await OrganisationsController
                .AddExistingPersonToOrganisationAsync(
                    _testOrganisationId.ToString(), _testPersonId.ToString(), "not-a-bool");
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, errorResult.StatusCode);
            Assert.AreEqual("Invalid value provided for isAdmin parameter.", errorResult.Value);
        }

        [Test]
        public async Task AddExistingPersonToNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _organisationsControllerInstance
                .OrganisationAdminAccessThrowsNotFoundException(_testOrganisationId);

            // Act
            try
            {
                await OrganisationsController.AddExistingPersonToOrganisationAsync(
                    _testOrganisationId.ToString(),
                    _testPersonId.ToString(),
                    "false");
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
        public async Task AddNonExistentPersonToOrganisationReturnsNotFoundForNonExistentPerson()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _peopleRepository
                .ExistsAsync(_testPersonId)
                .Returns(false);

            // Act
            try
            {
                await OrganisationsController.AddExistingPersonToOrganisationAsync(
                    _testOrganisationId.ToString(),
                    _testPersonId.ToString(),
                    "false");
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Person ID '{_testPersonId}' not found.", caughtException.Message);
        }

        [Test]
        public async Task OrganisationAdminCanAddExistingPersonToOrganisation()
        {
            // Arrange
            _peopleRepository
                .ExistsAsync(_testPersonId)
                .Returns(true);

            _organisationsRepository
                .SaveChangesAsync()
                .Returns(true);

            // Act
            IActionResult result = await OrganisationsController.AddExistingPersonToOrganisationAsync(
                _testOrganisationId.ToString(),
                _testPersonId.ToString(),
                "false");
            var statusCodeResult = result as StatusCodeResult;

            // Assert
            Assert.NotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status201Created, statusCodeResult.StatusCode);
            Assert.IsNull(result as ObjectResult);
        }
    }
}
