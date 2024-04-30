// <copyright file="RemoveOrganisationPersonTests.cs" company="Martialtech®">
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
    public class RemoveOrganisationPersonTests
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
        private Guid _testPersonId;

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
            _testPersonId = Guid.NewGuid();
        }

        [TestCase(ErrorResponseCode.InsufficientUserRole)]
        [TestCase(ErrorResponseCode.AzureUserNotRegistered)]
        [TestCase(ErrorResponseCode.NotOrganisationAdmin)]
        public async Task RemoveOrganisationPersonReturnsForbiddenWithErrorResponseCode(ErrorResponseCode errorResponseCode)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _organisationsControllerInstance
                .OrganisationAdminAccessReturnsForbidden(_testOrganisationId, errorResponseCode);

            // Act
            try
            {
                await OrganisationsController.RemoveOrganisationPersonAsync(
                    _testOrganisationId.ToString(),
                    _testPersonId.ToString());
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
        public async Task RemoveOrganisationPersonFromNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _organisationsControllerInstance.OrganisationAdminAccessThrowsNotFoundException(_testOrganisationId);

            _peopleRepository
                .ExistsAsync(_testPersonId)
                .Returns(true);

            // Act
            try
            {
                await OrganisationsController.RemoveOrganisationPersonAsync(
                    _testOrganisationId.ToString(),
                    _testPersonId.ToString());
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
        public async Task RemoveNonExistentPersonFromOrganisationReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;
            var invalidPersonId = Guid.NewGuid();

            _peopleRepository
                .ExistsAsync(invalidPersonId)
                .Returns(false);

            // Act
            try
            {
                await OrganisationsController.RemoveOrganisationPersonAsync(
                    _testOrganisationId.ToString(),
                    invalidPersonId.ToString());
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Person ID '{invalidPersonId}' not found.", caughtException.Message);
        }

        [Test]
        public async Task RemoveOrganisationPersonWhenPersonWouldBeOrphanEntityReturnsBadRequest()
        {
            // Arrange
            OrphanPersonEntityException caughtException = null;
            var testPersonId = Guid.NewGuid();

            _peopleRepository
                .ExistsAsync(testPersonId)
                .Returns(true);

            _organisationsRepository
                .When(x => x.RemoveOrganisationPersonAsync(_testOrganisationId, testPersonId))
                .Do(_ => throw new OrphanPersonEntityException());

            // Act
            try
            {
                await OrganisationsController.RemoveOrganisationPersonAsync(
                    _testOrganisationId.ToString(),
                    testPersonId.ToString());
            }
            catch (OrphanPersonEntityException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
        }

        [Test]
        public async Task OrganisationAdminCanRemoveOrganisationPerson()
        {
            // Arrange
            var testPersonId = Guid.NewGuid();

            _peopleRepository
                .ExistsAsync(testPersonId)
                .Returns(true);

            _organisationsRepository
                .SaveChangesAsync()
                .Returns(true);

            // Act
            IActionResult result = await OrganisationsController.RemoveOrganisationPersonAsync(
                    _testOrganisationId.ToString(), testPersonId.ToString());
            var statusCodeResult = result as StatusCodeResult;

            // Assert
            Assert.NotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status204NoContent, statusCodeResult.StatusCode);
            Assert.IsNull(result as ObjectResult);
        }
    }
}
