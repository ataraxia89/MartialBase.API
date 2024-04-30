// <copyright file="DeleteOrganisationTests.cs" company="Martialtech®">
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
    public class DeleteOrganisationTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteOrganisationTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public DeleteOrganisationTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task DeleteOrganisationWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await OrganisationsController.DeleteOrganisationResponseAsync(
                _fixture.Client, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonOrganisationAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationAdminUserCannotDeleteOrganisation(string roleName)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            // This test is specifically for the OrganisationAdmin role, so the user can be admin of an
            // organisation, but they still must have this role assigned to them to carry out functions
            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.DeleteOrganisationResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);

            Assert.True(OrganisationResources.CheckExists(testOrganisation.Id, DbIdentifier));
        }

        [Fact]
        public async Task DeleteOrganisationAlsoRemovesOrganisationPeople()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation otherOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier, false);

            foreach (Person testPerson in testPeople)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    testOrganisation.Id, testPerson.Id, DbIdentifier);

                // Test people also need to be added to another organisation to avoid orphan entity errors
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    otherOrganisation.Id, testPerson.Id, DbIdentifier);
            }

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.DeleteOrganisationResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Null(response.ResponseBody);

            Assert.False(OrganisationResources.CheckExists(testOrganisation.Id, DbIdentifier));

            foreach (Person testPerson in testPeople)
            {
                (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                    testOrganisation.Id, testPerson.Id, DbIdentifier);

                Assert.False(personExists);
            }
        }

        [Fact]
        public async Task BlockedOrganisationAdminUserCannotDeleteOrganisation()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await OrganisationsController.DeleteOrganisationResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);

            Assert.True(OrganisationResources.CheckExists(testOrganisation.Id, DbIdentifier));
        }

        [Fact]
        public async Task DeleteNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            var invalidOrganisationId = Guid.NewGuid();

            OrganisationResources.AssertDoesNotExist(invalidOrganisationId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.DeleteOrganisationResponseAsync(
                _fixture.Client, invalidOrganisationId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Organisation ID '{invalidOrganisationId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task DeleteOrganisationWhenRequestingUserIsNotAnOrganisationAdminReturnsForbidden()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.DeleteOrganisationResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);

            Assert.True(OrganisationResources.CheckExists(testOrganisation.Id, DbIdentifier));
        }

        [Fact]
        public async Task DeleteOrganisationWhenPersonOnlyHasOneOrganisationReturnsOrphanEntityBadRequest()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation otherOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier);

            foreach (Person testPerson in testPeople)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    testOrganisation.Id, testPerson.Id, DbIdentifier);
            }

            // This will ensure all but one test person belongs to another organisation, meaning one would
            // be left an orphan entity if the test organisation is removed
            for (int i = 1; i < 10; i++)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    otherOrganisation.Id, testPeople[i].Id, DbIdentifier);
            }

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.DeleteOrganisationResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorResponseCode.OrphanPersonEntity, response.ErrorResponseCode);

            Assert.True(OrganisationResources.CheckExists(testOrganisation.Id, DbIdentifier));
        }

        [Fact]
        public async Task DeleteOrganisationWhenSchoolBelongsToOrganisationReturnsOrphanEntityBadRequest()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            SchoolResources.EnsureSchoolBelongsToOrganisation(testSchool, testOrganisation, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.DeleteOrganisationResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorResponseCode.OrphanSchoolEntity, response.ErrorResponseCode);

            Assert.True(OrganisationResources.CheckExists(testOrganisation.Id, DbIdentifier));
        }

        [Fact]
        public async Task OrganisationAdminCanDeleteOrganisation()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation secondOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            // This is needed to avoid an orphan entity (requesting user would have no organisation)
            OrganisationPersonResources.EnsureOrganisationHasPerson(
                secondOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.DeleteOrganisationResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Null(response.ResponseBody);

            Assert.False(OrganisationResources.CheckExists(testOrganisation.Id, DbIdentifier));

            (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            Assert.False(personExists);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SuperUserCanDeleteOrganisationRegardlessOfOrganisationMembership(bool isMember)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation secondOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            if (isMember)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    testOrganisation.Id, TestUserPersonId, DbIdentifier);

                // This is needed to avoid an orphan entity (requesting super user would have no organisation)
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    secondOrganisation.Id, TestUserPersonId, DbIdentifier);
            }
            else
            {
                OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                    testOrganisation.Id, TestUserPersonId, DbIdentifier);
            }

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.DeleteOrganisationResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Null(response.ResponseBody);

            Assert.False(OrganisationResources.CheckExists(testOrganisation.Id, DbIdentifier));

            (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            Assert.False(personExists);
        }
    }
}
