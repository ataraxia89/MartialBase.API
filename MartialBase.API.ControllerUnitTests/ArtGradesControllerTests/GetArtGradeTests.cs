// <copyright file="GetArtGradeTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
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
    public class GetArtGradeTests
    {
        private IArtsRepository _artsRepository;
        private IArtGradesRepository _artGradesRepository;
        private IMartialBaseUserHelper _martialBaseUserHelper;
        private IOrganisationsRepository _organisationsRepository;
        private IAzureUserHelper _azureUserHelper;
        private ArtGradesControllerInstance _artGradesControllerInstance;
        private MartialBaseUser _testUser;
        private ArtGradeDTO _testArtGrade;
        private Guid _testArtGradeId;

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

            _testArtGrade = DataGenerator.ArtGrades.GenerateArtGradeDTO();

            _testArtGradeId = new Guid(_testArtGrade.Id);

            _artGradesRepository
                .ExistsAsync(_testArtGradeId)
                .Returns(true);

            _artGradesRepository
                .GetAsync(_testArtGradeId)
                .Returns(_testArtGrade);
        }

        [Test]
        public async Task GetArtGradeWithInvalidArtGradeIdReturnsInternalServerError()
        {
            // Arrange
            FormatException caughtException = null;

            _artGradesControllerInstance.SetTestUserRole(UserRoles.Thanos);

            // Act
            try
            {
                await ArtGradesController.GetArtGradeAsync("NotAGuid");
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
        }

        [Test]
        public async Task GetNonExistentArtGradeReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _artGradesControllerInstance.SetTestUserRole(UserRoles.Thanos);

            var invalidArtGradeId = Guid.NewGuid();

            _artGradesRepository
                .ExistsAsync(invalidArtGradeId)
                .Returns(false);

            // Act
            try
            {
                await ArtGradesController.GetArtGradeAsync(invalidArtGradeId.ToString());
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Art grade ID '{invalidArtGradeId}' not found.", caughtException.Message);
        }

        [Test]
        public async Task GetArtGradeWithNonRegisteredUserReturnsForbidden()
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _artGradesControllerInstance.SetTestUserRole(UserRoles.OrganisationMember);

            _artGradesControllerInstance.OrganisationMemberAccessReturnsForbidden(
                new Guid(_testArtGrade.OrganisationId), ErrorResponseCode.AzureUserNotRegistered);

            // Act
            try
            {
                await ArtGradesController.GetArtGradeAsync(_testArtGradeId.ToString());
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
        public async Task NonOrganisationUserCannotGetArtGrade(string roleName)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _artGradesControllerInstance.SetTestUserRole(roleName);

            _artGradesControllerInstance.OrganisationMemberAccessReturnsForbidden(
                new Guid(_testArtGrade.OrganisationId), ErrorResponseCode.NoOrganisationAccess);

            // Act
            try
            {
                await ArtGradesController.GetArtGradeAsync(_testArtGradeId.ToString());
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
        public async Task GetArtGradeWithNoAzureIdInRequestReturnsUnauthorized()
        {
            // Arrange
            _artGradesControllerInstance.SetTestUserRole(UserRoles.OrganisationMember);

            _artGradesControllerInstance.OrganisationMemberAccessThrowsUnauthorizedAccessException(
                new Guid(_testArtGrade.OrganisationId));

            // Act
            IActionResult result =
                await ArtGradesController.GetArtGradeAsync(_testArtGradeId.ToString());
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, errorResult.StatusCode);
            Assert.AreEqual("Auth token does not contain a valid user ID.", errorResult.Value);
        }

        [TestCaseSource(typeof(UserRoleResources), nameof(UserRoleResources.OrganisationUserRoleNames))]
        public async Task OrganisationMemberCanGetArtGrade(string roleName)
        {
            // Arrange
            _artGradesControllerInstance.SetTestUserRole(roleName);

            // Act
            IActionResult result =
                await ArtGradesController.GetArtGradeAsync(_testArtGradeId.ToString());
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<ArtGradeDTO>());

            var retrievedArtGrade = (ArtGradeDTO)objectResult.Value;

            ArtGradeResources.AssertEqual(_testArtGrade, retrievedArtGrade);
        }
    }
}
