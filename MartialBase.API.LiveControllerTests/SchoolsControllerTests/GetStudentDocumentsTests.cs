// <copyright file="GetStudentDocumentsTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using MartialBase.API.Controllers;
using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.LiveControllerTests.TestTools;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.SchoolsControllerTests
{
    /// <summary>
    /// Live controller tests with a live database for the
    /// <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> method in the
    /// <see cref="Controllers.SchoolsController"/>.
    /// </summary>
    [Collection("LiveControllerTests")]
    public class GetStudentDocumentsTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        private School _testSchool;
        private Person _testStudent;
        private List<Document> _testDocuments;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetStudentDocumentsTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The injected <see cref="TestServerFixture"/> instance currently in use.</param>
        public GetStudentDocumentsTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();

            PrepareTestResources();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> endpoint
        /// returns an <see cref="HttpStatusCode.Unauthorized">Unauthorized</see> response containing an
        /// <see cref="ErrorResponseCode.ExpiredToken">ExpiredToken</see> error code when called with an expired
        /// bearer token.
        /// </summary>
        [Fact]
        public async Task GetStudentDocumentsWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.SchoolsController.GetStudentDocumentsResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                "true");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> endpoint
        /// returns an <see cref="HttpStatusCode.Unauthorized">Unauthorized</see> response containing an
        /// <see cref="ErrorResponseCode.InsufficientUserRole">InsufficientUserRole</see> error code when called
        /// by an <see cref="MartialBaseUser"/> who is not assigned to the
        /// <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see> role.
        /// </summary>
        /// <param name="roleName">The name of the role to be assigned to the requesting user during testing.</param>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonSchoolSecretaryRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonSchoolSecretaryUserCannotGetStudentDocuments(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.SchoolsController.GetStudentDocumentsResponseAsync(
                _fixture.Client,
                _testSchool.Id.ToString(),
                _testStudent.Id.ToString(),
                "true");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> endpoint
        /// returns a <see cref="HttpStatusCode.BadRequest">BadRequest</see> response when called with an invalid
        /// value for the <c>includeInactive</c> parameter.
        /// </summary>
        [Fact]
        public async Task GetStudentDocumentWithInvalidIncludeInactiveParameterReturnsBadRequest()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.SchoolsController.GetStudentDocumentsResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                "NotABool");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal("Invalid value provided for includeInactive parameter.", response.ResponseBody);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> endpoint
        /// returns a <see cref="HttpStatusCode.NotFound">NotFound</see> response when provided with an invalid
        /// <see cref="School"/> ID.
        /// </summary>
        [Fact]
        public async Task GetStudentDocumentsFromANonExistentSchoolReturnsNotFound()
        {
            // Arrange
            var invalidSchoolId = Guid.NewGuid();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.SchoolsController.GetStudentDocumentsResponseAsync(
                _fixture.Client,
                invalidSchoolId.ToString(),
                _testStudent.Id.ToString(),
                "true");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"School ID '{invalidSchoolId}' not found.", response.ResponseBody);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> endpoint
        /// returns an <see cref="HttpStatusCode.Unauthorized">Unauthorized</see> response containing a
        /// <see cref="ErrorResponseCode.AzureUserNotRegistered">AzureUserNotRegistered</see> error code when
        /// called by an <see cref="MartialBaseUser"/> who has no associated <see cref="Person"/> record.
        /// </summary>
        [Fact]
        public async Task BlockedSchoolSecretaryUserCannotGetStudentDocuments()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.SchoolsController.GetStudentDocumentsResponseAsync(
                _fixture.Client,
                _testSchool.Id.ToString(),
                _testStudent.Id.ToString(),
                "true");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> endpoint
        /// successfully returns the requested <see cref="List{T}"/> of <see cref="DocumentDTO"/> objects to a
        /// super user when the user is not registered/assigned to the requested <see cref="Person"/>'s
        /// <see cref="School"/>.
        /// </summary>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Fact]
        public async Task SuperUserCanGetStudentDocumentsFromASchoolOfWhichTheyAreNotAMember()
        {
            // Arrange
            SchoolStudentResources.EnsureSchoolDoesNotHaveStudent(_testSchool.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            List<DocumentDTO> retrievedDocuments =
                await API.TestTools.ControllerMethods.SchoolsController.GetStudentDocumentsAsync(
                    _fixture.Client, _testSchool.Id.ToString(), _testStudent.Id.ToString(), "true");

            // Assert
            DocumentResources.AssertEqual(_testDocuments, retrievedDocuments);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> endpoint
        /// successfully returns the requested <see cref="List{T}"/> of <see cref="DocumentDTO"/> objects to a
        /// super user when the user is registered/assigned to the requested <see cref="Person"/>'s
        /// <see cref="School"/>, but is not a secretary of it.
        /// </summary>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Fact]
        public async Task SuperUserCanGetStudentDocumentsFromASchoolOfWhichTheyAreNotASecretary()
        {
            // Arrange
            SchoolStudentResources.EnsureSchoolHasStudent(
                _testSchool.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            List<DocumentDTO> retrievedDocuments =
                await API.TestTools.ControllerMethods.SchoolsController.GetStudentDocumentsAsync(
                    _fixture.Client, _testSchool.Id.ToString(), _testStudent.Id.ToString(), "true");

            // Assert
            DocumentResources.AssertEqual(_testDocuments, retrievedDocuments);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> endpoint
        /// returns an <see cref="HttpStatusCode.Unauthorized">Unauthorized</see> response containing a
        /// <see cref="ErrorResponseCode.NoAccessToPerson">NoAccessToPerson</see> error code when the requesting
        /// user is not a member of the subject student's <see cref="School"/>.
        /// </summary>
        [Fact]
        public async Task SchoolSecretaryCannotGetStudentDocumentsFromASchoolOfWhichTheyAreNotAMember()
        {
            // Arrange
            SchoolStudentResources.EnsureSchoolDoesNotHaveStudent(_testSchool.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.SchoolsController.GetStudentDocumentsResponseAsync(
                _fixture.Client,
                _testSchool.Id.ToString(),
                _testStudent.Id.ToString(),
                "true");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotSchoolSecretary, response.ErrorResponseCode);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> endpoint
        /// returns a <see cref="HttpStatusCode.NotFound">NotFound</see> response when provided with an invalid
        /// <see cref="Person"/> ID.
        /// </summary>
        [Fact]
        public async Task GetStudentDocumentsForANonExistentStudentReturnsNotFound()
        {
            // Arrange
            var invalidStudentId = Guid.NewGuid();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.SchoolsController.GetStudentDocumentsResponseAsync(
                _fixture.Client,
                _testSchool.Id.ToString(),
                invalidStudentId.ToString(),
                "true");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Person ID '{invalidStudentId}' not found.", response.ResponseBody);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> endpoint
        /// successfully returns an empty <see cref="List{T}"/> of <see cref="DocumentDTO"/> objects to a school
        /// secretary.
        /// </summary>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Fact]
        public async Task GetStudentDocumentsReturnsEmptyListForAStudentWithNoDocuments()
        {
            // Arrange
            SchoolStudentResources.EnsureSchoolHasStudent(
                _testSchool.Id, TestUserPersonId, DbIdentifier, false, true);

            DocumentResources.EnsureDoesNotExist(_testDocuments, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            List<DocumentDTO> retrievedDocuments =
                await API.TestTools.ControllerMethods.SchoolsController.GetStudentDocumentsAsync(
                    _fixture.Client, _testSchool.Id.ToString(), _testStudent.Id.ToString(), "true");

            // Assert
            Assert.NotNull(retrievedDocuments);
            Assert.Empty(retrievedDocuments);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> endpoint
        /// successfully returns the requested <see cref="List{T}"/> of <see cref="DocumentDTO"/> objects to a
        /// school secretary.
        /// </summary>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Fact]
        public async Task SchoolSecretaryCanGetStudentDocuments()
        {
            // Arrange
            SchoolStudentResources.EnsureSchoolHasStudent(
                _testSchool.Id, TestUserPersonId, DbIdentifier, false, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            // Act
            List<DocumentDTO> retrievedDocuments =
                await API.TestTools.ControllerMethods.SchoolsController.GetStudentDocumentsAsync(
                    _fixture.Client, _testSchool.Id.ToString(), _testStudent.Id.ToString(), "true");

            // Assert
            DocumentResources.AssertEqual(_testDocuments, retrievedDocuments);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> endpoint
        /// only returns active <see cref="DocumentDTO"/>s when specified by the requesting user.
        /// </summary>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Fact]
        public async Task GetStudentDocumentsOnlyReturnsActiveDocumentsWhenSpecified()
        {
            // Arrange
            List<Document> activeDocuments = _testDocuments;
            List<Document> inactiveDocuments = DocumentResources.CreateTestDocuments(10, _fixture.DbIdentifier);

            var activeDocumentIds = activeDocuments.Select(d => d.Id).ToList();
            var inactiveDocumentIds = inactiveDocuments.Select(d => d.Id).ToList();

            PersonDocumentResources.EnsurePersonHasDocuments(
                _testStudent.Id, inactiveDocumentIds, _fixture.DbIdentifier);

            PersonDocumentResources.EnsurePersonDocumentsActiveStatus(
                _testStudent.Id, activeDocumentIds, true, _fixture.DbIdentifier);
            PersonDocumentResources.EnsurePersonDocumentsActiveStatus(
                _testStudent.Id, inactiveDocumentIds, false, _fixture.DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            List<DocumentDTO> retrievedDocuments =
                await API.TestTools.ControllerMethods.SchoolsController.GetStudentDocumentsAsync(
                    _fixture.Client, _testSchool.Id.ToString(), _testStudent.Id.ToString(), "false");

            // Assert
            DocumentResources.AssertEqual(activeDocuments, retrievedDocuments);
        }

        private void PrepareTestResources()
        {
            _testSchool = SchoolResources.CreateTestSchool(_fixture.DbIdentifier);
            _testStudent = PersonResources.CreateTestPerson(_fixture.DbIdentifier);
            _testDocuments = DocumentResources.CreateTestDocuments(10, _fixture.DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                _testSchool.Id, _testStudent.Id, _fixture.DbIdentifier);

            PersonDocumentResources.EnsurePersonHasDocuments(
                _testStudent.Id, _testDocuments.Select(d => d.Id).ToList(), _fixture.DbIdentifier);
        }
    }
}
