// <copyright file="GetPersonIdForCurrentMartialBaseUserTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Net;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.LiveControllerTests.TestTools;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.PeopleControllerTests
{
    [Collection("LiveControllerTests")]
    public class GetPersonIdForCurrentMartialBaseUserTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPersonIdForCurrentMartialBaseUserTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetPersonIdForCurrentMartialBaseUserTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task GetPersonIdForMartialBaseUserWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response =
                await PeopleController.GetPersonIdForCurrentMartialBaseUserResponseAsync(_fixture.Client);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.AllUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task AnyUserCanGetPersonIdForThemselves(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            Guid? returnedId =
                await PeopleController.GetPersonIdForCurrentMartialBaseUserAsync(_fixture.Client);

            // Assert
            Assert.NotNull(returnedId);
            Assert.Equal(TestUserPersonId, returnedId);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.AllUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task AnyUserGetsRegisteredWithAnInvitationCode(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            _fixture.RemoveAzureIdFromDbTestUser();

            MartialBaseUser testUser = MartialBaseUserResources.GetUser(TestUserId, DbIdentifier);

            Assert.Null(testUser.AzureId);
            Assert.Equal(_fixture.InvitationCode, testUser.InvitationCode);

            _fixture.GenerateAuthorizationToken();

            // Act
            Guid? returnedId =
                await PeopleController.GetPersonIdForCurrentMartialBaseUserAsync(_fixture.Client);

            // Assert
            Assert.NotNull(returnedId);
            Assert.Equal(TestUserPersonId, returnedId);

            testUser = MartialBaseUserResources.GetUser(TestUserId, DbIdentifier);

            Assert.Equal(_fixture.TestUser.AzureId, testUser.AzureId);
            Assert.Null(testUser.InvitationCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.AllUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task GetPersonIdForBlockedMartialBaseUserReturnsNull(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            _fixture.BlockTestUserAccess();

            MartialBaseUser testUser = MartialBaseUserResources.GetUser(TestUserId, DbIdentifier);

            Assert.Null(testUser.AzureId);
            Assert.Null(testUser.InvitationCode);

            _fixture.GenerateAuthorizationToken();

            // Act
            Guid? returnedId =
                await PeopleController.GetPersonIdForCurrentMartialBaseUserAsync(_fixture.Client);

            // Assert
            Assert.Null(returnedId);
        }
    }
}
