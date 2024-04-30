// <copyright file="ChangeOrganisationParentTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.OrganisationsControllerTests
{
    [Collection("LiveControllerTests")]
    public class ChangeOrganisationParentTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeOrganisationParentTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public ChangeOrganisationParentTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        public static IEnumerable<object[]> NotAdminOfChildAndOldAndNewParentOrganisations
        {
            get
            {
                object[] memberofNone = { false, false, false, false, false, false };
                object[] memberOfChildOnly = { true, false, false, false, false, false };
                object[] memberOfOldParentOnly = { false, false, true, false, false, false };
                object[] memberOfNewParentOnly = { false, false, false, false, true, false };
                object[] adminOfChildOnly = { true, true, false, false, false, false };
                object[] adminOfOldParentOnly = { false, false, true, true, false, false };
                object[] adminOfNewParentOnly = { false, false, false, false, true, true };
                object[] adminOfChildMemberOfOldParentOnly = { true, true, true, false, false, false };
                object[] adminOfChildAndOldParentOnly = { true, true, true, true, false, false };
                object[] adminOfChildMemberOfNewParentOnly = { true, true, false, false, true, false };
                object[] adminOfChildAndNewParentOnly = { true, true, false, false, true, true };
                object[] memberOfChildAdminOfOldParentOnly = { true, false, true, true, false, false };
                object[] memberOfChildAdminOfNewParentOnly = { true, false, false, false, true, true };
                object[] memberOfChildAndOldParentAdminOfNewParent = { true, false, true, false, true, true };
                object[] memberOfChildAndNewParentMemberOfOldParent = { true, false, true, true, true, false };
                object[] adminOfChildMemberOfOldAndNewParent = { true, true, true, false, true, false };
                object[] adminOfChildAndOldParentMemberOfNewParent = { true, true, true, true, true, false };
                object[] adminOfChildAndNewParentMemberOfOldParent = { true, true, true, false, true, true };
                object[] memberOfChildAdminOfOldAndNewParent = { true, false, true, true, true, true };
                object[] memberOfAllThree = { true, false, true, false, true, false };

                return new List<object[]>
                {
                    memberofNone,
                    memberOfChildOnly,
                    memberOfOldParentOnly,
                    memberOfNewParentOnly,
                    adminOfChildOnly,
                    adminOfOldParentOnly,
                    adminOfNewParentOnly,
                    adminOfChildMemberOfOldParentOnly,
                    adminOfChildAndOldParentOnly,
                    adminOfChildMemberOfNewParentOnly,
                    adminOfChildAndNewParentOnly,
                    memberOfChildAdminOfOldParentOnly,
                    memberOfChildAdminOfNewParentOnly,
                    memberOfChildAndOldParentAdminOfNewParent,
                    memberOfChildAndNewParentMemberOfOldParent,
                    adminOfChildMemberOfOldAndNewParent,
                    adminOfChildAndOldParentMemberOfNewParent,
                    adminOfChildAndNewParentMemberOfOldParent,
                    memberOfChildAdminOfOldAndNewParent,
                    memberOfAllThree
                };
            }
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task ChangeOrganisationParentWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await OrganisationsController.ChangeOrganisationParentResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonOrganisationAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationAdminUserCannotChangeOrganisationParent(string roleName)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation testParentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.ChangeOrganisationParentResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                testParentOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonOrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationUserCannotChangeOrganisationParent(string roleName)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation testParentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.ChangeOrganisationParentResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                testParentOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Fact]
        public async Task BlockedOrganisationAdminUserCannotChangeOrganisationParent()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation testParentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await OrganisationsController.ChangeOrganisationParentResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                testParentOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        [Fact]
        public async Task ChangeOrganisationParentReturnsBadRequestWhenNoParentIdIsProvided()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(_fixture.DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.ChangeOrganisationParentResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal("No parent ID parameter specified.", response.ResponseBody);
        }

        [Fact]
        public async Task ChangeOrganisationParentForNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            var invalidOrganisationId = Guid.NewGuid();
            Organisation parentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationResources.AssertDoesNotExist(invalidOrganisationId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.ChangeOrganisationParentResponseAsync(
                _fixture.Client,
                invalidOrganisationId.ToString(),
                parentOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Organisation ID '{invalidOrganisationId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task ChangeOrganisationParentToNonExistentParentReturnsNotFound()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            var invalidParentId = Guid.NewGuid();

            OrganisationResources.AssertDoesNotExist(invalidParentId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.ChangeOrganisationParentResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                invalidParentId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Organisation ID '{invalidParentId}' not found.", response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(NotAdminOfChildAndOldAndNewParentOrganisations))]
        public async Task SuperUserCanChangeOrganisationParentRegardlessOfMembershipOrAdminStatus(
            bool memberOfChild,
            bool adminOfChild,
            bool memberOfOldParent,
            bool adminOfOldParent,
            bool memberOfNewParent,
            bool adminOfNewParent)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation oldParentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation newParentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationResources.AssertNotEqual(oldParentOrganisation, newParentOrganisation);

            OrganisationResources.SetOrganisationParent(testOrganisation, oldParentOrganisation, DbIdentifier);

            if (memberOfChild)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    testOrganisation.Id, TestUserPersonId, DbIdentifier, adminOfChild);
            }

            if (memberOfOldParent)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    oldParentOrganisation.Id, TestUserPersonId, DbIdentifier, adminOfOldParent);
            }

            if (memberOfNewParent)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    newParentOrganisation.Id, TestUserPersonId, DbIdentifier, adminOfNewParent);
            }

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            // Assert
            Assert.True(
                await OrganisationsController.ChangeOrganisationParentAsync(
                    _fixture.Client, testOrganisation.Id.ToString(), newParentOrganisation.Id.ToString()));

            Assert.True(
                OrganisationResources.CheckOrganisationBelongsToParent(
                    testOrganisation.Id, newParentOrganisation.Id, DbIdentifier));
        }

        [Theory]
        [MemberData(nameof(NotAdminOfChildAndOldAndNewParentOrganisations))]
        public async Task OrganisationAdminCannotChangeOrganisationParentIfTheyAreNotAdminOfAllThree(
            bool memberOfChild,
            bool adminOfChild,
            bool memberOfOldParent,
            bool adminOfOldParent,
            bool memberOfNewParent,
            bool adminOfNewParent)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation oldParentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation newParentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationResources.AssertNotEqual(oldParentOrganisation, newParentOrganisation);

            OrganisationResources.SetOrganisationParent(testOrganisation, oldParentOrganisation, DbIdentifier);

            if (memberOfChild)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    testOrganisation.Id, TestUserPersonId, DbIdentifier, adminOfChild);
            }

            if (memberOfOldParent)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    oldParentOrganisation.Id, TestUserPersonId, DbIdentifier, adminOfOldParent);
            }

            if (memberOfNewParent)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    newParentOrganisation.Id, TestUserPersonId, DbIdentifier, adminOfNewParent);
            }

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.ChangeOrganisationParentResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                newParentOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        [Fact]
        public async Task OrganisationAdminCanChangeOrganisationParentIfTheyAreAdminOfAllThreeOrganisations()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation oldParentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation newParentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationResources.AssertNotEqual(oldParentOrganisation, newParentOrganisation);

            OrganisationResources.SetOrganisationParent(
                testOrganisation, oldParentOrganisation, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);
            OrganisationPersonResources.EnsureOrganisationHasPerson(
                oldParentOrganisation.Id, TestUserPersonId, DbIdentifier, true);
            OrganisationPersonResources.EnsureOrganisationHasPerson(
                newParentOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            // Assert
            Assert.True(
                await OrganisationsController.ChangeOrganisationParentAsync(
                    _fixture.Client,
                    testOrganisation.Id.ToString(),
                    newParentOrganisation.Id.ToString()));

            Assert.True(OrganisationResources.CheckOrganisationBelongsToParent(
                testOrganisation.Id, newParentOrganisation.Id, _fixture.DbIdentifier));
        }
    }
}
