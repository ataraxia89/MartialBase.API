// <copyright file="BlockUserAccessTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
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

namespace MartialBase.API.LiveControllerTests.AuthControllerTests
{
    [Collection("LiveControllerTests")]
    public class BlockUserAccessTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockUserAccessTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public BlockUserAccessTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Theory]
        [MemberData(nameof(UserRoleResources.NonSystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonSystemAdminCannotBlockUserAccess(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await AuthController.BlockUserAccessResponseAsync(
                _fixture.Client, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.SystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task SystemAdminCanBlockUserAccess(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            Assert.NotNull(testUser.AzureId);

            // Act
            HttpResponseModel response = await AuthController.BlockUserAccessResponseAsync(
                _fixture.Client, testUser.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Null(response.ResponseBody);

            MartialBaseUser retrievedUser = MartialBaseUserResources.GetUser(testUser.Id, _fixture.DbIdentifier);

            Assert.Null(retrievedUser.AzureId);
        }

        [Fact]
        public async Task BlockUserAccessReturnsBadRequestWhenNoUserProvided()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response =
                await AuthController.BlockUserAccessResponseAsync(_fixture.Client, null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal("No user ID parameter specified.", response.ResponseBody);
        }

        [Fact]
        public async Task BlockNonExistentUserReturnsNotFound()
        {
            // Arrange
            string wrongUserId = Guid.NewGuid().ToString();

            Assert.False(MartialBaseUserResources.CheckExists(wrongUserId, DbIdentifier));

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response =
                await AuthController.BlockUserAccessResponseAsync(_fixture.Client, wrongUserId);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"User ID '{wrongUserId}' not found.", response.ResponseBody);
        }
    }
}
