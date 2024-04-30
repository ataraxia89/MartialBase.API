// <copyright file="GetCountriesTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Net;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.LiveControllerTests.TestTools;
using MartialBase.API.Models.DTOs.Countries;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.CountriesControllerTests
{
    [Collection("LiveControllerTests")]
    public class GetCountriesTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetCountriesTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetCountriesTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        [Fact]
        public async Task GetCountriesWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await CountriesController.GetCountriesResponseAsync(_fixture.Client);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.AllUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task AllUsersCanGetCountries(string roleName)
        {
            // Arrange
            int populationLimit = 1000000;

            MartialBaseUserRoleResources.EnsureUserHasRole(_fixture.TestUser.Id, roleName, _fixture.DbIdentifier);

            List<Country> testCountries = Countries.GetAllCountries(populationLimit);

            // Act
            List<CountryDTO> retrievedCountries = await CountriesController.GetCountriesAsync(_fixture.Client);

            // Assert
            CountryResources.AssertEqual(testCountries, retrievedCountries);
        }
    }
}
