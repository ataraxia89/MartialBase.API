// <copyright file="DeleteArtGradeTests.cs" company="Martialtech®">
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
    public class DeleteArtGradeTests
    {
        private IArtsRepository _artsRepository;
        private IArtGradesRepository _artGradesRepository;
        private IMartialBaseUserHelper _martialBaseUserHelper;
        private IOrganisationsRepository _organisationsRepository;
        private IAzureUserHelper _azureUserHelper;
        private ArtGradesControllerInstance _artGradesControllerInstance;
        private MartialBaseUser _testUser;
        private Guid _testArtGradeId;
        private Guid _testOrganisationId;
        private ArtGradeDTO _testArtGradeDTO;

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

            PrepareArtGradeResources();
        }

        [Test]
        public async Task DeleteNonExistentArtGradeReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _artGradesRepository
                .ExistsAsync(_testArtGradeId)
                .Returns(false);

            // Act
            try
            {
                await ArtGradesController.DeleteArtGradeAsync(_testArtGradeId.ToString());
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Art grade ID '{_testArtGradeId}' not found.", caughtException.Message);
        }

        [TestCase(ErrorResponseCode.InsufficientUserRole)]
        [TestCase(ErrorResponseCode.AzureUserNotRegistered)]
        [TestCase(ErrorResponseCode.NotOrganisationAdmin)]
        public async Task DeleteArtGradeReturnsForbiddenWithErrorResponseCode(ErrorResponseCode expectedErrorResponseCode)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _artGradesControllerInstance
                .OrganisationAdminAccessReturnsForbidden(_testOrganisationId, expectedErrorResponseCode);

            // Act
            try
            {
                await ArtGradesController.DeleteArtGradeAsync(_testArtGradeId.ToString());
            }
            catch (ErrorResponseCodeException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual(expectedErrorResponseCode, caughtException.ErrorResponseCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, caughtException.StatusCode);
        }

        [Test]
        public async Task CanDeleteArtGrade()
        {
            // Act
            IActionResult result = await ArtGradesController.DeleteArtGradeAsync(_testArtGradeId.ToString());
            var statusCodeResult = result as StatusCodeResult;

            // Assert
            Assert.NotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status204NoContent, statusCodeResult.StatusCode);
            Assert.Null(result as ObjectResult);
        }

        [Test]
        public async Task DeleteArtGradeAsInvalidUserReturnsUnauthorized()
        {
            // Arrange
            _artGradesControllerInstance
                .OrganisationAdminAccessThrowsUnauthorizedAccessException(_testOrganisationId);

            // Act
            IActionResult result = await ArtGradesController.DeleteArtGradeAsync(_testArtGradeId.ToString());
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, errorResult.StatusCode);
            Assert.AreEqual("Auth token does not contain a valid user ID.", errorResult.Value);
        }

        private void PrepareArtGradeResources()
        {
            _testArtGradeId = Guid.NewGuid();
            _testOrganisationId = Guid.NewGuid();

            _testArtGradeDTO = DataGenerator.ArtGrades.GenerateArtGradeDTO();

            _testArtGradeDTO.OrganisationId = _testOrganisationId.ToString();

            _artGradesRepository
                .ExistsAsync(_testArtGradeId)
                .Returns(true);

            _artGradesRepository
                .GetArtGradeOrganisationIdAsync(_testArtGradeId)
                .Returns(_testOrganisationId);

            _artGradesRepository
                .SaveChangesAsync()
                .Returns(true);
        }
    }
}
