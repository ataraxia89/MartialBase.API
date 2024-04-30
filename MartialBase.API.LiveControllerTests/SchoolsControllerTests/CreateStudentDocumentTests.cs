// <copyright file="CreateStudentDocumentTests.cs" company="Martialtech®">
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
using MartialBase.API.TestTools.TestResources;
using MartialBase.API.Tools;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.SchoolsControllerTests
{
    /// <summary>
    /// Live controller tests with a live database for the
    /// <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see> method in the
    /// <see cref="Controllers.SchoolsController"/>.
    /// </summary>
    [Collection("LiveControllerTests")]
    public class CreateStudentDocumentTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        private School _testSchool;
        private Person _testStudent;
        private CreateDocumentDTO _testCreateDocumentDTO;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateStudentDocumentTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The injected <see cref="TestServerFixture"/> instance currently in use.</param>
        public CreateStudentDocumentTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();

            PrepareTestResources();
        }

        /// <summary>
        /// A collection of invalid <see cref="CreateDocumentDTO"/>s for use in model state testing.
        /// </summary>
        public static IEnumerable<object[]> InvalidCreateDocumentDTOs
        {
            get
            {
                object[] noDocumentTypeProvided =
                {
                    new CreateDocumentDTO(),
                    new Dictionary<string, string[]>
                    {
                        { "DocumentTypeId", new[] { "Document type ID required." } }
                    }
                };

                object[] documentTypeIdTooLong =
                {
                    new CreateDocumentDTO
                    {
                        DocumentTypeId = new string('*', 69)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "DocumentTypeId", new[] { "Document type ID cannot be longer than 68 characters." } }
                    }
                };

                object[] referenceTooLong =
                {
                    new CreateDocumentDTO
                    {
                        DocumentTypeId = Guid.NewGuid().ToString(),
                        Reference = new string('*', 21)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Reference", new[] { "Reference cannot be longer than 20 characters." } }
                    }
                };

                object[] urlTooLong =
                {
                    new CreateDocumentDTO
                    {
                        DocumentTypeId = Guid.NewGuid().ToString(),
                        URL = new string('*', 251)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "URL", new[] { "URL cannot be longer than 250 characters." } }
                    }
                };

                return new List<object[]>
                {
                    noDocumentTypeProvided,
                    documentTypeIdTooLong,
                    referenceTooLong,
                    urlTooLong
                };
            }
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see>
        /// endpoint returns an <see cref="HttpStatusCode.Unauthorized">Unauthorized</see> response containing an
        /// <see cref="ErrorResponseCode.ExpiredToken">ExpiredToken</see> error code when called with an expired
        /// bearer token.
        /// </summary>
        [Fact]
        public async Task CreateStudentDocumentWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            var response = await API.TestTools.ControllerMethods.SchoolsController.CreateStudentDocumentResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                _testCreateDocumentDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see>
        /// endpoint returns an <see cref="HttpStatusCode.InternalServerError">InternalServerError</see>
        /// response containing a list of model state errors when provided with an invalid
        /// <see cref="CreateDocumentDTO"/> object.
        /// </summary>
        /// <param name="invalidCreateDocumentDTO">The invalid <see cref="CreateDocumentDTO"/> to be provided.</param>
        /// <param name="expectedModelStateErrors">A <see cref="Dictionary{TKey,TValue}"/> containing the expected model state errors.</param>
        [Theory]
        [MemberData(nameof(InvalidCreateDocumentDTOs))]
        public async Task CreateStudentDocumentWithInvalidCreateDTOReturnsInternalServerError(
            CreateDocumentDTO invalidCreateDocumentDTO,
            Dictionary<string, string[]> expectedModelStateErrors)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            var response = await API.TestTools.ControllerMethods.SchoolsController.CreateStudentDocumentResponseAsync(
                _fixture.Client,
                _testSchool.Id.ToString(),
                _testStudent.Id.ToString(),
                invalidCreateDocumentDTO);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Dictionary<string, string[]> modelStateErrors = ModelStateTools.ParseModelStateErrors(response.ResponseBody);

            DictionaryResources.AssertEqual(expectedModelStateErrors, modelStateErrors);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see>
        /// endpoint returns an <see cref="HttpStatusCode.Unauthorized">Unauthorized</see> response containing an
        /// <see cref="ErrorResponseCode.InsufficientUserRole">InsufficientUserRole</see> error code when called
        /// by a <see cref="MartialBaseUser"/> who is not assigned to the
        /// <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see> role.
        /// </summary>
        /// <param name="roleName">The name of the role to be assigned to the requesting user during testing.</param>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonSchoolSecretaryRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonSchoolSecretaryUserCannotCreateStudentDocument(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            var response = await API.TestTools.ControllerMethods.SchoolsController.CreateStudentDocumentResponseAsync(
                _fixture.Client,
                _testSchool.Id.ToString(),
                _testStudent.Id.ToString(),
                _testCreateDocumentDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see>
        /// endpoint returns a <see cref="HttpStatusCode.NotFound">NotFound</see> response when provided with an
        /// invalid <see cref="School"/> ID.
        /// </summary>
        [Fact]
        public async Task CreateStudentDocumentForAStudentOfANonExistentSchoolReturnsNotFound()
        {
            // Arrange
            var invalidSchoolId = Guid.NewGuid();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            var response = await API.TestTools.ControllerMethods.SchoolsController.CreateStudentDocumentResponseAsync(
                _fixture.Client,
                invalidSchoolId.ToString(),
                _testStudent.Id.ToString(),
                _testCreateDocumentDTO);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"School ID '{invalidSchoolId}' not found.", response.ResponseBody);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see>
        /// endpoint returns a <see cref="HttpStatusCode.NotFound">NotFound</see> response when provided with a
        /// <see cref="CreateDocumentDTO"/> object containing an invalid <see cref="DocumentType"/> ID.
        /// </summary>
        [Fact]
        public async Task CreateStudentDocumentWithInvalidDocumentTypeReturnsNotFound()
        {
            // Arrange
            string invalidDocumentTypeId = Guid.NewGuid().ToString();

            _testCreateDocumentDTO.DocumentTypeId = invalidDocumentTypeId;

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            var response = await API.TestTools.ControllerMethods.SchoolsController.CreateStudentDocumentResponseAsync(
                _fixture.Client,
                _testSchool.Id.ToString(),
                _testStudent.Id.ToString(),
                _testCreateDocumentDTO);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Document type ID '{invalidDocumentTypeId}' not found.", response.ResponseBody);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see>
        /// endpoint returns an <see cref="HttpStatusCode.Unauthorized">Unauthorized</see> response containing a
        /// <see cref="ErrorResponseCode.AzureUserNotRegistered">AzureUserNotRegistered</see> error code when
        /// called by a <see cref="MartialBaseUser"/> who has no associated <see cref="Person"/> record.
        /// </summary>
        [Fact]
        public async Task BlockedSchoolSecretaryUserCannotCreateStudentDocument()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            var response = await API.TestTools.ControllerMethods.SchoolsController.CreateStudentDocumentResponseAsync(
                _fixture.Client,
                _testSchool.Id.ToString(),
                _testStudent.Id.ToString(),
                _testCreateDocumentDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see>
        /// endpoint successfully creates a <see cref="Document"/> and returns a representative
        /// <see cref="DocumentDTO"/> to a super user when the user is not registered/assigned to the requested
        /// <see cref="Person"/>'s <see cref="School"/>.
        /// </summary>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Fact]
        public async Task SuperUserCanCreateStudentDocumentForAStudentOfASchoolOfWhichTheyAreNotAMember()
        {
            // Arrange
            SchoolStudentResources.EnsureSchoolDoesNotHaveStudent(_testSchool.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            DocumentDTO retrievedDocument =
                await API.TestTools.ControllerMethods.SchoolsController.CreateStudentDocumentAsync(
                    _fixture.Client, _testSchool.Id.ToString(), _testStudent.Id.ToString(), _testCreateDocumentDTO);

            // Assert
            DocumentResources.AssertEqual(_testCreateDocumentDTO, retrievedDocument);
            DocumentResources.AssertExists(retrievedDocument, _fixture.DbIdentifier);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentAsync">GetStudentDocument</see> endpoint
        /// successfully creates a <see cref="Document"/> and returns a representative <see cref="DocumentDTO"/>
        /// to a super user when the user is registered/assigned to the requested <see cref="Person"/>'s
        /// <see cref="School"/>, but is not a secretary of it.
        /// </summary>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Fact]
        public async Task SuperUserCanCreateStudentDocumentForAStudentOfASchoolOfWhichTheyAreNotASecretary()
        {
            // Arrange
            SchoolStudentResources.EnsureSchoolHasStudent(
                _testSchool.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            DocumentDTO retrievedDocument =
                await API.TestTools.ControllerMethods.SchoolsController.CreateStudentDocumentAsync(
                    _fixture.Client, _testSchool.Id.ToString(), _testStudent.Id.ToString(), _testCreateDocumentDTO);

            // Assert
            DocumentResources.AssertEqual(_testCreateDocumentDTO, retrievedDocument);
            DocumentResources.AssertExists(retrievedDocument, DbIdentifier);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see>
        /// endpoint returns an <see cref="HttpStatusCode.Unauthorized">Unauthorized</see> response containing a
        /// <see cref="ErrorResponseCode.NoAccessToPerson">NoAccessToPerson</see> error code when the requesting
        /// user is not a member of the subject student's <see cref="School"/>.
        /// </summary>
        [Fact]
        public async Task SchoolSecretaryCannotCreateStudentDocumentForAStudentOfASchoolOfWhichTheyAreNotAMember()
        {
            // Arrange
            SchoolStudentResources.EnsureSchoolDoesNotHaveStudent(_testSchool.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            // Act
            var response = await API.TestTools.ControllerMethods.SchoolsController.CreateStudentDocumentResponseAsync(
                _fixture.Client,
                _testSchool.Id.ToString(),
                _testStudent.Id.ToString(),
                _testCreateDocumentDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotSchoolSecretary, response.ErrorResponseCode);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see>
        /// endpoint returns a <see cref="HttpStatusCode.NotFound">NotFound</see> response when provided with an
        /// invalid <see cref="Person"/> ID.
        /// </summary>
        [Fact]
        public async Task CreateStudentDocumentForANonExistentStudentReturnsNotFound()
        {
            // Arrange
            var invalidStudentId = Guid.NewGuid();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            var response = await API.TestTools.ControllerMethods.SchoolsController.CreateStudentDocumentResponseAsync(
                _fixture.Client,
                _testSchool.Id.ToString(),
                invalidStudentId.ToString(),
                _testCreateDocumentDTO);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Person ID '{invalidStudentId}' not found.", response.ResponseBody);
        }

        /// <summary>
        /// Verifies that the <see cref="Controllers.SchoolsController.GetStudentDocumentAsync">GetStudentDocument</see> endpoint
        /// successfully creates a <see cref="Document"/> and returns a representative <see cref="DocumentDTO"/>
        /// to a school secretary.
        /// </summary>
        /// <returns>It doesn't return anything, it's a test. Apparently SonarCloud doesn't seem to recognise
        /// that, so I need to add this pointless documentation to stop it rejecting my pull requests.</returns>
        [Fact]
        public async Task SchoolSecretaryCanCreateStudentDocument()
        {
            // Arrange
            SchoolStudentResources.EnsureSchoolHasStudent(
                _testSchool.Id, TestUserPersonId, DbIdentifier, false, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            // Act
            DocumentDTO retrievedDocument =
                await API.TestTools.ControllerMethods.SchoolsController.CreateStudentDocumentAsync(
                    _fixture.Client, _testSchool.Id.ToString(), _testStudent.Id.ToString(), _testCreateDocumentDTO);

            // Assert
            DocumentResources.AssertEqual(_testCreateDocumentDTO, retrievedDocument);
            DocumentResources.AssertExists(retrievedDocument, _fixture.DbIdentifier);
        }

        private void PrepareTestResources()
        {
            _testSchool = SchoolResources.CreateTestSchool(_fixture.DbIdentifier);
            _testStudent = PersonResources.CreateTestPerson(_fixture.DbIdentifier);

            DocumentType testDocumentType = DocumentTypeResources.CreateTestDocumentType(_fixture.DbIdentifier);
            _testCreateDocumentDTO = DataGenerator.Documents.GenerateCreateDocumentDTO(testDocumentType.Id);

            SchoolStudentResources.EnsureSchoolHasStudent(
                _testSchool.Id, _testStudent.Id, _fixture.DbIdentifier);
        }
    }
}
