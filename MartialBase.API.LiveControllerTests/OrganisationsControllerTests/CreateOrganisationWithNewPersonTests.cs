// <copyright file="CreateOrganisationWithNewPersonTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.TestResources;
using MartialBase.API.Tools;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.OrganisationsControllerTests
{
    [Collection("LiveControllerTests")]
    public class CreateOrganisationWithNewPersonTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateOrganisationWithNewPersonTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public CreateOrganisationWithNewPersonTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        public static IEnumerable<object[]> InvalidCreatePersonOrganisationDTOs
        {
            get
            {
                var invalidCreatePersonOrganisationDTOs = new List<object[]>();

                foreach (object[] invalidCreateOrganisationDTO in
                    CreateOrganisationTests.InvalidCreateOrganisationDTOs)
                {
                    var invalidOrganisationErrors = new Dictionary<string, string[]>();

                    foreach (KeyValuePair<string, string[]> addressError in
                        (Dictionary<string, string[]>)invalidCreateOrganisationDTO[1])
                    {
                        invalidOrganisationErrors.Add($"Organisation.{addressError.Key}", addressError.Value);
                    }

                    object[] invalidCreatePersonOrganisationDTO =
                    {
                        new CreatePersonOrganisationDTO
                        {
                            Organisation = (CreateOrganisationDTO)invalidCreateOrganisationDTO[0],
                            Person = new CreatePersonDTO
                            {
                                FirstName = "John",
                                LastName = "Smith"
                            }
                        },
                        invalidOrganisationErrors
                    };

                    invalidCreatePersonOrganisationDTOs.Add(invalidCreatePersonOrganisationDTO);
                }

                foreach (object[] invalidCreateOrganisationAddressDTO in CommonMemberData.InvalidCreateAddressDTOs)
                {
                    var invalidOrganisationAddressErrors = new Dictionary<string, string[]>();

                    foreach (KeyValuePair<string, string[]> addressError in
                        (Dictionary<string, string[]>)invalidCreateOrganisationAddressDTO[1])
                    {
                        invalidOrganisationAddressErrors.Add(
                            $"Organisation.Address.{addressError.Key}", addressError.Value);
                    }

                    object[] invalidOrganisationDTO =
                    {
                        new CreatePersonOrganisationDTO
                        {
                            Organisation = new CreateOrganisationDTO
                            {
                                Initials = "TSTORG",
                                Name = "Test Organisation",
                                Address = (CreateAddressDTO)invalidCreateOrganisationAddressDTO[0]
                            },
                            Person = new CreatePersonDTO
                            {
                                FirstName = "John",
                                LastName = "Smith"
                            }
                        },
                        invalidOrganisationAddressErrors
                    };

                    invalidCreatePersonOrganisationDTOs.Add(invalidOrganisationDTO);
                }

                foreach (object[] invalidCreatePersonDTO in CommonMemberData.InvalidCreatePersonDTOs)
                {
                    var invalidPersonErrors = new Dictionary<string, string[]>();

                    foreach (KeyValuePair<string, string[]> addressError in
                        (Dictionary<string, string[]>)invalidCreatePersonDTO[1])
                    {
                        invalidPersonErrors.Add($"Person.{addressError.Key}", addressError.Value);
                    }

                    object[] invalidCreatePersonOrganisationDTO =
                    {
                        new CreatePersonOrganisationDTO
                        {
                            Organisation = new CreateOrganisationDTO
                            {
                                Initials = "TSTORG",
                                Name = "Test Organisation"
                            },
                            Person = (CreatePersonDTO)invalidCreatePersonDTO[0]
                        },
                        invalidPersonErrors
                    };

                    invalidCreatePersonOrganisationDTOs.Add(invalidCreatePersonOrganisationDTO);
                }

                return invalidCreatePersonOrganisationDTOs;
            }
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task CreateOrganisationWithNewPersonWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            var response = await OrganisationsController.CreateOrganisationWithNewPersonAsAdminResponseAsync(
                _fixture.Client,
                DataGenerator.Organisations.GenerateCreatePersonOrganisationDTOObject());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Fact]
        public async Task BasicUserCanCreatePersonOrganisationForThemselves()
        {
            // Arrange
            Organisation testOrganisation = DataGenerator.Organisations.GenerateOrganisationObject();
            Person testPerson = DataGenerator.People.GeneratePersonObject();
            CreatePersonOrganisationDTO createPersonOrganisationDTO =
                DataGenerator.Organisations.GenerateCreatePersonOrganisationDTOObject(testOrganisation, testPerson);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.User, DbIdentifier);

            _fixture.RemoveTestUser();

            // Act
            PersonOrganisationDTO createdPersonOrganisation =
                await OrganisationsController.CreateOrganisationWithNewPersonAsAdminAsync(
                    _fixture.Client, createPersonOrganisationDTO);

            // Assert
            OrganisationResources.AssertEqual(
                createPersonOrganisationDTO.Organisation, createdPersonOrganisation.Organisation);

            PersonResources.AssertEqual(
                createPersonOrganisationDTO.Person, createdPersonOrganisation.Person);

            OrganisationPerson organisationPerson = OrganisationPersonResources.GetOrganisationPerson(
                new Guid(createdPersonOrganisation.Organisation.Id),
                createdPersonOrganisation.Person.Id,
                DbIdentifier);

            Assert.NotNull(organisationPerson);
            Assert.True(organisationPerson.IsOrganisationAdmin);

            OrganisationResources.AssertEqual(
                createPersonOrganisationDTO.Organisation,
                organisationPerson.Organisation);
            PersonResources.AssertEqual(createPersonOrganisationDTO.Person, organisationPerson.Person);

            Assert.Equal(
                organisationPerson.Person.Id,
                PersonResources.GetPersonFromAzureId(_fixture.AzureId.ToString(), DbIdentifier).Id);
        }

        [Theory]
        [MemberData(nameof(InvalidCreatePersonOrganisationDTOs))]
        public async Task CreateOrganisationWithNewPersonWithInvalidCreateDTOReturnsInternalServerError(
            CreatePersonOrganisationDTO invalidCreatePersonOrganisationDTO,
            Dictionary<string, string[]> expectedModelStateErrors)
        {
            // Arrange
            _fixture.BlockTestUserAccess();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            var response = await OrganisationsController.CreateOrganisationWithNewPersonAsAdminResponseAsync(
                _fixture.Client, invalidCreatePersonOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Dictionary<string, string[]> modelStateErrors = ModelStateTools.ParseModelStateErrors(response.ResponseBody);

            DictionaryResources.AssertEqual(expectedModelStateErrors, modelStateErrors);
        }

        [Fact]
        public async Task CreateOrganisationWithNewPersonWithNonExistentOrganisationCountryReturnsNotFound()
        {
            // Arrange
            _fixture.BlockTestUserAccess();

            string invalidCountryCode = "XXX";

            CountryResources.AssertDoesNotExist(invalidCountryCode);

            Organisation testOrganisation = DataGenerator.Organisations.GenerateOrganisationObject();
            Person testPerson = DataGenerator.People.GeneratePersonObject();
            CreatePersonOrganisationDTO createPersonOrganisationDTO =
                DataGenerator.Organisations.GenerateCreatePersonOrganisationDTOObject(testOrganisation, testPerson);

            createPersonOrganisationDTO.Organisation.Address.CountryCode = invalidCountryCode;

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            var response = await OrganisationsController.CreateOrganisationWithNewPersonAsAdminResponseAsync(
                _fixture.Client, createPersonOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Country code '{invalidCountryCode}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task CreateOrganisationWithNewPersonWithNonExistentPersonCountryReturnsNotFound()
        {
            // Arrange
            _fixture.BlockTestUserAccess();

            string invalidCountryCode = "XXX";

            CountryResources.AssertDoesNotExist(invalidCountryCode);

            Organisation testOrganisation = DataGenerator.Organisations.GenerateOrganisationObject();
            Person testPerson = DataGenerator.People.GeneratePersonObject();
            CreatePersonOrganisationDTO createPersonOrganisationDTO =
                DataGenerator.Organisations.GenerateCreatePersonOrganisationDTOObject(testOrganisation, testPerson);

            createPersonOrganisationDTO.Person.Address.CountryCode = invalidCountryCode;

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            var response = await OrganisationsController.CreateOrganisationWithNewPersonAsAdminResponseAsync(
                _fixture.Client, createPersonOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Country code '{invalidCountryCode}' not found.", response.ResponseBody);
        }
    }
}
