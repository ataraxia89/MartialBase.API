// <copyright file="GetOrganisationDocumentTypesTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.DocumentTypes;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.TestResources;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.ControllerUnitTests.OrganisationsControllerTests
{
    public class GetOrganisationDocumentTypesTests
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
        }

        [TestCase(ErrorResponseCode.InsufficientUserRole)]
        [TestCase(ErrorResponseCode.AzureUserNotRegistered)]
        public async Task GetOrganisationDocumentTypesReturnsForbiddenWithErrorResponseCode(ErrorResponseCode errorResponseCode)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _organisationsControllerInstance
                .OrganisationMemberAccessReturnsForbidden(_testOrganisationId, errorResponseCode);

            // Act
            try
            {
                await OrganisationsController.GetOrganisationDocumentTypesAsync(_testOrganisationId.ToString());
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
        public async Task GetOrganisationDocumentTypesForNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _organisationsControllerInstance.OrganisationMemberAccessThrowsNotFoundException(_testOrganisationId);

            // Act
            try
            {
                await OrganisationsController.GetOrganisationDocumentTypesAsync(_testOrganisationId.ToString());
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
        public async Task OrganisationMemberCanGetOrganisationDocumentTypes()
        {
            // Arrange
            List<DocumentTypeDTO> testDocumentTypes = DataGenerator.DocumentTypes.GenerateDocumentTypeDTOs(50);

            _documentTypesRepository
                .GetAllAsync(_testOrganisationId)
                .Returns(testDocumentTypes);

            // Act
            IActionResult result =
                await OrganisationsController.GetOrganisationDocumentTypesAsync(_testOrganisationId.ToString());
            var errorResult = result as ObjectResult;

            // Assert
            Assert.NotNull(errorResult);
            Assert.AreEqual(StatusCodes.Status200OK, errorResult.StatusCode);
            Assert.That(errorResult.Value, Is.TypeOf<List<DocumentTypeDTO>>());

            var retrievedDocumentTypes = (List<DocumentTypeDTO>)errorResult.Value;

            DocumentTypeResources.AssertEqual(testDocumentTypes, retrievedDocumentTypes);
        }
    }
}
