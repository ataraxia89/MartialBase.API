// <copyright file="RemoveRoleFromUserTests.cs" company="Martialtech®">
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

namespace MartialBase.API.LiveControllerTests.MartialBaseUserControllerTests
{
    [Collection("LiveControllerTests")]
    public class RemoveRoleFromUserTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveRoleFromUserTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public RemoveRoleFromUserTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task RemoveRoleFromUserWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await MartialBaseUserController.RemoveRoleFromUserResponseAsync(
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
        [MemberData(nameof(UserRoleResources.NonSystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonSystemAdminCannotRemoveRoleFromUser(string roleName)
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            Guid testRoleId = UserRoleResources.GetUserRoleId(UserRoles.OrganisationAdmin, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(testUser.Id, testRoleId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.RemoveRoleFromUserResponseAsync(
                _fixture.Client,
                testUser.Id.ToString(),
                testRoleId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.SystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task SystemAdminCanRemoveRoleFromUser(string roleName)
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            Guid testRoleId = UserRoleResources.GetUserRoleId(UserRoles.OrganisationAdmin, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(testUser.Id, testRoleId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.RemoveRoleFromUserResponseAsync(
                _fixture.Client,
                testUser.Id.ToString(),
                testRoleId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Null(response.ResponseBody);

            MartialBaseUserRoleResources.AssertUserDoesNotHaveRole(testUser.Id, testRoleId, DbIdentifier);
        }

        [Fact]
        public async Task RemoveRoleFromNonExistentUserReturnsNotFound()
        {
            // Arrange
            string invalidUserId = Guid.NewGuid().ToString();

            Guid testRoleId = UserRoleResources.GetUserRoleId(UserRoles.OrganisationAdmin, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.RemoveRoleFromUserResponseAsync(
                _fixture.Client,
                invalidUserId,
                testRoleId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"User ID '{invalidUserId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task RemoveNonExistentRoleFromUserReturnsNotFound()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            var invalidRoleId = Guid.NewGuid();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.RemoveRoleFromUserResponseAsync(
                _fixture.Client,
                testUser.Id.ToString(),
                invalidRoleId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"User role ID '{invalidRoleId}' not found.", response.ResponseBody);
        }
    }
}
