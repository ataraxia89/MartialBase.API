// <copyright file="GetOrganisationsTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.OrganisationsControllerTests
{
    [Collection("LiveControllerTests")]
    public class GetOrganisationsTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetOrganisationsTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetOrganisationsTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task GetOrganisationsWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await OrganisationsController.GetOrganisationsResponseAsync(
                _fixture.Client, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.OrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task BlockedOrganisationUserCannotGetOrganisations(string roleName)
        {
            // Arrange
            OrganisationResources.CreateTestOrganisations(10, _fixture.DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response =
                await OrganisationsController.GetOrganisationsResponseAsync(_fixture.Client, null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        [Fact]
        public async Task SuperUserCanGetAllOrganisationsRegardlessOfMembership()
        {
            // Arrange
            Organisation testParentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            List<Organisation> testMemberOrganisations = OrganisationResources.CreateTestOrganisations(
                10, DbIdentifier, testParentOrganisation.Id);

            List<Organisation> testNonMemberOrganisations = OrganisationResources.CreateTestOrganisations(
                10, DbIdentifier, testParentOrganisation.Id);

            OrganisationPersonResources.EnsureOrganisationsHavePerson(
                testMemberOrganisations.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationsDoNotHavePerson(
                testNonMemberOrganisations.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            List<OrganisationDTO> retrievedOrganisations =
                await OrganisationsController.GetOrganisationsAsync(
                    _fixture.Client, testParentOrganisation.Id.ToString());

            // Assert
            Assert.Equal(
                testMemberOrganisations.Count + testNonMemberOrganisations.Count,
                retrievedOrganisations.Count);

            foreach (Organisation organisation in testMemberOrganisations)
            {
                OrganisationDTO retrievedOrganisation =
                    retrievedOrganisations.FirstOrDefault(o => o.Id == organisation.Id.ToString());

                OrganisationResources.AssertEqual(organisation, retrievedOrganisation);
            }

            foreach (Organisation organisation in testNonMemberOrganisations)
            {
                OrganisationDTO retrievedOrganisation =
                    retrievedOrganisations.FirstOrDefault(o => o.Id == organisation.Id.ToString());

                OrganisationResources.AssertEqual(organisation, retrievedOrganisation);
            }
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.OrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task OrganisationUserCanRetrieveOrganisationsTheyAreAMemberOf(string roleName)
        {
            // Arrange
            List<Organisation> testMemberOrganisations = OrganisationResources.CreateTestOrganisations(
                10, DbIdentifier);

            List<Organisation> testNonMemberOrganisations = OrganisationResources.CreateTestOrganisations(
                10, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationsHavePerson(
                testMemberOrganisations.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationsDoNotHavePerson(
                testNonMemberOrganisations.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            List<OrganisationDTO> retrievedOrganisations =
                await OrganisationsController.GetOrganisationsAsync(_fixture.Client, null);

            // Assert
            OrganisationResources.AssertEqual(testMemberOrganisations, retrievedOrganisations);

            foreach (Organisation organisation in testNonMemberOrganisations)
            {
                Assert.Null(retrievedOrganisations.FirstOrDefault(o =>
                    o.Id == organisation.Id.ToString()));
            }
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonOrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationUserCannotGetOrganisations(string roleName)
        {
            // Arrange
            OrganisationResources.CreateTestOrganisations(10, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response =
                await OrganisationsController.GetOrganisationsResponseAsync(_fixture.Client, null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Fact]
        public async Task CanGetChildOrganisationsForASpecifiedParent()
        {
            // Arrange
            Organisation parentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            List<Organisation> testOrganisationsWithParent = OrganisationResources.CreateTestOrganisations(
                10, DbIdentifier, parentOrganisation.Id);

            List<Organisation> testOrganisationsWithoutParent = OrganisationResources.CreateTestOrganisations(
                10, DbIdentifier);

            foreach (Organisation organisation in testOrganisationsWithoutParent)
            {
                Assert.Null(organisation.ParentId);
                Assert.Null(organisation.Parent);
            }

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            List<OrganisationDTO> retrievedOrganisations =
                await OrganisationsController.GetOrganisationsAsync(
                    _fixture.Client, parentOrganisation.Id.ToString());

            // Assert
            OrganisationResources.AssertEqual(testOrganisationsWithParent, retrievedOrganisations);

            foreach (Organisation organisation in testOrganisationsWithoutParent)
            {
                Assert.Null(retrievedOrganisations.FirstOrDefault(o =>
                    o.Id == organisation.Id.ToString()));
            }
        }

        [Fact]
        public async Task GetOrganisationsForNonExistentParentReturnsNotFound()
        {
            // Arrange
            string invalidOrganisationId = Guid.NewGuid().ToString();

            Organisation parentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationResources.CreateTestOrganisations(5, DbIdentifier, parentOrganisation.Id);

            OrganisationResources.CreateTestOrganisations(5, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.GetOrganisationsResponseAsync(
                _fixture.Client, invalidOrganisationId);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Organisation ID '{invalidOrganisationId}' not found.", response.ResponseBody);
        }
    }
}
