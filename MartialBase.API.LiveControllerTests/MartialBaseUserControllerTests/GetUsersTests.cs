// <copyright file="GetUsersTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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
    public class GetUsersTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetUsersTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetUsersTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task GetUsersWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await MartialBaseUserController.GetUsersResponseAsync(_fixture.Client);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonSystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonSystemAdminCannotGetUsers(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await MartialBaseUserController.GetUsersResponseAsync(_fixture.Client);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.SystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task SystemAdminUserCanGetUsers(string roleName)
        {
            // Arrange
            List<MartialBaseUser> dbUsers = MartialBaseUserResources.CreateMartialBaseUsers(10, DbIdentifier);

            dbUsers.AddRange(MartialBaseUserResources.CreateMartialBaseUsers(10, DbIdentifier));

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            List<MartialBaseUserDTO> retrievedUsers = await MartialBaseUserController.GetUsersAsync(_fixture.Client);

            // Assert
            foreach (MartialBaseUser user in dbUsers)
            {
                MartialBaseUserDTO retrievedUser = retrievedUsers.FirstOrDefault(u => u.Id == user.Id.ToString());

                Assert.NotNull(retrievedUser);
                MartialBaseUserResources.AssertEqual(user, retrievedUser);
            }
        }
    }
}
