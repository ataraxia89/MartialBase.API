// <copyright file="CreateArtGradeTests.cs" company="Martialtech®">
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
using MartialBase.API.Data.Models.InternalDTOs.ArtGrades;
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
    public class CreateArtGradeTests
    {
        private IArtsRepository _artsRepository;
        private IArtGradesRepository _artGradesRepository;
        private IMartialBaseUserHelper _martialBaseUserHelper;
        private IOrganisationsRepository _organisationsRepository;
        private IAzureUserHelper _azureUserHelper;
        private ArtGradesControllerInstance _artGradesControllerInstance;
        private MartialBaseUser _testUser;
        private Guid _testArtId;
        private Guid _testOrganisationId;
        private CreateArtGradeDTO _testCreateArtGradeDTO;

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

            _testArtId = Guid.NewGuid();
            _testOrganisationId = Guid.NewGuid();

            _testCreateArtGradeDTO =
                DataGenerator.ArtGrades.GenerateCreateArtGradeDTOObject(_testArtId, _testOrganisationId);
        }

        [Test]
        public async Task CreateArtGradeWithInvalidCreateDTOReturnsInternalServerError()
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
            IActionResult result = await ArtGradesController.CreateArtGradeAsync(_testCreateArtGradeDTO);
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
            Assert.That(errorResult.Value, Is.TypeOf<Dictionary<string, string[]>>());

            DictionaryResources.AssertEqual(expectedModelStateErrors, (Dictionary<string, string[]>)errorResult.Value);
        }

        [Test]
        public async Task CreateArtGradeWithInvalidArtIdReturnsInternalServerError()
        {
            // Arrange
            FormatException caughtException = null;
            _testCreateArtGradeDTO.ArtId = "NotAGuid";

            // Act
            try
            {
                await ArtGradesController.CreateArtGradeAsync(_testCreateArtGradeDTO);
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
        }

        [Test]
        public async Task CreateArtGradeWithInvalidOrganisationIdReturnsInternalServerError()
        {
            // Arrange
            FormatException caughtException = null;
            _testCreateArtGradeDTO.OrganisationId = "NotAGuid";

            // Act
            try
            {
                await ArtGradesController.CreateArtGradeAsync(_testCreateArtGradeDTO);
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
        }

        [Test]
        public async Task CreateArtGradeWithNonExistentArtIdReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _artsRepository
                .ExistsAsync(_testArtId)
                .Returns(false);

            // Act
            try
            {
                await ArtGradesController.CreateArtGradeAsync(_testCreateArtGradeDTO);
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Art ID '{_testCreateArtGradeDTO.ArtId}' not found.", caughtException.Message);
        }

        [Test]
        public async Task CreateArtGradeWithNonExistentOrganisationIdReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _artsRepository
                .ExistsAsync(_testArtId)
                .Returns(true);

            _artsRepository
                .ExistsAsync(_testOrganisationId)
                .Returns(false);

            // Act
            try
            {
                await ArtGradesController.CreateArtGradeAsync(_testCreateArtGradeDTO);
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
        public async Task CreateArtGradeWithNonRegisteredUserReturnsForbidden()
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _artsRepository
                .ExistsAsync(_testArtId)
                .Returns(true);

            _organisationsRepository
                .ExistsAsync(_testOrganisationId)
                .Returns(true);

            _artGradesControllerInstance.OrganisationAdminAccessReturnsForbidden(
                _testOrganisationId, ErrorResponseCode.AzureUserNotRegistered);

            // Act
            try
            {
                await ArtGradesController.CreateArtGradeAsync(_testCreateArtGradeDTO);
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
        public async Task NonOrganisationAdminUserCannotCreateArtGrade(string roleName)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _artGradesControllerInstance.SetTestUserRole(roleName);

            _artsRepository
                .ExistsAsync(_testArtId)
                .Returns(true);

            _organisationsRepository
                .ExistsAsync(_testOrganisationId)
                .Returns(true);

            _artGradesControllerInstance.OrganisationAdminAccessReturnsForbidden(
                _testOrganisationId, ErrorResponseCode.NoOrganisationAccess);

            // Act
            try
            {
                await ArtGradesController.CreateArtGradeAsync(_testCreateArtGradeDTO);
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
        public async Task CreateArtGradeWithNoAzureIdInRequestReturnsUnauthorized()
        {
            // Arrange
            _artsRepository
                .ExistsAsync(_testArtId)
                .Returns(true);

            _organisationsRepository
                .ExistsAsync(_testOrganisationId)
                .Returns(true);

            _artGradesControllerInstance.OrganisationAdminAccessThrowsUnauthorizedAccessException(
                _testOrganisationId);

            // Act
            IActionResult result =
                await ArtGradesController.CreateArtGradeAsync(_testCreateArtGradeDTO);
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, errorResult.StatusCode);
            Assert.AreEqual("Auth token does not contain a valid user ID.", errorResult.Value);
        }

        [Test]
        public async Task OrganisationAdminCanCreateArtGrade()
        {
            // Arrange
            _artGradesControllerInstance.SetTestUserRole(UserRoles.OrganisationAdmin);

            _artsRepository
                .ExistsAsync(_testArtId)
                .Returns(true);

            _organisationsRepository
                .ExistsAsync(_testOrganisationId)
                .Returns(true);

            _artGradesRepository
                .CreateAsync(Arg.Any<CreateArtGradeInternalDTO>())
                .Returns(new ArtGradeDTO());

            _artGradesRepository
                .SaveChangesAsync()
                .Returns(true);

            // Act
            IActionResult result = await ArtGradesController.CreateArtGradeAsync(_testCreateArtGradeDTO);
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<ArtGradeDTO>());
        }
    }
}
