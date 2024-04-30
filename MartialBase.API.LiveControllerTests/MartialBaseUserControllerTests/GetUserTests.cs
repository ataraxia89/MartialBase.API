// <copyright file="GetUserTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.MartialBaseUsers;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.MartialBaseUserControllerTests
{
    [Collection("LiveControllerTests")]
    public class GetUserTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetUserTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetUserTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task GetUserWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await MartialBaseUserController.GetUserResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                "false");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonSystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonSystemAdminCannotGetUser(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.GetUserResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                "false");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.SystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task SystemAdminUserCanGetUserWithPerson(string roleName)
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            MartialBaseUserDTO retrievedUser =
                await MartialBaseUserController.GetUserAsync(_fixture.Client, testUser.Id.ToString(), "true");

            // Assert
            MartialBaseUserResources.AssertEqual(testUser, retrievedUser);
        }

        [Fact]
        public async Task GetNonExistentUserReturnsNotFound()
        {
            // Arrange
            string invalidUserId = Guid.NewGuid().ToString();

            Assert.False(MartialBaseUserResources.CheckExists(invalidUserId, _fixture.DbIdentifier));

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.GetUserResponseAsync(
                _fixture.Client,
                invalidUserId,
                "false");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"User ID '{invalidUserId}' not found.", response.ResponseBody);
        }
    }
}
