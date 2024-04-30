// <copyright file="UpdateOrganisationTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.LiveControllerTests.TestTools;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.OrganisationsControllerTests
{
    [Collection("LiveControllerTests")]
    public class UpdateOrganisationTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateOrganisationTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public UpdateOrganisationTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        public static IEnumerable<object[]> InvalidUpdateOrganisationDTOs
        {
            get
            {
                object[] initialsTooLong =
                {
                    new UpdateOrganisationDTO()
                    {
                        Initials = new string('*', 9),
                        Name = "Test Organisation",
                    },
                    "Initials\":[\"Initials cannot be longer than 8 characters.\"]"
                };

                object[] nameTooLong =
                {
                    new UpdateOrganisationDTO()
                    {
                        Initials = "TSTORG",
                        Name = new string('*', 61),
                    },
                    "Name\":[\"Name cannot be longer than 60 characters.\"]"
                };

                return new List<object[]>
                {
                    initialsTooLong,
                    nameTooLong
                };
            }
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task UpdateOrganisationWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await OrganisationsController.UpdateOrganisationResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                DataGenerator.Organisations.GenerateUpdateOrganisationDTOObject());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Fact]
        public async Task SuperUserCanUpdateOrganisationOfWhichTheyAreNotAMember()
        {
            // Arrange
            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            UpdateOrganisationDTO updateOrganisationDTO =
                DataGenerator.Organisations.GenerateUpdateOrganisationDTOObject();

            OrganisationResources.AssertNotEqual(testOrganisation, updateOrganisationDTO);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            OrganisationDTO updatedOrganisation =
                await OrganisationsController.UpdateOrganisationAsync(
                    _fixture.Client, testOrganisation.Id.ToString(), updateOrganisationDTO);

            // Assert
            OrganisationResources.AssertEqual(updateOrganisationDTO, updatedOrganisation);
            OrganisationResources.AssertExists(updatedOrganisation, DbIdentifier);
        }

        [Fact]
        public async Task SuperUserCanUpdateOrganisationOfWhichTheyAreAMemberButNotAdmin()
        {
            // Arrange
            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            UpdateOrganisationDTO updateOrganisationDTO =
                DataGenerator.Organisations.GenerateUpdateOrganisationDTOObject();

            OrganisationResources.AssertNotEqual(testOrganisation, updateOrganisationDTO);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            OrganisationDTO updatedOrganisation =
                await OrganisationsController.UpdateOrganisationAsync(
                    _fixture.Client, testOrganisation.Id.ToString(), updateOrganisationDTO);

            // Assert
            OrganisationResources.AssertEqual(updateOrganisationDTO, updatedOrganisation);
            OrganisationResources.AssertExists(updatedOrganisation, DbIdentifier);
        }

        [Fact]
        public async Task OrganisationAdminCanUpdateOrganisationOfWhichTheyAreAMember()
        {
            // Arrange
            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            UpdateOrganisationDTO updateOrganisationDTO =
                DataGenerator.Organisations.GenerateUpdateOrganisationDTOObject();

            OrganisationResources.AssertNotEqual(testOrganisation, updateOrganisationDTO);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            OrganisationDTO updatedOrganisation =
                await OrganisationsController.UpdateOrganisationAsync(
                    _fixture.Client, testOrganisation.Id.ToString(), updateOrganisationDTO);

            // Assert
            OrganisationResources.AssertEqual(updateOrganisationDTO, updatedOrganisation);
            OrganisationResources.AssertExists(updatedOrganisation, DbIdentifier);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonOrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationUserCannotUpdateOrganisation(string roleName)
        {
            // Arrange
            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            UpdateOrganisationDTO updateOrganisationDTO =
                DataGenerator.Organisations.GenerateUpdateOrganisationDTOObject();

            OrganisationResources.AssertNotEqual(testOrganisation, updateOrganisationDTO);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.UpdateOrganisationResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                updateOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Fact]
        public async Task BlockedOrganisationAdminUserCannotUpdateOrganisation()
        {
            // Arrange
            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            UpdateOrganisationDTO updateOrganisationDTO =
                DataGenerator.Organisations.GenerateUpdateOrganisationDTOObject();

            OrganisationResources.AssertNotEqual(testOrganisation, updateOrganisationDTO);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await OrganisationsController.UpdateOrganisationResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                updateOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        /// <summary>
        /// Tests that a regular member of an organisation cannot update their organisation unless they are
        /// assigned as an admin of the given organisation, even if they are in the OrganisationAdmin role.
        /// </summary>
        [Fact]
        public async Task NonAdminOrganisationMemberCannotUpdateOrganisationRegardlessOfUserRole()
        {
            // Arrange
            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            UpdateOrganisationDTO updateOrganisationDTO =
                DataGenerator.Organisations.GenerateUpdateOrganisationDTOObject();

            OrganisationResources.AssertNotEqual(testOrganisation, updateOrganisationDTO);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.UpdateOrganisationResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                updateOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        /// <summary>
        /// Tests that a user without the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role
        /// cannot update an organisation even if they are assigned as an organisation admin.
        /// </summary>
        /// <param name="roleName">The name of the <see cref="UserRole"/> to be assigned to the test user.</param>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonOrganisationAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationAdminUserCannotUpdateOrganisationRegardlessOfMembership(string roleName)
        {
            // Arrange
            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            UpdateOrganisationDTO updateOrganisationDTO =
                DataGenerator.Organisations.GenerateUpdateOrganisationDTOObject();

            OrganisationResources.AssertNotEqual(testOrganisation, updateOrganisationDTO);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.UpdateOrganisationResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                updateOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(InvalidUpdateOrganisationDTOs))]
        public async Task UpdateOrganisationWithInvalidDTOReturnsInternalServerError(
            UpdateOrganisationDTO invalidUpdateOrganisationDTO, string expectedErrorMessage)
        {
            // Arrange
            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            OrganisationResources.AssertNotEqual(testOrganisation, invalidUpdateOrganisationDTO);

            // Act
            HttpResponseModel response = await OrganisationsController.UpdateOrganisationResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                invalidUpdateOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            // TODO: Change this to check a dictionary for a parameter value
            Assert.Contains(expectedErrorMessage, response.ResponseBody);
        }

        [Fact]
        public async Task UpdateNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            string invalidOrganisationId = Guid.NewGuid().ToString();

            UpdateOrganisationDTO updateOrganisationDTO =
                DataGenerator.Organisations.GenerateUpdateOrganisationDTOObject();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.UpdateOrganisationResponseAsync(
                _fixture.Client,
                invalidOrganisationId,
                updateOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Organisation ID '{invalidOrganisationId}' not found.", response.ResponseBody);
        }
    }
}
