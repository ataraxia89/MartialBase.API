// <copyright file="DeleteArtGradeTests.cs" company="Martialtech®">
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

namespace MartialBase.API.LiveControllerTests.ArtGradesControllerTests
{
    [Collection("LiveControllerTests")]
    public class DeleteArtGradeTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteArtGradeTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public DeleteArtGradeTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task DeleteArtGradeWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await ArtGradesController.DeleteArtGradeResponseAsync(
                _fixture.Client, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Fact]
        public async Task BlockedOrganisationAdminCannotDeleteArtGrade()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await ArtGradesController.DeleteArtGradeResponseAsync(
                _fixture.Client,
                ArtGradeResources.CreateTestArtGrade(DbIdentifier).Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        [Fact]
        public async Task DeleteNonExistentArtGradeReturnsNotFound()
        {
            // Arrange
            string invalidArtGradeId = Guid.NewGuid().ToString();

            // Act
            HttpResponseModel response = await ArtGradesController.DeleteArtGradeResponseAsync(
                _fixture.Client, invalidArtGradeId);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Art grade ID '{invalidArtGradeId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task OrganisationAdminCannotDeleteArtGradeForOrganisationOfWhichTheyAreNotAMember()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testArtGrade.OrganisationId, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await ArtGradesController.DeleteArtGradeResponseAsync(
                _fixture.Client, testArtGrade.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        [Fact]
        public async Task OrganisationAdminCannotDeleteArtGradeForOrganisationOfWhichTheyAreNotAdmin()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testArtGrade.OrganisationId, TestUserPersonId, DbIdentifier, isAdmin: false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await ArtGradesController.DeleteArtGradeResponseAsync(
                _fixture.Client, testArtGrade.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        [Fact]
        public async Task SuperUserCanDeleteArtGradeForOrganisationOfWhichTheyAreNotAMember()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testArtGrade.OrganisationId, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            await ArtGradesController.DeleteArtGradeAsync(_fixture.Client, testArtGrade.Id.ToString());

            // Assert
            ArtGradeResources.AssertDoesNotExist(testArtGrade.Id, DbIdentifier);
        }

        [Fact]
        public async Task SuperUserCanDeleteArtGradeForOrganisationOfWhichTheyAreNotAdmin()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testArtGrade.OrganisationId, TestUserPersonId, DbIdentifier, isAdmin: false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            await ArtGradesController.DeleteArtGradeAsync(_fixture.Client, testArtGrade.Id.ToString());

            // Assert
            ArtGradeResources.AssertDoesNotExist(testArtGrade.Id, DbIdentifier);
        }

        [Fact]
        public async Task OrganisationAdminCanDeleteArtGrade()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testArtGrade.OrganisationId, TestUserPersonId, DbIdentifier, isAdmin: true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            await ArtGradesController.DeleteArtGradeAsync(_fixture.Client, testArtGrade.Id.ToString());

            // Assert
            ArtGradeResources.AssertDoesNotExist(testArtGrade.Id, DbIdentifier);
        }
    }
}
