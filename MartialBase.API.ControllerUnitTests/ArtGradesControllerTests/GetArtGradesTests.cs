// <copyright file="GetArtGradesTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.ArtGrades;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.TestResources;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.ControllerUnitTests.ArtGradesControllerTests
{
    public class GetArtGradesTests
    {
        private IArtsRepository _artsRepository;
        private IArtGradesRepository _artGradesRepository;
        private IMartialBaseUserHelper _martialBaseUserHelper;
        private IOrganisationsRepository _organisationsRepository;
        private IAzureUserHelper _azureUserHelper;
        private ArtGradesControllerInstance _artGradesControllerInstance;
        private MartialBaseUser _testUser;

        private ArtGradesController ArtGradesController => _artGradesControllerInstance.Instance;

        [SetUp]
        public void Setup()
        {
            _artsRepository = Substitute.For<IArtsRepository>();
            _artGradesRepository = Substitute.For<IArtGradesRepository>();
            _martialBaseUserHelper = Substitute.For<IMartialBaseUserHelper>();
            _organisationsRepository = Substitute.For<IOrganisationsRepository>();
            _azureUserHelper = Substitute.For<IAzureUserHelper>();

            _testUser =
                DataGenerator.MartialBaseUsers.GenerateMartialBaseUserObject();

            _artGradesControllerInstance = new ArtGradesControllerInstance(
                _artsRepository,
                _artGradesRepository,
                _martialBaseUserHelper,
                _organisationsRepository,
                _azureUserHelper,
                "Live",
                _testUser);
        }

        [Test]
        public async Task GetArtGradesWithNoArtIdParameterReturnsBadRequest()
        {
            // Arrange
            _artGradesControllerInstance.SetTestUserRole(UserRoles.Thanos);

            // Act
            IActionResult result = await ArtGradesController.GetArtGradesAsync(null, null);
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, errorResult.StatusCode);
            Assert.AreEqual("No art ID parameter specified.", errorResult.Value);
        }

        [Test]
        public async Task GetArtGradesWithNoOrganisationIdParameterReturnsBadRequest()
        {
            // Arrange
            _artGradesControllerInstance.SetTestUserRole(UserRoles.Thanos);

            // Act
            IActionResult result = await ArtGradesController.GetArtGradesAsync(Guid.NewGuid().ToString(), null);
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, errorResult.StatusCode);
            Assert.AreEqual("No organisation ID parameter specified.", errorResult.Value);
        }

        [Test]
        public async Task GetArtGradesWithInvalidArtIdReturnsInternalServerError()
        {
            // Arrange
            FormatException caughtException = null;

            _artGradesControllerInstance.SetTestUserRole(UserRoles.Thanos);

            // Act
            try
            {
                await ArtGradesController.GetArtGradesAsync("NotAGuid", Guid.NewGuid().ToString());
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
        }

        [Test]
        public async Task GetArtGradesWithInvalidOrganisationIdReturnsInternalServerError()
        {
            // Arrange
            FormatException caughtException = null;

            _artGradesControllerInstance.SetTestUserRole(UserRoles.Thanos);

            // Act
            try
            {
                await ArtGradesController.GetArtGradesAsync(Guid.NewGuid().ToString(), "NotAGuid");
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
        }

        [Test]
        public async Task GetArtGradesWithNonExistentArtIdReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _artGradesControllerInstance.SetTestUserRole(UserRoles.Thanos);

            var invalidArtId = Guid.NewGuid();

            _artsRepository
                .ExistsAsync(invalidArtId)
                .Returns(false);

            // Act
            try
            {
                await ArtGradesController.GetArtGradesAsync(invalidArtId.ToString(), Guid.NewGuid().ToString());
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Art ID '{invalidArtId}' not found.", caughtException.Message);
        }

        [Test]
        public async Task GetArtGradesWithNonExistentOrganisationIdReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _artGradesControllerInstance.SetTestUserRole(UserRoles.Thanos);

            var artId = Guid.NewGuid();
            var invalidOrganisationId = Guid.NewGuid();

            _artsRepository
                .ExistsAsync(artId)
                .Returns(true);

            _artGradesControllerInstance.OrganisationMemberAccessThrowsNotFoundException(invalidOrganisationId);

            // Act
            try
            {
                await ArtGradesController.GetArtGradesAsync(artId.ToString(), invalidOrganisationId.ToString());
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
        public async Task GetArtGradesWithNonRegisteredUserReturnsForbidden()
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _artGradesControllerInstance.SetTestUserRole(UserRoles.OrganisationMember);

            var artId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();

            _artsRepository
                .ExistsAsync(artId)
                .Returns(true);

            _artGradesControllerInstance.OrganisationMemberAccessReturnsForbidden(
                organisationId, ErrorResponseCode.AzureUserNotRegistered);

            // Act
            try
            {
                await ArtGradesController.GetArtGradesAsync(artId.ToString(), organisationId.ToString());
            }
            catch (ErrorResponseCodeException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual(ErrorResponseCode.AzureUserNotRegistered, caughtException.ErrorResponseCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, caughtException.StatusCode);
        }

        [TestCaseSource(typeof(UserRoleResources), nameof(UserRoleResources.NonOrganisationUserRoleNames))]
        public async Task NonOrganisationUserCannotGetArtGrades(string roleName)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _artGradesControllerInstance.SetTestUserRole(roleName);

            var artId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();

            _artsRepository
                .ExistsAsync(artId)
                .Returns(true);

            _artGradesControllerInstance.OrganisationMemberAccessReturnsForbidden(
                organisationId, ErrorResponseCode.NoOrganisationAccess);

            // Act
            try
            {
                await ArtGradesController.GetArtGradesAsync(artId.ToString(), organisationId.ToString());
            }
            catch (ErrorResponseCodeException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual(ErrorResponseCode.NoOrganisationAccess, caughtException.ErrorResponseCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, caughtException.StatusCode);
        }

        [Test]
        public async Task GetArtGradesWithNoAzureIdInRequestReturnsUnauthorized()
        {
            // Arrange
            _artGradesControllerInstance.SetTestUserRole(UserRoles.OrganisationMember);

            var artId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();

            _artsRepository
                .ExistsAsync(artId)
                .Returns(true);

            _artGradesControllerInstance.OrganisationMemberAccessThrowsUnauthorizedAccessException(
                organisationId);

            // Act
            IActionResult result =
                await ArtGradesController.GetArtGradesAsync(artId.ToString(), organisationId.ToString());
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, errorResult.StatusCode);
            Assert.AreEqual("Auth token does not contain a valid user ID.", errorResult.Value);
        }

        [TestCaseSource(typeof(UserRoleResources), nameof(UserRoleResources.OrganisationUserRoleNames))]
        public async Task OrganisationMemberCanGetArtGrades(string roleName)
        {
            // Arrange
            _artGradesControllerInstance.SetTestUserRole(roleName);

            var artId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();

            List<ArtGradeDTO> testArtGrades = DataGenerator.ArtGrades.GenerateArtGradeDTOs(10);

            _artsRepository
                .ExistsAsync(artId)
                .Returns(true);

            _artGradesRepository
                .GetAllAsync(artId, organisationId)
                .Returns(testArtGrades);

            // Act
            IActionResult result =
                await ArtGradesController.GetArtGradesAsync(artId.ToString(), organisationId.ToString());
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<List<ArtGradeDTO>>());

            var retrievedArtGrades = (List<ArtGradeDTO>)objectResult.Value;

            ArtGradeResources.AssertEqual(testArtGrades, retrievedArtGrades);
        }
    }
}
