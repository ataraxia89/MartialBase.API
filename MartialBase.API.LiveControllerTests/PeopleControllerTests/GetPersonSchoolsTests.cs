// <copyright file="GetPersonSchoolsTests.cs" company="Martialtech®">
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
    /// Live controller tests for <see cref="Controllers.PeopleController.GetPersonSchoolsAsync"/>.
    /// </summary>
    [Collection("LiveControllerTests")]
    public class GetPersonSchoolsTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPersonSchoolsTests"/> class.
        /// </summary>
        /// <param name="fixture">The <see cref="TestServerFixture"/> to be used by tests.</param>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        public GetPersonSchoolsTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        /// <summary>
        /// Ensures that a request to get a list of <see cref="School">Schools</see> for a
        /// <see cref="Person"/> with an expired authorization token returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result.
        /// </summary>
        [Fact]
        public async Task GetPersonSchoolsWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonSchoolsResponseAsync(
                _fixture.Client, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="School">Schools</see> for a
        /// <see cref="Person"/> from a non-admin user returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result.
        /// </summary>
        /// <param name="roleName">The <see cref="UserRole"/> to use in the request.</param>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonPersonAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonPersonAdminUserCannotGetPersonSchools(string roleName)
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonSchoolsResponseAsync(
                _fixture.Client, testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="School">Schools</see> for a
        /// <see cref="Person"/> from a non-admin user is successful where the
        /// <see cref="Person"/> being queried is the same as the requesting user.
        /// </summary>
        /// <param name="roleName">The <see cref="UserRole"/> to use in the request.</param>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonPersonAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonPersonAdminUserCanGetTheirOwnPersonSchools(string roleName)
        {
            // Arrange
            List<School> testSchools = SchoolResources.CreateTestSchools(5, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsHaveStudent(
                testSchools.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            List<StudentSchoolDTO> retrievedPersonSchools =
                await API.TestTools.ControllerMethods.PeopleController.GetPersonSchoolsAsync(_fixture.Client, TestUserPersonId.ToString());

            // Assert
            SchoolResources.AssertEqual(testSchools, retrievedPersonSchools);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="School">Schools</see> for a
        /// <see cref="Person"/> from a super user is successful, even when the <see cref="Person"/> being
        /// queried is not the same as the requesting user, nor do they have <see cref="School"/> or
        /// <see cref="Organisation"/> access to the person, nor are they admin of any of the schools.
        /// </summary>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Fact]
        public async Task SuperUserCanGetPersonSchoolsRegardlessOfAdmin()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            List<School> testSchools = SchoolResources.CreateTestSchools(5, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsHaveStudent(
                testSchools.Select(o => o.Id).ToList(), testPerson.Id, DbIdentifier, isSecretary: false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            List<StudentSchoolDTO> retrievedPersonSchools =
                await API.TestTools.ControllerMethods.PeopleController.GetPersonSchoolsAsync(_fixture.Client, testPerson.Id.ToString());

            // Assert
            SchoolResources.AssertEqual(testSchools, retrievedPersonSchools);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="School">Schools</see> for a
        /// <see cref="Person"/> from a <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see> user is
        /// successful where the requesting user is a secretary of the <see cref="School"/> that the
        /// <see cref="Person"/> being queried is a student of.
        /// </summary>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Fact]
        public async Task SchoolSecretaryCanGetPersonSchools()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            List<School> testSchools = SchoolResources.CreateTestSchools(5, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsHaveStudent(
                testSchools.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsHaveStudent(
                testSchools.Select(o => o.Id).ToList(), testPerson.Id, DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchools.First().Id, TestUserPersonId, DbIdentifier, isSecretary: true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            // Act
            List<StudentSchoolDTO> retrievedPersonSchools =
                await API.TestTools.ControllerMethods.PeopleController.GetPersonSchoolsAsync(_fixture.Client, testPerson.Id.ToString());

            // Assert
            SchoolResources.AssertEqual(testSchools, retrievedPersonSchools);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="School">Schools</see> for a
        /// <see cref="Person"/> from a non-super user with no associated <see cref="Person"/> record returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result.
        /// </summary>
        /// <param name="roleName">The name of the <see cref="UserRole"/> to be assigned to the test user.</param>
        [Theory]
        [InlineData(UserRoles.SchoolSecretary)]
        [InlineData(UserRoles.OrganisationAdmin)]
        public async Task NonSuperUserCannotGetPersonSchoolsIfTheyAreBlocked(string roleName)
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonSchoolsResponseAsync(
                _fixture.Client, testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="School">Schools</see> for a
        /// <see cref="Person"/> from a <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see>
        /// user is successful where the requesting user is a secretary of at least one of the
        /// <see cref="School">Schools</see> that the <see cref="Person"/> being queried is a
        /// member of.
        /// </summary>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Fact]
        public async Task SchoolSecretaryCanGetPersonSchoolsThatTheyAreNotSecretaryOf()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            List<School> testSchoolsWithMembership = SchoolResources.CreateTestSchools(5, DbIdentifier);

            List<School> testSchoolsWithoutMembership = SchoolResources.CreateTestSchools(5, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsHaveStudent(
                testSchoolsWithMembership.Select(o => o.Id).ToList(),
                TestUserPersonId,
                DbIdentifier,
                isSecretary: false);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchoolsWithMembership.First().Id,
                TestUserPersonId,
                DbIdentifier,
                isSecretary: true);

            SchoolStudentResources.EnsureSchoolsDoNotHaveStudent(
                testSchoolsWithoutMembership.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsHaveStudent(
                testSchoolsWithMembership.Select(o => o.Id).ToList(), testPerson.Id, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsHaveStudent(
                testSchoolsWithoutMembership.Select(o => o.Id).ToList(), testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            // Act
            List<StudentSchoolDTO> retrievedPersonSchools =
                await API.TestTools.ControllerMethods.PeopleController.GetPersonSchoolsAsync(_fixture.Client, testPerson.Id.ToString());

            // Assert
            SchoolResources.AssertEqual(testSchoolsWithMembership, retrievedPersonSchools);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="School">Schools</see> for a
        /// <see cref="Person"/> from a non-admin user returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result, even if the requesting user is a member
        /// of the same <see cref="School"/> as the <see cref="Person"/> being queried.
        /// </summary>
        /// <param name="roleName">The non-admin role to use in the request.</param>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonPersonAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonPersonAdminUserCannotGetPersonSchoolsRegardlessOfSchoolMembership(string roleName)
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, TestUserPersonId, DbIdentifier, isSecretary: false);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testPerson.Id, DbIdentifier, isSecretary: false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonSchoolsResponseAsync(
                _fixture.Client, testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="School">Schools</see> for a
        /// <see cref="Person"/> from an <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see>
        /// user is successful where the requesting user is an admin of at least one of the
        /// <see cref="Organisation">Organisations</see> that the <see cref="Person"/> being queried is a
        /// member of.
        /// </summary>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Fact]
        public async Task OrganisationAdminCanGetPersonSchoolsOfWhichTheyAreAMember()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            List<School> testSchoolsWithMembership =
                SchoolResources.CreateTestSchools(5, DbIdentifier);

            List<School> testSchoolsWithoutMembership =
                SchoolResources.CreateTestSchools(5, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsHaveStudent(
                testSchoolsWithMembership.Select(o => o.Id).ToList(),
                TestUserPersonId,
                DbIdentifier,
                isSecretary: false);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsDoNotHaveStudent(
                testSchoolsWithoutMembership.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsHaveStudent(
                testSchoolsWithMembership.Select(o => o.Id).ToList(), testPerson.Id, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsHaveStudent(
                testSchoolsWithoutMembership.Select(o => o.Id).ToList(), testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            List<StudentSchoolDTO> retrievedPersonSchools =
                await API.TestTools.ControllerMethods.PeopleController.GetPersonSchoolsAsync(_fixture.Client, testPerson.Id.ToString());

            // Assert
            SchoolResources.AssertEqual(testSchoolsWithMembership, retrievedPersonSchools);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="School">Schools</see> for a
        /// <see cref="Person"/> from an <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result if the requesting user is not an admin
        /// of the <see cref="Organisation"/> to which the <see cref="Person"/> being queried belongs.
        /// </summary>
        [Fact]
        public async Task OrganisationAdminCannotGetPersonSchoolsIfTheyAreNotAdminOfThePersonsOrganisation()
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
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonSchoolsResponseAsync(
                _fixture.Client, testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NoAccessToPerson, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="School">Schools</see> for a
        /// <see cref="Person"/> from a <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see> returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result if the requesting user is not a secretary
        /// of the <see cref="School"/> to which the <see cref="Person"/> being queried belongs.
        /// </summary>
        [Fact]
        public async Task SchoolSecretaryCannotGetPersonSchoolsIfTheyAreNotSchoolSecretaryOfThePersonsSchool()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            School otherSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, TestUserPersonId, DbIdentifier, isSecretary: false);

            SchoolStudentResources.EnsureSchoolHasStudent(
                otherSchool.Id, TestUserPersonId, DbIdentifier, isSecretary: true);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testPerson.Id, DbIdentifier, isSecretary: false);

            SchoolStudentResources.EnsureSchoolDoesNotHaveStudent(
                otherSchool.Id, testPerson.Id, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonSchoolsResponseAsync(
                _fixture.Client, testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NoAccessToPerson, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to get a list of <see cref="School">Schools</see> for a
        /// non-existent <see cref="Person"/> returns a <see cref="HttpStatusCode.NotFound">NotFound</see>
        /// result.
        /// </summary>
        [Fact]
        public async Task GetPersonSchoolsForNonExistentPersonReturnsNotFound()
        {
            // Arrange
            var invalidPersonId = Guid.NewGuid();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.GetPersonSchoolsResponseAsync(
                _fixture.Client, invalidPersonId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Person ID '{invalidPersonId}' not found.", response.ResponseBody);
        }
    }
}
