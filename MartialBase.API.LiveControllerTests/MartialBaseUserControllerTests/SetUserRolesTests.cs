// <copyright file="SetUserRolesTests.cs" company="Martialtech®">
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

namespace MartialBase.API.LiveControllerTests.MartialBaseUserControllerTests
{
    [Collection("LiveControllerTests")]
    public class SetUserRolesTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetUserRolesTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public SetUserRolesTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task SetUserRolesWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await MartialBaseUserController.SetUserRolesResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                new List<Guid>());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonSystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonSystemAdminCannotSetUserRoles(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.SetUserRolesResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                new List<Guid>());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.SystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task SystemAdminCanSetUserRoles(string roleName)
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            var testUserRoles = UserRoleResources.GetUserRoles(DbIdentifier).ToList();

            foreach (UserRole testUserRole in testUserRoles)
            {
                MartialBaseUserRoleResources.EnsureUserDoesNotHaveRole(
                    testUser.Id, testUserRole.Id, DbIdentifier);
            }

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.SetUserRolesResponseAsync(
                _fixture.Client,
                testUser.Id.ToString(),
                testUserRoles.Select(ur => ur.Id));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Null(response.ResponseBody);

            foreach (UserRole testUserRole in testUserRoles)
            {
                MartialBaseUserRoleResources.AssertUserHasRole(testUser.Id, testUserRole.Id, DbIdentifier);
            }
        }

        [Fact]
        public async Task SetUserRolesReturnsBadRequestWhenNoRolesProvided()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.SetUserRolesResponseAsync(
                _fixture.Client,
                testUser.Id.ToString(),
                null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal("No role IDs specified.", response.ResponseBody);
        }

        [Fact]
        public async Task SetUserRolesForNonExistentUserReturnsNotFound()
        {
            // Arrange
            var invalidUserId = Guid.NewGuid();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.SetUserRolesResponseAsync(
                _fixture.Client,
                invalidUserId.ToString(),
                new List<Guid>());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"User ID '{invalidUserId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task SetNonExistentUserRolesReturnsNotFound()
        {
            // Arrange
            var invalidRoleId = Guid.NewGuid();

            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.SetUserRolesResponseAsync(
                _fixture.Client,
                testUser.Id.ToString(),
                new List<Guid> { invalidRoleId });

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"User role ID '{invalidRoleId}' not found.", response.ResponseBody);
        }
    }
}
