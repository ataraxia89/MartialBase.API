// <copyright file="UpdatePersonTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;
using MartialBase.API.Tools;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.PeopleControllerTests
{
    /// <summary>
    /// Live controller tests for <see cref="Controllers.PeopleController.UpdatePersonAsync"/>.
    /// </summary>
    [Collection("LiveControllerTests")]
    public class UpdatePersonTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePersonTests"/> class.
        /// </summary>
        /// <param name="fixture">The <see cref="TestServerFixture"/> to be used by tests.</param>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        public UpdatePersonTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        /// <summary>
        /// Gets a collection of invalid <see cref="UpdatePersonDTO"/> objects, each with a
        /// <see cref="Dictionary{TKey,TValue}">Dictionary</see> of expected ModelState errors.
        /// </summary>
        public static IEnumerable<object[]> InvalidUpdatePersonDTOs
        {
            get
            {
                var invalidUpdatePersonDTOs = new List<object[]>();

                UpdatePersonDTO template = DataGenerator.People.GenerateUpdatePersonDTO(false);

                invalidUpdatePersonDTOs.Add(new object[]
                {
                    new UpdatePersonDTO()
                    {
                        Title = new string('*', 16),
                        FirstName = template.FirstName,
                        MiddleName = template.MiddleName,
                        LastName = template.LastName,
                        Address = template.Address,
                        Email = template.Email,
                        MobileNo = template.MobileNo
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Title", new[] { "Title cannot be longer than 15 characters." } }
                    }
                });

                invalidUpdatePersonDTOs.Add(new object[]
                {
                    new UpdatePersonDTO()
                    {
                        Title = template.Title,
                        FirstName = new string('*', 16),
                        MiddleName = template.MiddleName,
                        LastName = template.LastName,
                        Address = template.Address,
                        Email = template.Email,
                        MobileNo = template.MobileNo
                    },
                    new Dictionary<string, string[]>
                    {
                        { "FirstName", new[] { "First name cannot be longer than 15 characters." } }
                    }
                });

                invalidUpdatePersonDTOs.Add(new object[]
                {
                    new UpdatePersonDTO()
                    {
                        Title = template.Title,
                        FirstName = template.FirstName,
                        MiddleName = new string('*', 36),
                        LastName = template.LastName,
                        Address = template.Address,
                        Email = template.Email,
                        MobileNo = template.MobileNo
                    },
                    new Dictionary<string, string[]>
                    {
                        { "MiddleName", new[] { "Middle name cannot be longer than 35 characters." } }
                    }
                });

                invalidUpdatePersonDTOs.Add(new object[]
                {
                    new UpdatePersonDTO()
                    {
                        Title = template.Title,
                        FirstName = template.FirstName,
                        MiddleName = template.MiddleName,
                        LastName = new string('*', 26),
                        Address = template.Address,
                        Email = template.Email,
                        MobileNo = template.MobileNo
                    },
                    new Dictionary<string, string[]>
                    {
                        { "LastName", new[] { "Last name cannot be longer than 25 characters." } }
                    }
                });

                invalidUpdatePersonDTOs.Add(new object[]
                {
                    new UpdatePersonDTO()
                    {
                        Title = template.Title,
                        FirstName = template.FirstName,
                        MiddleName = template.MiddleName,
                        LastName = template.LastName,
                        Address = template.Address,
                        Email = new string('*', 46),
                        MobileNo = template.MobileNo
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Email", new[] { "Email cannot be longer than 45 characters." } }
                    }
                });

                invalidUpdatePersonDTOs.Add(new object[]
                {
                    new UpdatePersonDTO()
                    {
                        Title = template.Title,
                        FirstName = template.FirstName,
                        MiddleName = template.MiddleName,
                        LastName = template.LastName,
                        Address = template.Address,
                        Email = template.Email,
                        MobileNo = new string('*', 31)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "MobileNo", new[] { "Mobile no. cannot be longer than 30 characters." } }
                    }
                });

                foreach (object[] invalidUpdateAddressDTO in CommonMemberData.InvalidUpdateAddressDTOs)
                {
                    var invalidAddressErrors = new Dictionary<string, string[]>();

                    foreach (KeyValuePair<string, string[]> addressError in
                        (Dictionary<string, string[]>)invalidUpdateAddressDTO[1])
                    {
                        invalidAddressErrors.Add($"Address.{addressError.Key}", addressError.Value);
                    }

                    invalidUpdatePersonDTOs.Add(new object[]
                    {
                        new UpdatePersonDTO()
                        {
                            Title = template.Title,
                            FirstName = template.FirstName,
                            MiddleName = template.MiddleName,
                            LastName = template.LastName,
                            Address = (UpdateAddressDTO)invalidUpdateAddressDTO[0],
                            Email = template.Email,
                            MobileNo = template.MobileNo
                        },
                        invalidAddressErrors
                    });
                }

                return invalidUpdatePersonDTOs;
            }
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> with an expired authorization token returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result.
        /// </summary>
        [Fact]
        public async Task UpdatePersonWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                DataGenerator.People.GenerateUpdatePersonDTO(false));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> from a non-admin user returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result.
        /// </summary>
        /// <param name="roleName">The non-admin role to use in the request.</param>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonPersonAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonAdminUserCannotUpdatePerson(string roleName)
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                testPerson.Id.ToString(),
                DataGenerator.People.GenerateUpdatePersonDTO(false));

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> from a non-admin user is successful where the
        /// <see cref="Person"/> to be updated is the same as the requesting user.
        /// </summary>
        /// <param name="roleName">The non-admin role to use in the request.</param>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonPersonAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonAdminUserCanUpdateTheirOwnPersonDetails(string roleName)
        {
            // Arrange
            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(true);

            PersonResources.AssertNotEqual(_fixture.TestUser.Person, updatePersonDTO);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            PersonDTO updatedPerson =
                await API.TestTools.ControllerMethods.PeopleController.UpdatePersonAsync(_fixture.Client, TestUserPersonId.ToString(), updatePersonDTO);

            // Assert
            PersonResources.AssertEqual(updatePersonDTO, updatedPerson);
            PersonResources.AssertExists(updatedPerson, DbIdentifier);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> from a
        /// <see cref="UserRoles.SystemAdmin">SystemAdmin</see> user returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result.
        /// </summary>
        [Fact]
        public async Task SystemAdminUserCannotUpdatePerson()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SystemAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                testPerson.Id.ToString(),
                DataGenerator.People.GenerateUpdatePersonDTO(false));

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/>  from a non-super user with no associated
        /// <see cref="Person"/> record returns a <see cref="HttpStatusCode.Forbidden">Forbidden</see> result.
        /// </summary>
        /// <param name="roleName">The name of the <see cref="UserRole"/> to be assigned to the test user.</param>
        [Theory]
        [InlineData(UserRoles.SchoolSecretary)]
        [InlineData(UserRoles.OrganisationAdmin)]
        public async Task NonSuperUserCannotUpdatePersonIfTheyAreBlocked(string roleName)
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                testPerson.Id.ToString(),
                DataGenerator.People.GenerateUpdatePersonDTO(false));

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> from a super user is successful, even when
        /// the person to be updated is not the same as the requesting user nor do they have <see cref="School"/>
        /// or <see cref="Organisation"/> access to the person.
        /// </summary>
        [Fact]
        public async Task SuperUserCanUpdatePerson()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(true);

            PersonResources.AssertNotEqual(testPerson, updatePersonDTO);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            PersonDTO updatedPerson =
                await API.TestTools.ControllerMethods.PeopleController.UpdatePersonAsync(_fixture.Client, testPerson.Id.ToString(), updatePersonDTO);

            // Assert
            PersonResources.AssertEqual(updatePersonDTO, updatedPerson);
            PersonResources.AssertExists(updatedPerson, DbIdentifier);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> from an
        /// <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> user is successful where the
        /// requesting user is admin of the <see cref="Organisation"/> that the <see cref="Person"/> to be
        /// updated is a member of.
        /// </summary>
        [Fact]
        public async Task OrganisationAdminUserCanUpdatePerson()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier);

            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(true);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            PersonDTO updatedPerson =
                await API.TestTools.ControllerMethods.PeopleController.UpdatePersonAsync(_fixture.Client, testPerson.Id.ToString(), updatePersonDTO);

            // Assert
            PersonResources.AssertEqual(updatePersonDTO, updatedPerson);
            PersonResources.AssertExists(updatedPerson, DbIdentifier);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> from a
        /// <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see> user is successful where the requesting
        /// user is a secretary of the <see cref="School"/> that the <see cref="Person"/> to be updated is a
        /// student of.
        /// </summary>
        [Fact]
        public async Task SchoolSecretaryCanUpdatePerson()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testPerson.Id, DbIdentifier);

            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(true);

            SchoolResources.EnsurePersonIsSecretary(testSchool, _fixture.TestUser.Person, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            // Act
            PersonDTO updatedPerson =
                await API.TestTools.ControllerMethods.PeopleController.UpdatePersonAsync(_fixture.Client, testPerson.Id.ToString(), updatePersonDTO);

            // Assert
            PersonResources.AssertEqual(updatePersonDTO, updatedPerson);
            PersonResources.AssertExists(updatedPerson, DbIdentifier);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> from a non-admin user returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result, even if the requesting user is a member
        /// of the same <see cref="Organisation"/> as the <see cref="Person"/> being updated.
        /// </summary>
        /// <param name="roleName">The non-admin role to use in the request.</param>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonPersonAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationAdminUserCannotUpdatePersonRegardlessOfOrganisationMembership(string roleName)
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier);

            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(true);

            PersonResources.AssertNotEqual(testPerson, updatePersonDTO);

            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                testPerson.Id.ToString(),
                updatePersonDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> from a non-admin user returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result, even if the requesting user is a student
        /// of the same <see cref="School"/> as the <see cref="Person"/> being updated.
        /// </summary>
        /// <param name="roleName">The non-admin role to use in the request.</param>
        [Theory]
        [MemberData(nameof(UserRoleResources.NonPersonAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonSchoolAdminUserCannotUpdatePersonRegardlessOfSchoolMembership(string roleName)
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier);

            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(true);

            PersonResources.AssertNotEqual(testPerson, updatePersonDTO);

            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                testPerson.Id.ToString(),
                updatePersonDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> from an
        /// <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result if the requesting user is not an admin
        /// of the <see cref="Organisation"/> to which the <see cref="Person"/> to be updated belongs.
        /// </summary>
        [Fact]
        public async Task OrganisationAdminCannotUpdateAPersonIfTheyAreNotAdminOfThePersonsOrganisation()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier);

            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, testPerson.Id, DbIdentifier);

            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(true);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                testPerson.Id.ToString(),
                updatePersonDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NoAccessToPerson, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> from a
        /// <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see> returns a
        /// <see cref="HttpStatusCode.Forbidden">Forbidden</see> result if the requesting user is not a secretary
        /// of the <see cref="School"/> to which the <see cref="Person"/> to be updated belongs.
        /// </summary>
        [Fact]
        public async Task SchoolSecretaryCannotUpdateAPersonOutsideOfTheirSchoolEvenIfTheyAreAStudent()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testPerson.Id, DbIdentifier);

            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(true);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.SchoolSecretary, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                testPerson.Id.ToString(),
                updatePersonDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NoAccessToPerson, response.ErrorResponseCode);
        }

        /// <summary>
        /// Ensures that a request to update a non-existent <see cref="Person"/> returns a
        /// <see cref="HttpStatusCode.NotFound">NotFound</see> result.
        /// </summary>
        [Fact]
        public async Task UpdateNonExistentPersonReturnsNotFound()
        {
            // Arrange
            var invalidPersonId = Guid.NewGuid();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                invalidPersonId.ToString(),
                DataGenerator.People.GenerateUpdatePersonDTO(false));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Person ID '{invalidPersonId}' not found.", response.ResponseBody);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> with an invalid country code returns a
        /// <see cref="HttpStatusCode.NotFound">NotFound</see> result.
        /// </summary>
        [Fact]
        public async Task UpdatePersonWithInvalidCountryReturnsNotFound()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(true);

            string invalidCountryCode = "***";

            updatePersonDTO.Address.CountryCode = invalidCountryCode;

            PersonResources.AssertNotEqual(testPerson, updatePersonDTO);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                testPerson.Id.ToString(),
                updatePersonDTO);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Country code '{invalidCountryCode}' not found.", response.ResponseBody);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> without specifying an ID returns a
        /// <see cref="HttpStatusCode.MethodNotAllowed">MethodNotAllowed</see> result.
        /// </summary>
        /// <remarks>This is because the URL requested would be the base "/people" endpoint, which does not
        /// support PUT requests.</remarks>
        [Fact]
        public async Task UpdatePersonWithNoPersonIdParameterReturnsMethodNotAllowed()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                null,
                DataGenerator.People.GenerateUpdatePersonDTO(false));

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            Assert.Null(response.ResponseBody);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> without providing an
        /// <see cref="UpdatePersonDTO"/> object returns an
        /// <see cref="HttpStatusCode.InternalServerError">InternalServerError</see> result.
        /// </summary>
        [Fact]
        public async Task UpdatePersonWithNoUpdatePersonDTOParameterReturnsInternalServerError()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                null);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var expectedModelStateErrors = new Dictionary<string, string[]>
            {
                { string.Empty, new[] { "A non-empty request body is required." } }
            };

            Dictionary<string, string[]> modelStateErrors = ModelStateTools.ParseModelStateErrors(response.ResponseBody);

            DictionaryResources.AssertEqual(expectedModelStateErrors, modelStateErrors);
        }

        /// <summary>
        /// Ensures that a request to update a <see cref="Person"/> with an invalid <see cref="UpdatePersonDTO"/>
        /// returns an <see cref="HttpStatusCode.InternalServerError">InternalServerError</see> result.
        /// </summary>
        /// <param name="invalidUpdatePersonDTO">The invalid <see cref="UpdatePersonDTO"/> to submit.</param>
        /// <param name="expectedModelStateErrors">The expected model state errors to be returned.</param>
        [Theory]
        [MemberData(nameof(InvalidUpdatePersonDTOs))]
        public async Task UpdatePersonWithInvalidDTOReturnsInternalServerError(
            UpdatePersonDTO invalidUpdatePersonDTO, Dictionary<string, string[]> expectedModelStateErrors)
        {
            // Arrange
            PersonResources.AssertNotEqual(_fixture.TestUser.Person, invalidUpdatePersonDTO);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await API.TestTools.ControllerMethods.PeopleController.UpdatePersonResponseAsync(
                _fixture.Client,
                TestUserPersonId.ToString(),
                invalidUpdatePersonDTO);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Dictionary<string, string[]> modelStateErrors = ModelStateTools.ParseModelStateErrors(response.ResponseBody);

            DictionaryResources.AssertEqual(expectedModelStateErrors, modelStateErrors);
        }
    }
}
