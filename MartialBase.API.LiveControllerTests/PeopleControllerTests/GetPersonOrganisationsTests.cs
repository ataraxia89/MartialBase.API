// <copyright file="GetPersonOrganisationsTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using MartialBase.API.Controllers;
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

namespace MartialBase.API.LiveControllerTests.PeopleControllerTests
{
    /// <summary>
    /// Live controller tests for <see cref="Controllers.PeopleController.GetPersonOrganisationsAsync"/>.
    /// </summary>
    [Collection("LiveControllerTests")]
    public class GetPersonOrganisationsTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPersonOrganisationsTests"/> class.
        /// </summary>
        /// <param name="fixture">The <see cref="TestServerFixture"/> to be used by tests.</param>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        public GetPersonOrganisationsTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        /// <summary>
        /// Ensures that a request to get a list of <see cref="Organisation">Organisations</see> for a
        /// <see cref="Person"/> with an expired authorization token returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result.
        /// </summary>
        [Fact]
        public async Task GetPersonOrganisationsWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonOrganisationsResponseAsync(
                _fixture.Client, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="Organisation">Organisations</see> for a
        /// <see cref="Person"/> from a non-admin user returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result.
        /// </summary>
        /// <param name="roleName">The <see cref="UserRole"/> to use in the request.</param>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonPersonAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonPersonAdminUserCannotGetPersonOrganisations(string roleName)
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonOrganisationsResponseAsync(
                _fixture.Client, testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="Organisation">Organisations</see> for a
        /// <see cref="Person"/> from a non-admin user is successful where the
        /// <see cref="Person"/> being queried is the same as the requesting user.
        /// </summary>
        /// <param name="roleName">The <see cref="UserRole"/> to use in the request.</param>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonPersonAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonPersonAdminUserCanGetTheirOwnPersonOrganisations(string roleName)
        {
            // Arrange
            List<Organisation> testOrganisations =
                OrganisationResources.CreateTestOrganisations(5, _fixture.DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationsHavePerson(
                testOrganisations.Select(o => o.Id).ToList(),
                TestUserPersonId,
                _fixture.DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            List<PersonOrganisationDTO> retrievedPersonOrganisations =
                await API.TestTools.ControllerMethods.PeopleController.GetPersonOrganisationsAsync(_fixture.Client, TestUserPersonId.ToString());

            // Assert
            OrganisationResources.AssertEqual(testOrganisations, retrievedPersonOrganisations);

            foreach (PersonOrganisationDTO personOrganisation in retrievedPersonOrganisations)
            {
                Assert.Null(personOrganisation.Person);
                Assert.False(personOrganisation.IsAdmin);
            }
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="Organisation">Organisations</see> for a
        /// <see cref="Person"/> from a super user is successful, even when the <see cref="Person"/> being
        /// queried is not the same as the requesting user, nor do they have <see cref="School"/> or
        /// <see cref="Organisation"/> access to the person, nor are they admin of any of the organisations.
        /// </summary>
        [Fact]
        public async Task SuperUserCanGetPersonOrganisationsRegardlessOfAdmin()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            List<Organisation> testOrganisations =
                OrganisationResources.CreateTestOrganisations(5, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationsHavePerson(
                testOrganisations.Select(o => o.Id).ToList(),
                testPerson.Id,
                DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            List<PersonOrganisationDTO> retrievedPersonOrganisations =
                await API.TestTools.ControllerMethods.PeopleController.GetPersonOrganisationsAsync(_fixture.Client, testPerson.Id.ToString());

            // Assert
            OrganisationResources.AssertEqual(testOrganisations, retrievedPersonOrganisations);

            foreach (PersonOrganisationDTO personOrganisation in retrievedPersonOrganisations)
            {
                Assert.Null(personOrganisation.Person);
                Assert.False(personOrganisation.IsAdmin);
            }
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="Organisation">Organisations</see> for a
        /// <see cref="Person"/> from a <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see> user is
        /// successful where the requesting user is a secretary of the <see cref="School"/> that the
        /// <see cref="Person"/> being queried is a student of.
        /// </summary>
        [Fact]
        public async Task SchoolSecretaryCanGetPersonOrganisations()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            List<Organisation> testOrganisations =
                OrganisationResources.CreateTestOrganisations(5, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationsHavePerson(
                testOrganisations.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationsHavePerson(
                testOrganisations.Select(o => o.Id).ToList(), testPerson.Id, DbIdentifier);

            School testSchool = SchoolResources.CreateTestSchool(_fixture.DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, TestUserPersonId, DbIdentifier, false, true);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            List<PersonOrganisationDTO> retrievedPersonOrganisations =
                await API.TestTools.ControllerMethods.PeopleController.GetPersonOrganisationsAsync(_fixture.Client, testPerson.Id.ToString());

            // Assert
            OrganisationResources.AssertEqual(testOrganisations, retrievedPersonOrganisations);

            foreach (PersonOrganisationDTO personOrganisation in retrievedPersonOrganisations)
            {
                Assert.Null(personOrganisation.Person);
                Assert.False(personOrganisation.IsAdmin);
            }
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="Organisation">Organisations</see> for a
        /// <see cref="Person"/> from a non-super user with no associated <see cref="Person"/> record returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result.
        /// </summary>
        /// <param name="roleName">The name of the <see cref="UserRole"/> to be assigned to the test user.</param>
        [Theory]
        [InlineData(UserRoles.SchoolSecretary)]
        [InlineData(UserRoles.OrganisationAdmin)]
        public async Task NonSuperUserCannotGetPersonOrganisationsIfTheyAreBlocked(string roleName)
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonOrganisationsResponseAsync(
                _fixture.Client, testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="Organisation">Organisations</see> for a
        /// <see cref="Person"/> from an <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see>
        /// user is successful where the requesting user is an admin of at least one of the
        /// <see cref="Organisation">Organisations</see> that the <see cref="Person"/> being queried is a
        /// member of.
        /// </summary>
        [Fact]
        public async Task OrganisationAdminCanGetPersonOrganisationsThatTheyAreNotAdminOf()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            List<Organisation> testOrganisationsWithMembership =
                OrganisationResources.CreateTestOrganisations(5, DbIdentifier);

            List<Organisation> testOrganisationsWithoutMembership =
                OrganisationResources.CreateTestOrganisations(5, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationsHavePerson(
                testOrganisationsWithMembership.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisationsWithMembership.First().Id, TestUserPersonId, DbIdentifier, true);

            OrganisationPersonResources.EnsureOrganisationsDoNotHavePerson(
                testOrganisationsWithoutMembership.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationsHavePerson(
                testOrganisationsWithMembership.Select(o => o.Id).ToList(), testPerson.Id, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationsHavePerson(
                testOrganisationsWithoutMembership.Select(o => o.Id).ToList(), testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            List<PersonOrganisationDTO> retrievedPersonOrganisations =
                await API.TestTools.ControllerMethods.PeopleController.GetPersonOrganisationsAsync(_fixture.Client, testPerson.Id.ToString());

            // Assert
            OrganisationResources.AssertEqual(testOrganisationsWithMembership, retrievedPersonOrganisations);

            foreach (PersonOrganisationDTO personOrganisation in retrievedPersonOrganisations)
            {
                Assert.Null(personOrganisation.Person);
                Assert.False(personOrganisation.IsAdmin);
            }
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="Organisation">Organisations</see> for a
        /// <see cref="Person"/> from a non-admin user returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result, even if the requesting user is a member
        /// of the same <see cref="Organisation"/> as the <see cref="Person"/> being queried.
        /// </summary>
        /// <param name="roleName">The non-admin role to use in the request.</param>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonPersonAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonPersonAdminUserCannotGetPersonOrganisationsRegardlessOfOrganisationMembership(string roleName)
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonOrganisationsResponseAsync(
                _fixture.Client, testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="Organisation">Organisations</see> for a
        /// <see cref="Person"/> from an <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result if the requesting user is not an admin
        /// of the <see cref="Organisation"/> to which the <see cref="Person"/> being queried belongs.
        /// </summary>
        [Fact]
        public async Task OrganisationAdminCannotGetPersonOrganisationsIfTheyAreNotAdminOfThePersonsOrganisation()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            Organisation otherOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                otherOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                otherOrganisation.Id, testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonOrganisationsResponseAsync(
                _fixture.Client, testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NoAccessToPerson, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="Organisation">Organisations</see> for a
        /// <see cref="Person"/> from a <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see> returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result if the requesting user is not a secretary
        /// of the <see cref="School"/> to which the <see cref="Person"/> being queried belongs.
        /// </summary>
        [Fact]
        public async Task SchoolSecretaryCannotGetPersonOrganisationsIfTheyAreNotSchoolSecretaryOfThePersonsSchool()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            School otherSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, TestUserPersonId, DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                otherSchool.Id, TestUserPersonId, DbIdentifier, false, true);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testPerson.Id, DbIdentifier);

            SchoolStudentResources.EnsureSchoolDoesNotHaveStudent(
                otherSchool.Id, testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonOrganisationsResponseAsync(
                _fixture.Client, testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NoAccessToPerson, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="Organisation">Organisations</see> for a
        /// non-existent <see cref="Person"/> returns a <see cref="HttpStatusCode.NotFound">NotFound</see>
        /// result.
        /// </summary>
        [Fact]
        public async Task GetPersonOrganisationsForNonExistentPersonReturnsNotFound()
        {
            // Arrange
            var invalidPersonId = Guid.NewGuid();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonOrganisationsResponseAsync(
                _fixture.Client, invalidPersonId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Person ID '{invalidPersonId}' not found.", response.ResponseBody);
        }
    }
}
