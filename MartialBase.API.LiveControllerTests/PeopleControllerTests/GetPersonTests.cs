// <copyright file="GetPersonTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.PeopleControllerTests
{
    [Collection("LiveControllerTests")]
    public class GetPersonTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPersonTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetPersonTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task GetPersonWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await PeopleController.GetPersonResponseAsync(_fixture.Client, null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonPersonAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonPersonAdminUserCannotGetPerson(string roleName)
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await PeopleController.GetPersonResponseAsync(
                _fixture.Client, testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonPersonAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonPersonAdminUserCanGetTheirOwnPersonDetails(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            PersonDTO returnedPerson =
                await PeopleController.GetPersonAsync(_fixture.Client, TestUserPersonId.ToString());

            // Assert
            PersonResources.AssertEqual(_fixture.TestUser.Person, returnedPerson);
        }

        [Fact]
        public async Task SystemAdminUserCannotGetPerson()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SystemAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await PeopleController.GetPersonResponseAsync(
                _fixture.Client, testPerson.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Fact]
        public async Task SuperUserCanGetPerson()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            PersonDTO returnedPerson =
                await PeopleController.GetPersonAsync(_fixture.Client, TestUserPersonId.ToString());

            // Assert
            PersonResources.AssertEqual(_fixture.TestUser.Person, returnedPerson);
        }

        [Fact]
        public async Task OrganisationAdminUserCanGetPerson()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(_fixture.DbIdentifier, false);

            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(_fixture.DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, _fixture.DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, _fixture.DbIdentifier, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            PersonDTO returnedPerson =
                await PeopleController.GetPersonAsync(_fixture.Client, testPerson.Id.ToString());

            // Assert
            PersonResources.AssertEqual(testPerson, returnedPerson);
        }

        [Fact]
        public async Task SchoolSecretaryCanGetPerson()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(_fixture.DbIdentifier, false);

            School testSchool = SchoolResources.CreateTestSchool(_fixture.DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testPerson.Id, _fixture.DbIdentifier);

            SchoolResources.EnsurePersonIsSecretary(testSchool, _fixture.TestUser.Person, _fixture.DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            // Act
            PersonDTO returnedPerson =
                await PeopleController.GetPersonAsync(_fixture.Client, testPerson.Id.ToString());

            // Assert
            PersonResources.AssertEqual(testPerson, returnedPerson);
        }

        [Fact]
        public async Task GetNonExistentPersonReturnsNotFound()
        {
            // Arrange
            var invalidPersonId = Guid.NewGuid();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await PeopleController.GetPersonResponseAsync(
                _fixture.Client, invalidPersonId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Person ID '{invalidPersonId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task GetPersonWithNoPersonIdParameterReturnsBadRequest()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await PeopleController.GetPersonResponseAsync(_fixture.Client, null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Null(response.ResponseBody);
        }
    }
}
