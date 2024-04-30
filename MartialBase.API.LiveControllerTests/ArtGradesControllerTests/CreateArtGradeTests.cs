// <copyright file="CreateArtGradeTests.cs" company="Martialtech®">
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
using MartialBase.API.TestTools.TestResources;
using MartialBase.API.Tools;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.ArtGradesControllerTests
{
    [Collection("LiveControllerTests")]
    public class CreateArtGradeTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateArtGradeTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public CreateArtGradeTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        public static IEnumerable<object[]> InvalidCreateArtGradeDTOs
        {
            get
            {
                object[] noArtIdProvided =
                {
                    new CreateArtGradeDTO
                    {
                        OrganisationId = Guid.NewGuid().ToString(),
                        GradeLevel = RandomData.GetRandomNumber(),
                        Description = RandomData.GetRandomString(20)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "ArtId", new[] { "Art ID required." } }
                    }
                };

                object[] artIdTooLong =
                {
                    new CreateArtGradeDTO
                    {
                        ArtId = RandomData.GetRandomString(69),
                        OrganisationId = Guid.NewGuid().ToString(),
                        GradeLevel = RandomData.GetRandomNumber(),
                        Description = RandomData.GetRandomString(20)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "ArtId", new[] { "Art ID cannot be longer than 68 characters." } }
                    }
                };

                object[] noOrganisationIdProvided =
                {
                    new CreateArtGradeDTO
                    {
                        ArtId = Guid.NewGuid().ToString(),
                        GradeLevel = RandomData.GetRandomNumber(),
                        Description = RandomData.GetRandomString(20)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "OrganisationId", new[] { "Organisation ID required." } }
                    }
                };

                object[] organisationIdTooLong =
                {
                    new CreateArtGradeDTO
                    {
                        ArtId = Guid.NewGuid().ToString(),
                        OrganisationId = RandomData.GetRandomString(69),
                        GradeLevel = RandomData.GetRandomNumber(),
                        Description = RandomData.GetRandomString(20)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "OrganisationId", new[] { "Organisation ID cannot be longer than 68 characters." } }
                    }
                };

                object[] noDescriptionProvided =
                {
                    new CreateArtGradeDTO
                    {
                        ArtId = Guid.NewGuid().ToString(),
                        OrganisationId = Guid.NewGuid().ToString(),
                        GradeLevel = RandomData.GetRandomNumber()
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Description", new[] { "Description required." } }
                    }
                };

                object[] descriptionTooLong =
                {
                    new CreateArtGradeDTO
                    {
                        ArtId = Guid.NewGuid().ToString(),
                        OrganisationId = Guid.NewGuid().ToString(),
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
                    noArtIdProvided,
                    artIdTooLong,
                    noOrganisationIdProvided,
                    organisationIdTooLong,
                    noDescriptionProvided,
                    descriptionTooLong
                };
            }
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task CreateArtGradeWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            _fixture.GenerateAuthorizationToken(-10);

            // Act
            var response = await ArtGradesController.CreateArtGradeResponseAsync(
                _fixture.Client,
                DataGenerator.ArtGrades.GenerateCreateArtGradeDTOObject(testArtId, testOrganisationId));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(InvalidCreateArtGradeDTOs))]
        public async Task CreateArtGradeWithInvalidCreateDTOReturnsInternalServerError(
            CreateArtGradeDTO invalidCreateArtGradeDTO, Dictionary<string, string[]> expectedModelStateErrors)
        {
            // Act
            var response = await ArtGradesController.CreateArtGradeResponseAsync(
                _fixture.Client, invalidCreateArtGradeDTO);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Dictionary<string, string[]> modelStateErrors =
                ModelStateTools.ParseModelStateErrors(response.ResponseBody);

            DictionaryResources.AssertEqual(expectedModelStateErrors, modelStateErrors);
        }

        [Fact]
        public async Task CreateArtGradeWithNonExistentArtReturnsNotFound()
        {
            // Arrange
            var invalidArtId = Guid.NewGuid();
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            // Act
            var response = await ArtGradesController.CreateArtGradeResponseAsync(
                _fixture.Client,
                DataGenerator.ArtGrades.GenerateCreateArtGradeDTOObject(invalidArtId, testOrganisationId));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Art ID '{invalidArtId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task CreateArtGradeWithNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            var invalidOrganisationId = Guid.NewGuid();

            // Act
            var response = await ArtGradesController.CreateArtGradeResponseAsync(
                _fixture.Client,
                DataGenerator.ArtGrades.GenerateCreateArtGradeDTOObject(testArtId, invalidOrganisationId));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Organisation ID '{invalidOrganisationId}' not found.", response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.AllUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task BlockedUserCannotCreateArtGrade(string roleName)
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            var response = await ArtGradesController.CreateArtGradeResponseAsync(
                _fixture.Client,
                DataGenerator.ArtGrades.GenerateCreateArtGradeDTOObject(testArtId, testOrganisationId));

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonOrganisationAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationAdminUserCannotCreateArtGrade(string roleName)
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            var response = await ArtGradesController.CreateArtGradeResponseAsync(
                _fixture.Client,
                DataGenerator.ArtGrades.GenerateCreateArtGradeDTOObject(testArtId, testOrganisationId));

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Fact]
        public async Task OrganisationAdminUserCannotCreateArtGradeForOrganisationOfWhichTheyAreNotAdmin()
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, isAdmin: false);

            // Act
            var response = await ArtGradesController.CreateArtGradeResponseAsync(
                _fixture.Client,
                DataGenerator.ArtGrades.GenerateCreateArtGradeDTOObject(testArtId, testOrganisation.Id));

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        [Fact]
        public async Task SuperUserCanCreateArtGradeForOrganisationOfWhichTheyAreNotAMember()
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            CreateArtGradeDTO createArtGradeDTO =
                DataGenerator.ArtGrades.GenerateCreateArtGradeDTOObject(testArtId, testOrganisation.Id);

            // Act
            ArtGradeDTO createdArtGrade =
                await ArtGradesController.CreateArtGradeAsync(_fixture.Client, createArtGradeDTO);

            // Assert
            ArtGradeResources.AssertEqual(createArtGradeDTO, createdArtGrade);
        }

        [Fact]
        public async Task SuperUserCanCreateArtGradeForOrganisationOfWhichTheyAreNotAdmin()
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, isAdmin: false);

            CreateArtGradeDTO createArtGradeDTO =
                DataGenerator.ArtGrades.GenerateCreateArtGradeDTOObject(testArtId, testOrganisation.Id);

            // Act
            ArtGradeDTO createdArtGrade =
                await ArtGradesController.CreateArtGradeAsync(_fixture.Client, createArtGradeDTO);

            // Assert
            ArtGradeResources.AssertEqual(createArtGradeDTO, createdArtGrade);
        }

        [Fact]
        public async Task OrganisationAdminCanCreateArtGrade()
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, isAdmin: true);

            CreateArtGradeDTO createArtGradeDTO =
                DataGenerator.ArtGrades.GenerateCreateArtGradeDTOObject(testArtId, testOrganisation.Id);

            // Act
            ArtGradeDTO createdArtGrade =
                await ArtGradesController.CreateArtGradeAsync(_fixture.Client, createArtGradeDTO);

            // Assert
            ArtGradeResources.AssertEqual(createArtGradeDTO, createdArtGrade);
        }
    }
}
