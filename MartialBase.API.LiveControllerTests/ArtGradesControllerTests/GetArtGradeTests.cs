// <copyright file="GetArtGradeTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.ArtGrades;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.ArtGradesControllerTests
{
    [Collection("LiveControllerTests")]
    public class GetArtGradeTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetArtGradeTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetArtGradeTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task GetArtGradeWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradeResponseAsync(
                _fixture.Client, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Fact]
        public async Task GetNonExistentArtGradeReturnsNotFound()
        {
            // Arrange
            string invalidArtGradeId = Guid.NewGuid().ToString();

            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradeResponseAsync(
                _fixture.Client, invalidArtGradeId);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"Art grade ID '{invalidArtGradeId}' not found.", response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonOrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationUserCannotGetArtGrade(string roleName)
        {
            // Arrange
            Guid testArtGradeId = ArtGradeResources.CreateTestArtGrade(DbIdentifier).Id;

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradeResponseAsync(
                _fixture.Client, testArtGradeId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.OrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task OrganisationUserCannotGetArtGradeFromAnOrganisationOfWhichTheyAreNotAMember(string roleName)
        {
            // Arrange
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            Guid testArtGradeId = ArtGradeResources.CreateTestArtGrade(
                DbIdentifier, organisationId: testOrganisationId).Id;

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testOrganisationId, TestUserPersonId, DbIdentifier);

            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradeResponseAsync(
                _fixture.Client, testArtGradeId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NoOrganisationAccess, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.AllUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task BlockedOrganisationUserCannotGetArtGrade(string roleName)
        {
            // Arrange
            Guid testArtGradeId = ArtGradeResources.CreateTestArtGrade(DbIdentifier).Id;

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradeResponseAsync(
                _fixture.Client, testArtGradeId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        [Fact]
        public async Task SuperUserCanGetArtGradeFromAnOrganisationOfWhichTheyAreNotAMember()
        {
            // Arrange
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(
                DbIdentifier, organisationId: testOrganisationId);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testOrganisationId, TestUserPersonId, DbIdentifier);

            // Act
            ArtGradeDTO retrievedArtGrade = await ArtGradesController.GetArtGradeAsync(
                _fixture.Client, testArtGrade.Id.ToString());

            // Assert
            ArtGradeResources.AssertEqual(testArtGrade, retrievedArtGrade);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.OrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task OrganisationMemberCanGetArtGrade(string roleName)
        {
            // Arrange
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(
                DbIdentifier, organisationId: testOrganisationId);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisationId, TestUserPersonId, DbIdentifier);

            // Act
            ArtGradeDTO retrievedArtGrade = await ArtGradesController.GetArtGradeAsync(
                _fixture.Client, testArtGrade.Id.ToString());

            // Assert
            ArtGradeResources.AssertEqual(testArtGrade, retrievedArtGrade);
        }
    }
}
