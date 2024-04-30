// <copyright file="UpdateArtGradeTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.ArtGrades;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;
using MartialBase.API.Tools;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.ArtGradesControllerTests
{
    [Collection("LiveControllerTests")]
    public class UpdateArtGradeTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateArtGradeTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public UpdateArtGradeTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        public static IEnumerable<object[]> InvalidUpdateArtGradeDTOs
        {
            get
            {
                object[] descriptionTooLong =
                {
                    new UpdateArtGradeDTO
                    {
                        GradeLevel = RandomData.GetRandomNumber(),
                        Description = RandomData.GetRandomString(21)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Description", new[] { "Description cannot be longer than 20 characters." } }
                    }
                };

                return new List<object[]>
                {
                    descriptionTooLong
                };
            }
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task UpdateArtGradeWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await ArtGradesController.UpdateArtGradeResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                DataGenerator.ArtGrades.GenerateUpdateArtGradeDTOObject());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(InvalidUpdateArtGradeDTOs))]
        public async Task UpdateArtGradeWithInvalidDTOReturnsInternalServerError(
            UpdateArtGradeDTO invalidUpdateArtGradeDTO, Dictionary<string, string[]> expectedModelStateErrors)
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            ArtGradeResources.AssertNotEqual(testArtGrade, invalidUpdateArtGradeDTO);

            // Act
            HttpResponseModel response = await ArtGradesController.UpdateArtGradeResponseAsync(
                _fixture.Client,
                testArtGrade.Id.ToString(),
                invalidUpdateArtGradeDTO);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Dictionary<string, string[]> modelStateErrors = ModelStateTools.ParseModelStateErrors(response.ResponseBody);

            DictionaryResources.AssertEqual(expectedModelStateErrors, modelStateErrors);
        }

        [Fact]
        public async Task UpdateNonExistentArtGradeReturnsNotFound()
        {
            // Arrange
            string invalidArtGradeId = Guid.NewGuid().ToString();

            // Act
            HttpResponseModel response = await ArtGradesController.UpdateArtGradeResponseAsync(
                _fixture.Client,
                invalidArtGradeId,
                DataGenerator.ArtGrades.GenerateUpdateArtGradeDTOObject());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Art grade ID '{invalidArtGradeId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task BlockedOrganisationAdminUserCannotUpdateArtGrade()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await ArtGradesController.UpdateArtGradeResponseAsync(
                _fixture.Client,
                ArtGradeResources.CreateTestArtGrade(DbIdentifier).Id.ToString(),
                DataGenerator.ArtGrades.GenerateUpdateArtGradeDTOObject());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        [Fact]
        public async Task OrganisationAdminCannotUpdateArtGradeForOrganisationOfWhichTheyAreNotAMember()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testArtGrade.OrganisationId, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await ArtGradesController.UpdateArtGradeResponseAsync(
                _fixture.Client,
                testArtGrade.Id.ToString(),
                DataGenerator.ArtGrades.GenerateUpdateArtGradeDTOObject());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        [Fact]
        public async Task OrganisationAdminCannotUpdateArtGradeForOrganisationOfWhichTheyAreNotAdmin()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testArtGrade.OrganisationId, TestUserPersonId, DbIdentifier, isAdmin: false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await ArtGradesController.UpdateArtGradeResponseAsync(
                _fixture.Client,
                testArtGrade.Id.ToString(),
                DataGenerator.ArtGrades.GenerateUpdateArtGradeDTOObject());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        [Fact]
        public async Task SuperUserCanUpdateArtGradeForOrganisationOfWhichTheyAreNotAMember()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            UpdateArtGradeDTO updateArtGradeDTO = DataGenerator.ArtGrades.GenerateUpdateArtGradeDTOObject();

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testArtGrade.OrganisationId, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            ArtGradeDTO updatedArtGrade = await ArtGradesController.UpdateArtGradeAsync(
                _fixture.Client, testArtGrade.Id.ToString(), updateArtGradeDTO);

            // Assert
            ArtGradeResources.AssertEqual(updateArtGradeDTO, updatedArtGrade);
            ArtGradeResources.AssertExists(updatedArtGrade, DbIdentifier);
        }

        [Fact]
        public async Task SuperUserCanUpdateArtGradeForOrganisationOfWhichTheyAreNotAdmin()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            UpdateArtGradeDTO updateArtGradeDTO = DataGenerator.ArtGrades.GenerateUpdateArtGradeDTOObject();

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testArtGrade.OrganisationId, TestUserPersonId, DbIdentifier, isAdmin: false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            ArtGradeDTO updatedArtGrade = await ArtGradesController.UpdateArtGradeAsync(
                _fixture.Client, testArtGrade.Id.ToString(), updateArtGradeDTO);

            // Assert
            ArtGradeResources.AssertEqual(updateArtGradeDTO, updatedArtGrade);
            ArtGradeResources.AssertExists(updatedArtGrade, DbIdentifier);
        }

        [Fact]
        public async Task OrganisationAdminCanUpdateArtGrade()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            UpdateArtGradeDTO updateArtGradeDTO = DataGenerator.ArtGrades.GenerateUpdateArtGradeDTOObject();

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testArtGrade.OrganisationId, TestUserPersonId, DbIdentifier, isAdmin: true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            ArtGradeDTO updatedArtGrade = await ArtGradesController.UpdateArtGradeAsync(
                _fixture.Client, testArtGrade.Id.ToString(), updateArtGradeDTO);

            // Assert
            ArtGradeResources.AssertEqual(updateArtGradeDTO, updatedArtGrade);
            ArtGradeResources.AssertExists(updatedArtGrade, DbIdentifier);
        }
    }
}
