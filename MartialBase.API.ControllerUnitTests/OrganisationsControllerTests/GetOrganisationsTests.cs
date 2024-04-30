// <copyright file="GetOrganisationsTests.cs" company="Martialtech®">
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
    public class GetOrganisationsTests
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
        }

        [TestCaseSource(typeof(UserRoleResources), nameof(UserRoleResources.NonOrganisationUserRoleNames))]
        public async Task GetOrganisationsAsNonOrganisationUserReturnsInsufficientUserRole(string roleName)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _organisationsControllerInstance.SetTestUserRole(roleName);

            // Act
            try
            {
                await OrganisationsController.GetOrganisationsAsync(null);
            }
            catch (ErrorResponseCodeException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual(ErrorResponseCode.InsufficientUserRole, caughtException.ErrorResponseCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, caughtException.StatusCode);
        }

        [Test]
        public async Task GetChildOrganisationsOrNonExistentParentReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;
            var invalidParentId = Guid.NewGuid();

            _organisationsControllerInstance.SetTestUserRole(UserRoles.Thanos);

            _organisationsControllerInstance.OrganisationMemberAccessThrowsNotFoundException(invalidParentId);

            // Act
            try
            {
                await OrganisationsController.GetOrganisationsAsync(invalidParentId.ToString());
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Organisation ID '{invalidParentId}' not found.", caughtException.Message);
        }

        [TestCaseSource(typeof(UserRoleResources), nameof(UserRoleResources.OrganisationUserRoleNames))]
        public async Task OrganisationPersonCanGetChildOrganisationsOfParent(string roleName)
        {
            // Arrange
            var testParentId = Guid.NewGuid();
            List<OrganisationDTO> testOrganisations = DataGenerator.Organisations.GenerateOrganisationDTOs(50);

            _organisationsControllerInstance.SetTestUserRole(roleName);

            _organisationsRepository
                .ExistsAsync(testParentId)
                .Returns(true);

            _martialBaseUserHelper
                .FilterOrganisationsByMemberAccess(testOrganisations, _testUser.PersonId)
                .Returns(testOrganisations);

            _organisationsRepository
                .GetChildOrganisationsAsync(testParentId)
                .Returns(testOrganisations);

            // Act
            IActionResult result = await OrganisationsController.GetOrganisationsAsync(testParentId.ToString());
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<List<OrganisationDTO>>());

            var retrievedOrganisations = (List<OrganisationDTO>)objectResult.Value;

            OrganisationResources.AssertEqual(testOrganisations, retrievedOrganisations);
        }

        [TestCaseSource(typeof(UserRoleResources), nameof(UserRoleResources.OrganisationUserRoleNames))]
        public async Task OrganisationPersonCanOnlyGetOrganisationsWhichTheyHaveAccessTo(string roleName)
        {
            // Arrange
            List<OrganisationDTO> memberOrganisations = DataGenerator.Organisations.GenerateOrganisationDTOs(50);
            List<OrganisationDTO> nonMemberOrganisations = DataGenerator.Organisations.GenerateOrganisationDTOs(50);

            var testOrganisations = new List<OrganisationDTO>();
            testOrganisations.AddRange(memberOrganisations);
            testOrganisations.AddRange(nonMemberOrganisations);

            _organisationsControllerInstance.SetTestUserRole(roleName);

            _martialBaseUserHelper
                .FilterOrganisationsByMemberAccess(testOrganisations, _testUser.PersonId)
                .Returns(memberOrganisations);

            _organisationsRepository
                .GetAllAsync()
                .Returns(testOrganisations);

            // Act
            IActionResult result = await OrganisationsController.GetOrganisationsAsync(null);
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<List<OrganisationDTO>>());

            var retrievedOrganisations = (List<OrganisationDTO>)objectResult.Value;

            OrganisationResources.AssertEqual(memberOrganisations, retrievedOrganisations);
        }
    }
}
