// <copyright file="FindPeopleTests.cs" company="Martialtech®">
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
    [Collection("LiveControllerTests")]
    public class FindPeopleTests : IClassFixture<TestServerFixture>
    {
        private readonly string _testPersonEmail;
        private readonly string _testPersonFirstName;
        private readonly string _testPersonMiddleName;
        private readonly string _testPersonLastName;

        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindPeopleTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public FindPeopleTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _testPersonEmail = $"{RandomData.GetRandomString(38)}@me.com";
            _testPersonFirstName = RandomData.GetRandomString(15);
            _testPersonMiddleName = RandomData.GetRandomString(35);
            _testPersonLastName = RandomData.GetRandomString(25);

            _fixture.SetUpUnitTest();
        }

        public static IEnumerable<object[]> FindPeopleParameters
        {
            get
            {
                object[] emailOnly = { false, true, true, true };

                object[] firstNameOnly = { true, false, true, true };

                object[] middleNameOnly = { true, true, false, true };

                object[] lastNameOnly = { true, true, true, false };

                object[] emailAndFirstName = { false, false, true, true };

                object[] emailAndMiddleName = { false, true, false, true };

                object[] emailAndLastName = { false, true, true, false };

                object[] firstNameAndMiddleName = { true, false, false, true };

                object[] firstNameAndLastName = { true, false, true, false };

                object[] middleNameAndLastName = { true, true, false, false };

                object[] emailAndFirstNameAndMiddleName = { false, false, false, true };

                object[] emailAndFirstNameAndLastName = { false, false, true, false };

                object[] emailAndMiddleNameAndLastName = { false, true, false, false };

                object[] firstNameAndMiddleNameAndLastName = { true, false, false, false };

                var findPeopleParameters = new List<object[]>
                {
                    emailOnly,
                    firstNameOnly,
                    middleNameOnly,
                    lastNameOnly,
                    emailAndFirstName,
                    emailAndMiddleName,
                    emailAndLastName,
                    firstNameAndMiddleName,
                    firstNameAndLastName,
                    middleNameAndLastName,
                    emailAndFirstNameAndMiddleName,
                    emailAndFirstNameAndLastName,
                    emailAndMiddleNameAndLastName,
                    firstNameAndMiddleNameAndLastName
                };

                var returnParameters = new List<object[]>();

                foreach (object[] role in UserRoleResources.SystemAdminRoleNames)
                {
                    foreach (object[] parameters in findPeopleParameters)
                    {
                        returnParameters.Add(new[] { role[0], parameters[0], parameters[1], parameters[2], parameters[3] });
                    }
                }

                return returnParameters;
            }
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task FindPeopleWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            HttpResponseModel response = await PeopleController.FindPeopleResponseAsync(
                _fixture.Client,
                null,
                null,
                null,
                null,
                null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Fact]
        public async Task FindPeopleWithNoParametersReturnsBadRequest()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await PeopleController.FindPeopleResponseAsync(
                _fixture.Client,
                null,
                null,
                null,
                null,
                null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NoSearchParameters, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonSystemAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonSystemAdminUserCannotFindPeople(string roleName)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            HttpResponseModel response = await PeopleController.FindPeopleResponseAsync(
                _fixture.Client,
                _testPersonEmail,
                _testPersonFirstName,
                _testPersonMiddleName,
                _testPersonLastName,
                "true");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(FindPeopleParameters))]
        public async Task SystemAdminUserCanFindPeople(string roleName, bool emailIsNull, bool firstNameIsNull, bool middleNameIsNull, bool lastNameIsNull)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            Person templatePerson = DataGenerator.People.GeneratePersonObject();
            templatePerson.Email = _testPersonEmail;
            templatePerson.FirstName = _testPersonFirstName;
            templatePerson.MiddleName = _testPersonMiddleName;
            templatePerson.LastName = _testPersonLastName;

            List<Person> testPeople = PersonResources.CreateTestPeople(templatePerson, 10, _fixture.DbIdentifier, true);

            // Ensure there are other people on the database too as a control measure.
            PersonResources.CreateTestPeople(10, _fixture.DbIdentifier);

            string searchEmail = emailIsNull ? null : _testPersonEmail;
            string searchFirstName = firstNameIsNull ? null : _testPersonFirstName;
            string searchMiddleName = middleNameIsNull ? null : _testPersonMiddleName;
            string searchLastName = lastNameIsNull ? null : _testPersonLastName;

            // Act
            List<PersonDTO> retrievedPeople =
                await PeopleController.FindPeopleAsync(
                    _fixture.Client, searchEmail, searchFirstName, searchMiddleName, searchLastName, "true");

            // Assert
            PersonResources.AssertEqual(testPeople, retrievedPeople);
        }

        [Fact]
        public async Task CanFindPeopleWithNoAddresses()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            Person templatePerson = DataGenerator.People.GeneratePersonObject();
            templatePerson.Email = _testPersonEmail;
            templatePerson.FirstName = _testPersonFirstName;
            templatePerson.MiddleName = _testPersonMiddleName;
            templatePerson.LastName = _testPersonLastName;

            List<Person> testPeople = PersonResources.CreateTestPeople(templatePerson, 10, _fixture.DbIdentifier, true);

            foreach (Person testPerson in testPeople)
            {
                testPerson.AddressId = null;
                testPerson.Address = null;
            }

            // Ensure there are other people on the database too as a control measure.
            PersonResources.CreateTestPeople(10, _fixture.DbIdentifier);

            // Act
            List<PersonDTO> retrievedPeople =
                await PeopleController.FindPeopleAsync(
                    _fixture.Client, _testPersonEmail, _testPersonFirstName, _testPersonMiddleName, _testPersonLastName, "false");

            // Assert
            PersonResources.AssertEqual(testPeople, retrievedPeople);
        }

        [Fact]
        public async Task FindPeopleWithNoAddressParameterSpecifiedReturnsNoAddresses()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            Person templatePerson = DataGenerator.People.GeneratePersonObject();
            templatePerson.Email = _testPersonEmail;
            templatePerson.FirstName = _testPersonFirstName;
            templatePerson.MiddleName = _testPersonMiddleName;
            templatePerson.LastName = _testPersonLastName;

            List<Person> testPeople = PersonResources.CreateTestPeople(templatePerson, 10, _fixture.DbIdentifier, true);

            foreach (Person testPerson in testPeople)
            {
                testPerson.AddressId = null;
                testPerson.Address = null;
            }

            // Ensure there are other people on the database too as a control measure.
            PersonResources.CreateTestPeople(10, _fixture.DbIdentifier);

            // Act
            List<PersonDTO> retrievedPeople =
                await PeopleController.FindPeopleAsync(
                    _fixture.Client, _testPersonEmail, _testPersonFirstName, _testPersonMiddleName, _testPersonLastName, null);

            // Assert
            PersonResources.AssertEqual(testPeople, retrievedPeople);
        }

        [Fact]
        public async Task FindPeopleWithInvalidAddressParameterReturnsBadRequest()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            HttpResponseModel response = await PeopleController.FindPeopleResponseAsync(
                _fixture.Client,
                _testPersonEmail,
                _testPersonFirstName,
                _testPersonMiddleName,
                _testPersonLastName,
                "Not-A-Bool");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Invalid value provided for returnAddresses parameter.", response.ResponseBody);
        }

        [Fact]
        public async Task FindPeopleWithNoMatchingParametersReturnsEmptyBody()
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            List<PersonDTO> retrievedPeople =
                await PeopleController.FindPeopleAsync(
                    _fixture.Client,
                    _testPersonEmail,
                    _testPersonFirstName,
                    _testPersonMiddleName,
                    _testPersonLastName,
                    "true");

            // Assert
            Assert.Empty(retrievedPeople);
        }
    }
}
