// <copyright file="GetArtTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.Arts;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.ArtsControllerTests
{
    [Collection("LiveControllerTests")]
    public class GetArtTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetArtTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetArtTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task GetArtWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await ArtsController.GetArtsResponseAsync(_fixture.Client);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.AllUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task AllUsersCanGetArt(string roleName)
        {
            // Arrange
            List<Art> dbArts = ArtResources.GetAllArts(_fixture.DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            foreach (Art dbArt in dbArts)
            {
                // Act
                ArtDTO retrievedArt = await ArtsController.GetArtAsync(_fixture.Client, dbArt.Id.ToString());

                // Assert
                ArtResources.AssertEqual(dbArt, retrievedArt);
            }
        }

        [Fact]
        public async Task GetNonExistentArtReturnsNotFound()
        {
            // Arrange
            var wrongArtId = Guid.NewGuid();

            ArtResources.AssertDoesNotExist(wrongArtId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.User, DbIdentifier);

            // Act
            HttpResponseModel response = await ArtsController.GetArtResponseAsync(
                _fixture.Client, wrongArtId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Art ID '{wrongArtId}' not found.", response.ResponseBody);
        }
    }
}
