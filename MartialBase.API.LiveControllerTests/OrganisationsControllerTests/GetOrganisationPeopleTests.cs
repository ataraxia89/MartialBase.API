// <copyright file="GetOrganisationPeopleTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.OrganisationsControllerTests
{
    [Collection("LiveControllerTests")]
    public class GetOrganisationPeopleTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetOrganisationPeopleTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetOrganisationPeopleTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task GetOrganisationPeopleWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await OrganisationsController.GetOrganisationPeopleResponseAsync(
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
        public async Task NonOrganisationAdminUserCannotGetOrganisationPeople(string roleName)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier);

            // This test is specifically for the OrganisationAdmin role, so the user can be admin of an
            // organisation, but they still must have this role assigned to them to carry out functions
            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            OrganisationPersonResources.EnsureOrganisationHasPeople(
                testOrganisation.Id, testPeople.Select(p => p.Id).ToList(), DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.GetOrganisationPeopleResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Fact]
        public async Task BlockedOrganisationAdminUserCannotGetOrganisationPeople()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPeople(
                testOrganisation.Id, testPeople.Select(p => p.Id).ToList(), DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await OrganisationsController.GetOrganisationPeopleResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        [Fact]
        public async Task GetPeopleFromNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            var invalidOrganisationId = Guid.NewGuid();

            OrganisationResources.AssertDoesNotExist(invalidOrganisationId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.GetOrganisationPeopleResponseAsync(
                _fixture.Client, invalidOrganisationId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Organisation ID '{invalidOrganisationId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task GetOrganisationPeopleWhenRequestingUserIsNotAnOrganisationAdminReturnsForbidden()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPeople(
                testOrganisation.Id, testPeople.Select(p => p.Id).ToList(), DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.GetOrganisationPeopleResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        [Fact]
        public async Task OrganisationAdminCanGetOrganisationPeople()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation otherOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier);
            List<Person> otherPeople = PersonResources.CreateTestPeople(10, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPeople(
                testOrganisation.Id, testPeople.Select(p => p.Id).ToList(), DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPeople(
                otherOrganisation.Id, otherPeople.Select(p => p.Id).ToList(), DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePeople(
                testOrganisation.Id, otherPeople.Select(p => p.Id).ToList(), DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePeople(
                otherOrganisation.Id, testPeople.Select(p => p.Id).ToList(), DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            testPeople.Add(_fixture.TestUser.Person);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            List<OrganisationPersonDTO> retrievedOrganisationPeople =
                await OrganisationsController.GetOrganisationPeopleAsync(
                    _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            PersonResources.AssertEqual(testPeople, retrievedOrganisationPeople);

            foreach (Person otherPerson in otherPeople)
            {
                Assert.Null(retrievedOrganisationPeople.FirstOrDefault(o =>
                    o.Person.Id == otherPerson.Id));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SuperUserCanGetOrganisationPeopleRegardlessOfOrganisationMembership(bool isMember)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation otherOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier);
            List<Person> otherPeople = PersonResources.CreateTestPeople(10, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPeople(
                testOrganisation.Id, testPeople.Select(p => p.Id).ToList(), DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPeople(
                otherOrganisation.Id, otherPeople.Select(p => p.Id).ToList(), DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePeople(
                testOrganisation.Id, otherPeople.Select(p => p.Id).ToList(), DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePeople(
                otherOrganisation.Id, testPeople.Select(p => p.Id).ToList(), DbIdentifier);

            if (isMember)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    testOrganisation.Id, TestUserPersonId, DbIdentifier);

                testPeople.Add(_fixture.TestUser.Person);
            }
            else
            {
                OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                    testOrganisation.Id, TestUserPersonId, DbIdentifier);
            }

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            List<OrganisationPersonDTO> retrievedOrganisationPeople =
                await OrganisationsController.GetOrganisationPeopleAsync(
                    _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            PersonResources.AssertEqual(testPeople, retrievedOrganisationPeople);

            foreach (Person otherPerson in otherPeople)
            {
                Assert.Null(retrievedOrganisationPeople.FirstOrDefault(o =>
                    o.Person.Id == otherPerson.Id));
            }
        }
    }
}
