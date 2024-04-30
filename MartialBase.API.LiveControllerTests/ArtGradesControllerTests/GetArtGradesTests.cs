// <copyright file="GetArtGradesTests.cs" company="Martialtech®">
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
    public class GetArtGradesTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetArtGradesTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetArtGradesTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task GetArtGradesWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradesResponseAsync(
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

        [Fact]
        public async Task GetArtGradesWithNoArtIdParameterReturnsBadRequest()
        {
            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradesResponseAsync(
                _fixture.Client,
                null,
                Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("No art ID parameter specified.", response.ResponseBody);
        }

        [Fact]
        public async Task GetArtGradesWithNoOrganisationIdParameterReturnsBadRequest()
        {
            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradesResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("No organisation ID parameter specified.", response.ResponseBody);
        }

        [Fact]
        public async Task GetArtGradesWithNonExistentArtIdReturnsNotFound()
        {
            // Arrange
            string invalidArtId = Guid.NewGuid().ToString();
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradesResponseAsync(
                _fixture.Client,
                invalidArtId,
                testOrganisationId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"Art ID '{invalidArtId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task GetArtGradesWithNonExistentOrganisationIdReturnsNotFound()
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            string invalidOrganisationId = Guid.NewGuid().ToString();

            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradesResponseAsync(
                _fixture.Client,
                testArtId.ToString(),
                invalidOrganisationId);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"Organisation ID '{invalidOrganisationId}' not found.", response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonOrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationUserCannotGetArtGrades(string roleName)
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            ArtGradeResources.CreateTestArtGrades(10, DbIdentifier, testArtId, testOrganisationId);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradesResponseAsync(
                _fixture.Client,
                testArtId.ToString(),
                testOrganisationId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.OrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task OrganisationUserCannotGetArtGradesForAnOrganisationOfWhichTheyAreNotAMember(string roleName)
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            ArtGradeResources.CreateTestArtGrades(10, DbIdentifier, testArtId, testOrganisationId);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testOrganisationId, TestUserPersonId, DbIdentifier);

            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradesResponseAsync(
                _fixture.Client,
                testArtId.ToString(),
                testOrganisationId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NoOrganisationAccess, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.AllUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task BlockedOrganisationUserCannotGetArtGrades(string roleName)
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            ArtGradeResources.CreateTestArtGrades(10, DbIdentifier, testArtId, testOrganisationId);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await ArtGradesController.GetArtGradesResponseAsync(
                _fixture.Client,
                testArtId.ToString(),
                testOrganisationId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        [Fact]
        public async Task SuperUserCanGetArtGradesForAnOrganisationOfWhichTheyAreNotAMember()
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            List<ArtGrade> testArtGrades = ArtGradeResources.CreateTestArtGrades(
                10, DbIdentifier, testArtId, testOrganisationId);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testOrganisationId, TestUserPersonId, DbIdentifier);

            // Act
            List<ArtGradeDTO> retrievedArtGrades = await ArtGradesController.GetArtGradesAsync(
                _fixture.Client, testArtId.ToString(), testOrganisationId.ToString());

            // Assert
            ArtGradeResources.AssertEqual(testArtGrades, retrievedArtGrades);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.OrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task OrganisationMemberCanGetArtGrades(string roleName)
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            List<ArtGrade> testArtGrades = ArtGradeResources.CreateTestArtGrades(
                10, DbIdentifier, testArtId, testOrganisationId);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisationId, TestUserPersonId, DbIdentifier);

            // Act
            List<ArtGradeDTO> retrievedArtGrades = await ArtGradesController.GetArtGradesAsync(
                _fixture.Client, testArtId.ToString(), testOrganisationId.ToString());

            // Assert
            ArtGradeResources.AssertEqual(testArtGrades, retrievedArtGrades);
        }
    }
}
