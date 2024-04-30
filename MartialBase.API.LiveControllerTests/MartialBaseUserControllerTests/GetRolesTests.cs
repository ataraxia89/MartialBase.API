// <copyright file="GetRolesTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.UserRoles;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.MartialBaseUserControllerTests
{
    [Collection("LiveControllerTests")]
    public class GetRolesTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetRolesTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetRolesTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task GetRolesWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await MartialBaseUserController.GetRolesResponseAsync(_fixture.Client);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonSystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonSystemAdminCannotGetRoles(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.GetRolesResponseAsync(_fixture.Client);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.SystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task SystemAdminCanGetRoles(string roleName)
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            var testRoles = UserRoleResources
                .GetUserRoles(DbIdentifier)
                .Where(ur => ur.Name != UserRoles.Thanos).ToList();

            foreach (string userRoleName in testRoles.Select(ur => ur.Name))
            {
                MartialBaseUserRoleResources.EnsureUserHasRole(testUser.Id, userRoleName, DbIdentifier, false);
            }

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            List<UserRoleDTO> retrievedRoles = await MartialBaseUserController.GetRolesAsync(_fixture.Client);

            // Assert
            foreach (UserRole role in testRoles)
            {
                UserRoleDTO retrievedRole = retrievedRoles.FirstOrDefault(r => r.Id == role.Id.ToString());

                UserRoleResources.AssertEqual(role, retrievedRole);
            }
        }
    }
}
