// <copyright file="RemoveOrganisationParentTests.cs" company="Martialtech®">
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
    public class RemoveOrganisationParentTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveOrganisationParentTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public RemoveOrganisationParentTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        public static IEnumerable<object[]> NotAdminOfChildAndParentOrganisations
        {
            get
            {
                object[] memberofNeither = { false, false, false, false };
                object[] memberOfChildOnly = { true, false, false, false };
                object[] memberOfParentOnly = { false, false, true, false };
                object[] adminOfChildOnly = { true, true, false, false };
                object[] adminOfParentOnly = { false, false, true, true };
                object[] adminOfChildMemberOfParent = { true, true, true, false };
                object[] memberOfChildAdminOfParent = { true, false, true, true };
                object[] memberOfBoth = { true, false, true, false };

                return new List<object[]>
                {
                    memberofNeither,
                    memberOfChildOnly,
                    memberOfParentOnly,
                    adminOfChildOnly,
                    adminOfParentOnly,
                    adminOfChildMemberOfParent,
                    memberOfChildAdminOfParent,
                    memberOfBoth
                };
            }
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task RemoveOrganisationParentWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationParentResponseAsync(
                _fixture.Client, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Fact]
        public async Task BlockedOrganisationAdminUserCannotRemoveOrganisationParent()
        {
            // Arrange
            Organisation parentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(
                DbIdentifier, parentOrganisation.Id);

            Assert.True(OrganisationResources.CheckOrganisationBelongsToParent(
                testOrganisation.Id, parentOrganisation.Id, DbIdentifier));

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationParentResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonOrganisationAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationAdminUserCannotRemoveOrganisationParent(string roleName)
        {
            // Arrange
            Organisation parentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(
                DbIdentifier, parentOrganisation.Id);

            Assert.True(OrganisationResources.CheckOrganisationBelongsToParent(
                testOrganisation.Id, parentOrganisation.Id, DbIdentifier));

            // This test is specifically for the OrganisationAdmin role, so the user can be admin of an
            // organisation, but they still must have this role assigned to them to carry out functions
            OrganisationPersonResources.EnsureOrganisationHasPerson(
                parentOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationParentResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Fact]
        public async Task RemoveParentFromNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            var invalidOrganisationId = Guid.NewGuid();

            OrganisationResources.AssertDoesNotExist(invalidOrganisationId, _fixture.DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationParentResponseAsync(
                _fixture.Client, invalidOrganisationId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Organisation ID '{invalidOrganisationId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task RemoveParentFromOrganisationWhichHasNoParentReturnsNoContent()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            Assert.Null(testOrganisation.ParentId);
            Assert.Null(testOrganisation.Parent);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationParentResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(NotAdminOfChildAndParentOrganisations))]
        public async Task SuperUserCanRemoveParentFromOrganisationRegardlessOfMembership(
            bool memberOfChild, bool adminOfChild, bool memberOfParent, bool adminOfParent)
        {
            // Arrange
            Organisation parentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationResources.SetOrganisationParent(
                testOrganisation, parentOrganisation, DbIdentifier);

            if (memberOfChild)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    testOrganisation.Id, TestUserPersonId, DbIdentifier, adminOfChild);
            }

            if (memberOfParent)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    parentOrganisation.Id, TestUserPersonId, DbIdentifier, adminOfParent);
            }

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationParentResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Null(response.ResponseBody);

            Assert.False(OrganisationResources.CheckOrganisationBelongsToParent(
                testOrganisation.Id, parentOrganisation.Id, _fixture.DbIdentifier));
        }

        [Theory]
        [MemberData(nameof(NotAdminOfChildAndParentOrganisations))]
        public async Task OrganisationAdminCannotRemoveParentFromOrganisationIfTheyAreNotAdminOfBoth(
            bool memberOfChild, bool adminOfChild, bool memberOfParent, bool adminOfParent)
        {
            // Arrange
            Organisation parentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationResources.SetOrganisationParent(
                testOrganisation, parentOrganisation, DbIdentifier);

            if (memberOfChild)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    testOrganisation.Id, TestUserPersonId, DbIdentifier, adminOfChild);
            }

            if (memberOfParent)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    parentOrganisation.Id, TestUserPersonId, DbIdentifier, adminOfParent);
            }

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationParentResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        [Fact]
        public async Task NonSuperUserCanRemoveParentFromOrganisationIfTheyAreAdminOfBoth()
        {
            // Arrange
            Organisation parentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(
                DbIdentifier, parentOrganisation.Id);

            Assert.True(OrganisationResources.CheckOrganisationBelongsToParent(
                testOrganisation.Id, parentOrganisation.Id, DbIdentifier));

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                parentOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationParentResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Null(response.ResponseBody);

            Assert.False(OrganisationResources.CheckOrganisationBelongsToParent(
                testOrganisation.Id, parentOrganisation.Id, _fixture.DbIdentifier));
        }
    }
}
