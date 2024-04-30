// <copyright file="ChangeOrganisationParentTests.cs" company="Martialtech®">
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
    public class ChangeOrganisationParentTests
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
        private Guid _testOldParentId;
        private Guid _testOrganisationId;
        private Guid _testNewParentId;

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

            _testOldParentId = Guid.NewGuid();
            _testOrganisationId = Guid.NewGuid();
            _testNewParentId = Guid.NewGuid();
        }

        [TestCase(ErrorResponseCode.InsufficientUserRole)]
        [TestCase(ErrorResponseCode.AzureUserNotRegistered)]
        [TestCase(ErrorResponseCode.NotOrganisationAdmin)]
        public async Task ChangeOrganisationParentReturnsForbiddenWithErrorResponseCode(ErrorResponseCode errorResponseCode)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _organisationsControllerInstance
                .OrganisationAdminAccessReturnsForbidden(_testOrganisationId, errorResponseCode);

            // Act
            try
            {
                await OrganisationsController.ChangeOrganisationParentAsync(
                    _testOrganisationId.ToString(),
                    _testNewParentId.ToString());
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
        public async Task ChangeOrganisationParentWithNoParentIdProvidedReturnsBadRequest()
        {
            // Arrange
            string testChildId = Guid.NewGuid().ToString();

            // Act
            IActionResult result =
                await OrganisationsController.ChangeOrganisationParentAsync(testChildId, null);
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, errorResult.StatusCode);
            Assert.AreEqual("No parent ID parameter specified.", errorResult.Value);
        }

        [Test]
        public async Task ChangeOrganisationParentForNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;
            var invalidOrganisationId = Guid.NewGuid();

            _organisationsControllerInstance
                .OrganisationAdminAccessThrowsNotFoundException(invalidOrganisationId);

            _organisationsRepository
                .ExistsAsync(_testNewParentId)
                .Returns(true);

            // Act
            try
            {
                await OrganisationsController.ChangeOrganisationParentAsync(
                    invalidOrganisationId.ToString(),
                    _testNewParentId.ToString());
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Organisation ID '{invalidOrganisationId}' not found.", caughtException.Message);
        }

        [Test]
        public async Task ChangeOrganisationParentToNonExistentParentReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;
            var invalidNewParentId = Guid.NewGuid();

            _organisationsControllerInstance
                .OrganisationAdminAccessThrowsNotFoundException(invalidNewParentId);

            // Act
            try
            {
                await OrganisationsController.ChangeOrganisationParentAsync(
                    _testOrganisationId.ToString(),
                    invalidNewParentId.ToString());
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Organisation ID '{invalidNewParentId}' not found.", caughtException.Message);
        }

        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(false, false, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        public async Task ChangeOrganisationParentWhenRequestingUserIsNotAdminOfAllThreeReturnsForbidden(bool isOldParentAdmin, bool isChildAdmin, bool isNewParentAdmin)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _organisationsRepository
                .GetOrganisationParentIdAsync(_testOrganisationId)
                .Returns(_testOldParentId);

            if (!isOldParentAdmin)
            {
                _organisationsControllerInstance.OrganisationAdminAccessReturnsForbidden(
                        _testOldParentId, ErrorResponseCode.NotOrganisationAdmin);
            }

            if (!isChildAdmin)
            {
                _organisationsControllerInstance.OrganisationAdminAccessReturnsForbidden(
                    _testOrganisationId, ErrorResponseCode.NotOrganisationAdmin);
            }

            if (!isNewParentAdmin)
            {
                _organisationsControllerInstance.OrganisationAdminAccessReturnsForbidden(
                    _testNewParentId, ErrorResponseCode.NotOrganisationAdmin);
            }

            // Act
            try
            {
                await OrganisationsController.ChangeOrganisationParentAsync(
                    _testOrganisationId.ToString(),
                    _testNewParentId.ToString());
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
        public async Task OrganisationAdminCanChangeOrganisationParent()
        {
            // Arrange
            _organisationsRepository
                .GetOrganisationParentIdAsync(_testOrganisationId)
                .Returns(_testOldParentId);

            _organisationsRepository
                .SaveChangesAsync()
                .Returns(true);

            // Act
            IActionResult result =
                await OrganisationsController.ChangeOrganisationParentAsync(
                    _testOrganisationId.ToString(),
                    _testNewParentId.ToString());
            var statusCodeResult = result as StatusCodeResult;

            // Assert
            Assert.NotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status200OK, statusCodeResult.StatusCode);
            Assert.IsNull(result as ObjectResult);
        }
    }
}
