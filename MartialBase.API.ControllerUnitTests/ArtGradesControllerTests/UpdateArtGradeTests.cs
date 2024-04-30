// <copyright file="UpdateArtGradeTests.cs" company="Martialtech®">
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
    public class UpdateArtGradeTests
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
        private UpdateArtGradeDTO _testUpdateArtGradeDTO;
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
        public async Task UpdateArtGradeWithInvalidDTOReturnsInternalServerError()
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
                ArtGradesController.ModelState.AddModelError(testError.Key, testError.Value);
            }

            // Act
            IActionResult result = await ArtGradesController.UpdateArtGradeAsync(
                _testArtGradeId.ToString(), _testUpdateArtGradeDTO);
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
            Assert.That(errorResult.Value, Is.TypeOf<Dictionary<string, string[]>>());

            DictionaryResources.AssertEqual(expectedModelStateErrors, (Dictionary<string, string[]>)errorResult.Value);
        }

        [Test]
        public async Task UpdateNonExistentArtGradeReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _artGradesRepository
                .ExistsAsync(_testArtGradeId)
                .Returns(false);

            // Act
            try
            {
                await ArtGradesController.UpdateArtGradeAsync(
                    _testArtGradeId.ToString(),
                    _testUpdateArtGradeDTO);
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
        public async Task UpdateArtGradeReturnsForbiddenWithErrorResponseCode(ErrorResponseCode expectedErrorResponseCode)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _artGradesControllerInstance
                .OrganisationAdminAccessReturnsForbidden(_testOrganisationId, expectedErrorResponseCode);

            // Act
            try
            {
                await ArtGradesController.UpdateArtGradeAsync(
                    _testArtGradeId.ToString(),
                    _testUpdateArtGradeDTO);
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
        public async Task CanUpdateArtGrade()
        {
            // Act
            IActionResult result = await ArtGradesController.UpdateArtGradeAsync(
                _testArtGradeId.ToString(), _testUpdateArtGradeDTO);
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<ArtGradeDTO>());

            var updatedArtGrade = (ArtGradeDTO)objectResult.Value;

            ArtGradeResources.AssertEqual(_testArtGradeDTO, updatedArtGrade);
        }

        [Test]
        public async Task UpdateArtGradeAsInvalidUserReturnsUnauthorized()
        {
            // Arrange
            _artGradesControllerInstance
                .OrganisationAdminAccessThrowsUnauthorizedAccessException(_testOrganisationId);

            // Act
            IActionResult result = await ArtGradesController.UpdateArtGradeAsync(
                _testArtGradeId.ToString(), _testUpdateArtGradeDTO);
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

            _testUpdateArtGradeDTO = DataGenerator.ArtGrades.GenerateUpdateArtGradeDTOObject();
            _testArtGradeDTO = DataGenerator.ArtGrades.GenerateArtGradeDTO();

            _testArtGradeDTO.OrganisationId = _testOrganisationId.ToString();

            _artGradesRepository
                .ExistsAsync(_testArtGradeId)
                .Returns(true);

            _artGradesRepository
                .GetArtGradeOrganisationIdAsync(_testArtGradeId)
                .Returns(_testOrganisationId);

            _artGradesRepository
                .UpdateAsync(_testArtGradeId, _testUpdateArtGradeDTO)
                .Returns(_testArtGradeDTO);

            _artGradesRepository
                .SaveChangesAsync()
                .Returns(true);
        }
    }
}
