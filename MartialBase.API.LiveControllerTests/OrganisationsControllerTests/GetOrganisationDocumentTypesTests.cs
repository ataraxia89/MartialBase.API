// <copyright file="GetOrganisationDocumentTypesTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.DocumentTypes;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.OrganisationsControllerTests
{
    [Collection("LiveControllerTests")]
    public class GetOrganisationDocumentTypesTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetOrganisationDocumentTypesTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetOrganisationDocumentTypesTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task GetOrganisationDocumentTypesWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await OrganisationsController.GetOrganisationDocumentTypesResponseAsync(
                _fixture.Client, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonOrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationUserCannotGetOrganisationDocumentTypes(string roleName)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<DocumentType> testDocumentTypes =
                DocumentTypeResources.CreateTestDocumentTypes(10, DbIdentifier);

            DocumentTypeResources.EnsureOrganisationHasDocumentTypes(
                testOrganisation, testDocumentTypes, DbIdentifier);

            // This test is specifically for the OrganisationAdmin role, so the user can be admin of an
            // organisation, but they still must have this role assigned to them to carry out functions
            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.GetOrganisationDocumentTypesResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Fact]
        public async Task BlockedOrganisationAdminUserCannotGetOrganisationDocumentTypes()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<DocumentType> testDocumentTypes =
                DocumentTypeResources.CreateTestDocumentTypes(10, DbIdentifier);

            DocumentTypeResources.EnsureOrganisationHasDocumentTypes(
                testOrganisation, testDocumentTypes, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await OrganisationsController.GetOrganisationDocumentTypesResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        [Fact]
        public async Task GetDocumentTypesFromNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            var invalidOrganisationId = Guid.NewGuid();

            OrganisationResources.AssertDoesNotExist(invalidOrganisationId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.GetOrganisationDocumentTypesResponseAsync(
                _fixture.Client, invalidOrganisationId.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Organisation ID '{invalidOrganisationId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task GetOrganisationDocumentTypesWhenRequestingUserIsNotAnOrganisationMemberReturnsForbidden()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<DocumentType> testDocumentTypes =
                DocumentTypeResources.CreateTestDocumentTypes(10, DbIdentifier);

            DocumentTypeResources.EnsureOrganisationHasDocumentTypes(
                testOrganisation, testDocumentTypes, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationMember, DbIdentifier);

            // Act
            HttpResponseModel response = await OrganisationsController.GetOrganisationDocumentTypesResponseAsync(
                _fixture.Client, testOrganisation.Id.ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NoOrganisationAccess, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.OrganisationUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task OrganisationMemberCanGetOrganisationDocumentTypes(string roleName)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation otherOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            List<DocumentType> testDocumentTypes =
                DocumentTypeResources.CreateTestDocumentTypes(10, DbIdentifier);
            List<DocumentType> otherDocumentTypes =
                DocumentTypeResources.CreateTestDocumentTypes(10, DbIdentifier);

            DocumentTypeResources.EnsureOrganisationHasDocumentTypes(
                testOrganisation, testDocumentTypes, DbIdentifier);
            DocumentTypeResources.EnsureOrganisationHasDocumentTypes(
                otherOrganisation, otherDocumentTypes, DbIdentifier);

            // Don't need to ensure "other" document types don't belong to the "test" organisation as they can
            // only belong to one at a time (by design)

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            List<DocumentTypeDTO> retrievedDocumentTypes =
                await OrganisationsController.GetOrganisationDocumentTypesAsync(
                    _fixture.Client,
                    testOrganisation.Id.ToString());

            // Assert
            DocumentTypeResources.AssertEqual(testDocumentTypes, retrievedDocumentTypes);

            foreach (DocumentType otherDocumentType in otherDocumentTypes)
            {
                Assert.Null(retrievedDocumentTypes.FirstOrDefault(o =>
                    o.Id == otherDocumentType.Id.ToString()));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SuperUserCanGetOrganisationDocumentTypesRegardlessOfOrganisationMembership(bool isMember)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation otherOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            List<DocumentType> testDocumentTypes =
                DocumentTypeResources.CreateTestDocumentTypes(10, DbIdentifier);
            List<DocumentType> otherDocumentTypes =
                DocumentTypeResources.CreateTestDocumentTypes(10, DbIdentifier);

            DocumentTypeResources.EnsureOrganisationHasDocumentTypes(
                testOrganisation, testDocumentTypes, DbIdentifier);
            DocumentTypeResources.EnsureOrganisationHasDocumentTypes(
                otherOrganisation, otherDocumentTypes, DbIdentifier);

            // Don't need to ensure "other" document types don't belong to the "test" organisation as they can
            // only belong to one at a time (by design)

            if (isMember)
            {
                OrganisationPersonResources.EnsureOrganisationHasPerson(
                    testOrganisation.Id, TestUserPersonId, DbIdentifier);
            }
            else
            {
                OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                    testOrganisation.Id, TestUserPersonId, DbIdentifier);
            }

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            List<DocumentTypeDTO> retrievedDocumentTypes =
                await OrganisationsController.GetOrganisationDocumentTypesAsync(
                    _fixture.Client,
                    testOrganisation.Id.ToString());

            // Assert
            DocumentTypeResources.AssertEqual(testDocumentTypes, retrievedDocumentTypes);

            foreach (DocumentType otherDocumentType in otherDocumentTypes)
            {
                Assert.Null(retrievedDocumentTypes.FirstOrDefault(o =>
                    o.Id == otherDocumentType.Id.ToString()));
            }
        }
    }
}
