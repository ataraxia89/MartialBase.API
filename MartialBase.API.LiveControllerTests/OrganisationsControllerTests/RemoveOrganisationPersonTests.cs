// <copyright file="RemoveOrganisationPersonTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
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
    public class RemoveOrganisationPersonTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveOrganisationPersonTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public RemoveOrganisationPersonTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task RemoveOrganisationPersonWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationPersonResponseAsync(
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
        public async Task NonOrganisationAdminUserCannotRemoveOrganisationPerson(string roleName)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier);

            // This test is specifically for the OrganisationAdmin role, so the user can be admin of an
            // organisation, but they still must have this role assigned to them to carry out functions
            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationPersonResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);

            (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            Assert.True(personExists);
        }

        [Fact]
        public async Task BlockedOrganisationAdminUserCannotRemoveOrganisationPerson()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationPersonResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);

            (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            Assert.True(personExists);
        }

        [Fact]
        public async Task RemovePersonFromNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier);
            var invalidOrganisationId = Guid.NewGuid();

            OrganisationResources.AssertDoesNotExist(invalidOrganisationId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationPersonResponseAsync(
                _fixture.Client,
                invalidOrganisationId.ToString(),
                testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Organisation ID '{invalidOrganisationId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task RemoveNonExistentPersonFromOrganisationReturnsNotFound()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            var invalidPersonId = Guid.NewGuid();

            Assert.False(PersonResources.CheckExists(invalidPersonId, DbIdentifier));

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationPersonResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                invalidPersonId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Person ID '{invalidPersonId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task RemoveOrganisationPersonWhenRequestingUserIsNotAnOrganisationAdminReturnsForbidden()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationPersonResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);

            (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            Assert.True(personExists);
        }

        [Fact]
        public async Task RemoveOrganisationPersonWhenPersonOnlyHasOneOrganisationReturnsOrphanEntityBadRequest()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.RemoveOrganisationPersonResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorResponseCode.OrphanPersonEntity, response.ErrorResponseCode);

            (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            Assert.True(personExists);
        }

        [Fact]
        public async Task OrganisationAdminCanRemoveOrganisationPerson()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation secondOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            // This is required to avoid an orphan entity exception as the user would otherwise have no other
            // organisation to be a member of
            OrganisationPersonResources.EnsureOrganisationHasPerson(
                secondOrganisation.Id, testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            // Assert
            Assert.True(
                await OrganisationsController.RemoveOrganisationPersonAsync(
                    _fixture.Client,
                    testOrganisation.Id.ToString(),
                    testPerson.Id.ToString()));

            (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            Assert.False(personExists);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SuperUserCanRemoveOrganisationPersonRegardlessOfOrganisationMembership(bool isMember)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation secondOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            if (isMember)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    testOrganisation.Id, TestUserPersonId, DbIdentifier);
            }
            else
            {
                OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                    testOrganisation.Id, TestUserPersonId, DbIdentifier);
            }

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            // This is required to avoid an orphan entity exception as the user would otherwise have no other
            // organisation to be a member of
            OrganisationPersonResources.EnsureOrganisationHasPerson(
                secondOrganisation.Id, testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            // Assert
            Assert.True(
                await OrganisationsController.RemoveOrganisationPersonAsync(
                    _fixture.Client,
                    testOrganisation.Id.ToString(),
                    testPerson.Id.ToString()));

            (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            Assert.False(personExists);
        }
    }
}
