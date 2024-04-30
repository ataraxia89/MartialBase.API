// <copyright file="RemoveOrganisationParentTests.cs" company="Martialtech®">
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
    public class RemoveOrganisationParentTests
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
        private Guid _testParentId;

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
            _testParentId = Guid.NewGuid();
        }

        [TestCase(ErrorResponseCode.InsufficientUserRole)]
        [TestCase(ErrorResponseCode.AzureUserNotRegistered)]
        [TestCase(ErrorResponseCode.NotOrganisationAdmin)]
        public async Task RemoveOrganisationParentReturnsForbiddenWithErrorResponseCode(ErrorResponseCode errorResponseCode)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _organisationsControllerInstance
                .OrganisationAdminAccessReturnsForbidden(_testOrganisationId, errorResponseCode);

            // Act
            try
            {
                await OrganisationsController.RemoveOrganisationParentAsync(_testOrganisationId.ToString());
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
        public async Task RemoveOrganisationParentFromNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _organisationsControllerInstance.OrganisationAdminAccessThrowsNotFoundException(_testOrganisationId);

            // Act
            try
            {
                await OrganisationsController.RemoveOrganisationParentAsync(_testOrganisationId.ToString());
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
        public async Task RemoveOrganisationParentFromOrganisationThatHasNoParentReturnsNoContent()
        {
            // Arrange
            _organisationsRepository
                .GetOrganisationParentIdAsync(_testOrganisationId)
                .Returns((Guid?)null);

            // Act
            IActionResult result =
                await OrganisationsController.RemoveOrganisationParentAsync(_testOrganisationId.ToString());
            var statusCodeResult = result as StatusCodeResult;

            // Assert
            Assert.NotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status204NoContent, statusCodeResult.StatusCode);
            Assert.IsNull(result as ObjectResult);
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public async Task RemoveOrganisationParentWhenRequestingUserIsNotAdminOfBothReturnsForbidden(bool isOldParentAdmin, bool isChildAdmin)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            if (!isOldParentAdmin)
            {
                _organisationsControllerInstance
                    .OrganisationAdminAccessReturnsForbidden(
                        _testParentId, ErrorResponseCode.NotOrganisationAdmin);
            }

            if (!isChildAdmin)
            {
                _organisationsControllerInstance
                    .OrganisationAdminAccessReturnsForbidden(
                        _testOrganisationId, ErrorResponseCode.NotOrganisationAdmin);
            }

            _organisationsRepository
                .GetOrganisationParentIdAsync(_testOrganisationId)
                .Returns(_testParentId);

            // Act
            try
            {
                await OrganisationsController.RemoveOrganisationParentAsync(_testOrganisationId.ToString());
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
        public async Task OrganisationAdminCanRemoveOrganisationParent()
        {
            // Arrange
            _organisationsRepository
                .GetOrganisationParentIdAsync(_testOrganisationId)
                .Returns(_testParentId);

            _organisationsRepository
                .SaveChangesAsync()
                .Returns(true);

            // Act
            IActionResult result =
                await OrganisationsController.RemoveOrganisationParentAsync(_testOrganisationId.ToString());
            var statusCodeResult = result as StatusCodeResult;

            // Assert
            Assert.NotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status204NoContent, statusCodeResult.StatusCode);
            Assert.IsNull(result as ObjectResult);
        }
    }
}
