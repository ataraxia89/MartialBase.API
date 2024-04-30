// <copyright file="UpdateOrganisationTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.TestResources;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.ControllerUnitTests.OrganisationsControllerTests
{
    public class UpdateOrganisationTests
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
        private OrganisationDTO _testOrganisationDTO;
        private Guid _testOrganisationId;
        private UpdateOrganisationDTO _testUpdateOrganisationDTO;

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

            _testOrganisationDTO = DataGenerator.Organisations.GenerateOrganisationDTO();
            _testOrganisationId = new Guid(_testOrganisationDTO.Id);
            _testUpdateOrganisationDTO = DataGenerator.Organisations.GenerateUpdateOrganisationDTOObject();
        }

        [Test]
        public async Task UpdateOrganisationWithInvalidDTOReturnsInternalServerErrorWithDetails()
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
            IActionResult result = await OrganisationsController.UpdateOrganisationAsync(
                _testOrganisationId.ToString(), _testUpdateOrganisationDTO);
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
            Assert.That(errorResult.Value, Is.TypeOf<Dictionary<string, string[]>>());

            DictionaryResources.AssertEqual(
                expectedModelStateErrors, (Dictionary<string, string[]>)errorResult.Value);
        }

        [TestCase(ErrorResponseCode.InsufficientUserRole)]
        [TestCase(ErrorResponseCode.AzureUserNotRegistered)]
        [TestCase(ErrorResponseCode.NotOrganisationAdmin)]
        public async Task UpdateOrganisationReturnsForbiddenWithErrorResponseCode(ErrorResponseCode errorResponseCode)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _organisationsControllerInstance
                .OrganisationAdminAccessReturnsForbidden(_testOrganisationId, errorResponseCode);

            // Act
            try
            {
                await OrganisationsController.UpdateOrganisationAsync(
                    _testOrganisationId.ToString(),
                    _testUpdateOrganisationDTO);
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
        public async Task UpdateNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _organisationsControllerInstance.OrganisationAdminAccessThrowsNotFoundException(_testOrganisationId);

            // Act
            try
            {
                await OrganisationsController.UpdateOrganisationAsync(
                    _testOrganisationId.ToString(),
                    _testUpdateOrganisationDTO);
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
        public async Task OrganisationAdminCanUpdateOrganisation()
        {
            // Arrange
            _organisationsRepository
                .UpdateAsync(_testOrganisationId, _testUpdateOrganisationDTO)
                .Returns(_testOrganisationDTO);

            _organisationsRepository
                .SaveChangesAsync()
                .Returns(true);

            // Act
            IActionResult result = await OrganisationsController.UpdateOrganisationAsync(
                    _testOrganisationId.ToString(), _testUpdateOrganisationDTO);
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<OrganisationDTO>());

            var updatedOrganisation = (OrganisationDTO)objectResult.Value;

            OrganisationResources.AssertEqual(_testOrganisationDTO, updatedOrganisation);
        }
    }
}
