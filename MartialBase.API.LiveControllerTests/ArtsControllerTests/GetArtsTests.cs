// <copyright file="GetArtsTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Net;

using MartialBase.API.LiveControllerTests.TestTools;
using MartialBase.API.Models.DTOs.Arts;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.ArtsControllerTests
{
    [Collection("LiveControllerTests")]
    public class GetArtsTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetArtsTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetArtsTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        [Fact]
        public async Task GetArtsWithExpiredTokenReturnsUnauthorized()
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
        public async Task AllUsersCanGetArts(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(_fixture.TestUser.Id, roleName, _fixture.DbIdentifier);

            // Act
            List<ArtDTO> retrievedArts = await ArtsController.GetArtsAsync(_fixture.Client);

            // Assert
            ArtResources.AssertExist(retrievedArts, _fixture.DbIdentifier);
        }
    }
}
