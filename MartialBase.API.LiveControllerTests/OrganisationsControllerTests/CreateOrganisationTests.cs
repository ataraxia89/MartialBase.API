// <copyright file="CreateOrganisationTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.TestResources;
using MartialBase.API.Tools;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.OrganisationsControllerTests
{
    [Collection("LiveControllerTests")]
    public class CreateOrganisationTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateOrganisationTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public CreateOrganisationTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        public static IEnumerable<object[]> InvalidCreateOrganisationDTOs
        {
            get
            {
                object[] noInitialsProvided =
                {
                    new CreateOrganisationDTO
                    {
                        Name = "Test Organisation"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Initials", new[] { "Initials required." } }
                    }
                };

                object[] noNameProvided =
                {
                    new CreateOrganisationDTO
                    {
                        Initials = "TSTORG"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Name", new[] { "Name required." } }
                    }
                };

                object[] initialsTooLong =
                {
                    new CreateOrganisationDTO
                    {
                        Initials = new string('*', 9),
                        Name = "Test Organisation"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Initials", new[] { "Initials cannot be longer than 8 characters." } }
                    }
                };

                object[] nameTooLong =
                {
                    new CreateOrganisationDTO
                    {
                        Initials = "TSTORG",
                        Name = new string('*', 61)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Name", new[] { "Name cannot be longer than 60 characters." } }
                    }
                };

                object[] parentIdTooLong =
                {
                    new CreateOrganisationDTO
                    {
                        Initials = "TSTORG",
                        Name = "Test Organisation",
                        ParentId = new string('*', 69)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "ParentId", new[] { "Parent ID cannot be longer than 68 characters." } }
                    }
                };

                var invalidCreateOrganisationDTOs = new List<object[]>
                {
                    noInitialsProvided,
                    noNameProvided,
                    initialsTooLong,
                    nameTooLong,
                    parentIdTooLong
                };

                foreach (object[] invalidCreateAddressDTO in CommonMemberData.InvalidCreateAddressDTOs)
                {
                    var invalidAddressErrors = new Dictionary<string, string[]>();

                    foreach (KeyValuePair<string, string[]> addressError in
                        (Dictionary<string, string[]>)invalidCreateAddressDTO[1])
                    {
                        invalidAddressErrors.Add($"Address.{addressError.Key}", addressError.Value);
                    }

                    object[] invalidOrganisationDTO =
                    {
                        new CreateOrganisationDTO
                        {
                            Initials = "TSTORG",
                            Name = "Test Organisation",
                            Address = (CreateAddressDTO)invalidCreateAddressDTO[0]
                        },
                        invalidAddressErrors
                    };

                    invalidCreateOrganisationDTOs.Add(invalidOrganisationDTO);
                }

                return invalidCreateOrganisationDTOs;
            }
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task CreateOrganisationWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            var response = await OrganisationsController.CreateOrganisationResponseAsync(
                _fixture.Client,
                DataGenerator.Organisations.GenerateCreateOrganisationDTOObject());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Fact]
        public async Task SuperUserCanCreateOrganisationAndBecomeOrganisationAdmin()
        {
            // Arrange
            CreateOrganisationDTO createOrganisationDTO =
                DataGenerator.Organisations.GenerateCreateOrganisationDTOObject();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            OrganisationDTO createdOrganisation =
                await OrganisationsController.CreateOrganisationAsync(_fixture.Client, createOrganisationDTO);

            // Assert
            OrganisationResources.AssertEqual(createOrganisationDTO, createdOrganisation);

            Organisation dbOrganisation = OrganisationResources.GetOrganisation(
                new Guid(createdOrganisation.Id), DbIdentifier);

            OrganisationResources.AssertEqual(createOrganisationDTO, dbOrganisation);

            OrganisationPerson organisationPerson = OrganisationPersonResources.GetOrganisationPerson(
                dbOrganisation.Id, TestUserPersonId, DbIdentifier);

            Assert.NotNull(organisationPerson);
            Assert.True(organisationPerson.IsOrganisationAdmin);
        }

        [Fact]
        public async Task BasicUserCanCreateOrganisationAndBecomeOrganisationAdmin()
        {
            // Arrange
            CreateOrganisationDTO createOrganisationDTO =
                DataGenerator.Organisations.GenerateCreateOrganisationDTOObject();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.User, DbIdentifier);

            // Act
            OrganisationDTO createdOrganisation =
                await OrganisationsController.CreateOrganisationAsync(_fixture.Client, createOrganisationDTO);

            // Assert
            OrganisationResources.AssertEqual(createOrganisationDTO, createdOrganisation);

            Organisation dbOrganisation = OrganisationResources.GetOrganisation(
                new Guid(createdOrganisation.Id), DbIdentifier);

            OrganisationResources.AssertEqual(createOrganisationDTO, dbOrganisation);

            OrganisationPerson organisationPerson = OrganisationPersonResources.GetOrganisationPerson(
                dbOrganisation.Id, TestUserPersonId, DbIdentifier);

            Assert.NotNull(organisationPerson);
            Assert.True(organisationPerson.IsOrganisationAdmin);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.AllUserRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task BlockedUserCannotCreateOrganisation(string roleName)
        {
            // Arrange
            CreateOrganisationDTO createOrganisationDTO =
                DataGenerator.Organisations.GenerateCreateOrganisationDTOObject();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            var response = await OrganisationsController.CreateOrganisationResponseAsync(
                _fixture.Client, createOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(InvalidCreateOrganisationDTOs))]
        public async Task CreateOrganisationWithInvalidCreateDTOReturnsInternalServerError(
            CreateOrganisationDTO invalidCreateOrganisationDTO,
            Dictionary<string, string[]> expectedModelStateErrors)
        {
            // Arrange
            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            var response = await OrganisationsController.CreateOrganisationResponseAsync(
                _fixture.Client, invalidCreateOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Dictionary<string, string[]> modelStateErrors = ModelStateTools.ParseModelStateErrors(response.ResponseBody);

            DictionaryResources.AssertEqual(expectedModelStateErrors, modelStateErrors);
        }

        [Fact]
        public async Task CreateOrganisationWithNonExistentParentReturnsNotFound()
        {
            // Arrange
            var invalidOrganisationId = Guid.NewGuid();

            OrganisationResources.AssertDoesNotExist(invalidOrganisationId, DbIdentifier);

            CreateOrganisationDTO createOrganisationDTO =
                DataGenerator.Organisations.GenerateCreateOrganisationDTOObject();

            createOrganisationDTO.ParentId = invalidOrganisationId.ToString();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            var response = await OrganisationsController.CreateOrganisationResponseAsync(
                _fixture.Client, createOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Organisation ID '{invalidOrganisationId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task CreateOrganisationWithParentOfWhichTheUserIsNotAdminReturnsUnauthorized()
        {
            // Arrange
            Organisation parentOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                parentOrganisation.Id, TestUserPersonId, DbIdentifier);

            CreateOrganisationDTO createOrganisationDTO =
                DataGenerator.Organisations.GenerateCreateOrganisationDTOObject();

            createOrganisationDTO.ParentId = parentOrganisation.Id.ToString();

            // The user is added as OrganisationAdmin because this isn't a test of an admin of *any* organisation,
            // it's a test of *this* organisation. If the user does not have the OrganisationAdmin role then an
            // unauthorized response may be returned prematurely (i.e. before checking for their access to the
            // parent).
            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            var response = await OrganisationsController.CreateOrganisationResponseAsync(
                _fixture.Client, createOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        [Fact]
        public async Task CreateOrganisationWithParentOfWhichTheUserIsNotMemberReturnsUnauthorized()
        {
            // Arrange
            Organisation parentOrganisation =
                OrganisationResources.CreateTestOrganisation(_fixture.DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                parentOrganisation.Id, TestUserPersonId, DbIdentifier);

            CreateOrganisationDTO createOrganisationDTO =
                DataGenerator.Organisations.GenerateCreateOrganisationDTOObject();

            createOrganisationDTO.ParentId = parentOrganisation.Id.ToString();

            // The user is added as OrganisationAdmin because the endpoint requires this permission, but the user
            // may be an admin of the new organisation yet not a member of the parent organisation
            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            var response = await OrganisationsController.CreateOrganisationResponseAsync(
                _fixture.Client, createOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        [Fact]
        public async Task SuperUserCanCreateOrganisationWithParentOfWhichTheyAreNotAnAdmin()
        {
            // Arrange
            Organisation parentOrganisation =
                OrganisationResources.CreateTestOrganisation(_fixture.DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                parentOrganisation.Id, TestUserPersonId, DbIdentifier);

            CreateOrganisationDTO createOrganisationDTO =
                DataGenerator.Organisations.GenerateCreateOrganisationDTOObject();

            createOrganisationDTO.ParentId = parentOrganisation.Id.ToString();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            OrganisationDTO createdOrganisation =
                await OrganisationsController.CreateOrganisationAsync(_fixture.Client, createOrganisationDTO);

            // Assert
            OrganisationResources.AssertEqual(createOrganisationDTO, createdOrganisation);
            OrganisationResources.AssertExists(createdOrganisation, _fixture.DbIdentifier);
        }

        [Fact]
        public async Task SuperUserCanCreateOrganisationWithParentOfWhichTheyAreNotAMember()
        {
            // Arrange
            Organisation parentOrganisation =
                OrganisationResources.CreateTestOrganisation(_fixture.DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                parentOrganisation.Id, TestUserPersonId, DbIdentifier);

            CreateOrganisationDTO createOrganisationDTO =
                DataGenerator.Organisations.GenerateCreateOrganisationDTOObject();

            createOrganisationDTO.ParentId = parentOrganisation.Id.ToString();

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            OrganisationDTO createdOrganisation =
                await OrganisationsController.CreateOrganisationAsync(_fixture.Client, createOrganisationDTO);

            // Assert
            OrganisationResources.AssertEqual(createOrganisationDTO, createdOrganisation);
            OrganisationResources.AssertExists(createdOrganisation, _fixture.DbIdentifier);
        }

        [Fact]
        public async Task CreateOrganisationWithNonExistentAddressCountryReturnsNotFound()
        {
            // Arrange
            string invalidCountryCode = "XXX";

            CountryResources.AssertDoesNotExist(invalidCountryCode);

            CreateOrganisationDTO createOrganisationDTO =
                DataGenerator.Organisations.GenerateCreateOrganisationDTOObject();

            createOrganisationDTO.Address.CountryCode = invalidCountryCode;

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            var response = await OrganisationsController.CreateOrganisationResponseAsync(
                _fixture.Client, createOrganisationDTO);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Country code '{invalidCountryCode}' not found.", response.ResponseBody);
        }
    }
}
