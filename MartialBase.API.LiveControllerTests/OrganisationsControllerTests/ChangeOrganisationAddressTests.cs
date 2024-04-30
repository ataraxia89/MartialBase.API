// <copyright file="ChangeOrganisationAddressTests.cs" company="Martialtech®">
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
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.TestResources;
using MartialBase.API.Tools;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.OrganisationsControllerTests
{
    [Collection("LiveControllerTests")]
    public class ChangeOrganisationAddressTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeOrganisationAddressTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public ChangeOrganisationAddressTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task ChangeOrganisationAddressWithExpiredTokenReturnsUnauthorized()
        {
            // Arrange
            _fixture.GenerateAuthorizationToken(-10);

            // Act
            var response = await OrganisationsController.ChangeOrganisationAddressResponseAsync(
                _fixture.Client,
                Guid.NewGuid().ToString(),
                DataGenerator.Addresses.GenerateCreateAddressDTO());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(
                $"error=\"invalid_token\", error_description=\"The token expired at '{_fixture.AuthTokenExpiry:MM/dd/yyyy HH:mm:ss}'\"",
                response.ResponseHeaders.WwwAuthenticate.First(rh => rh.Scheme == "Bearer").Parameter);
            Assert.Null(response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(CommonMemberData.InvalidCreateAddressDTOs), MemberType = typeof(CommonMemberData))]
        public async Task ChangeOrganisationAddressWithInvalidDTOReturnsInternalServerError(
            CreateAddressDTO invalidCreateAddressDTO, Dictionary<string, string[]> expectedModelStateErrors)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            AddressResources.AssertNotEqual(testOrganisation.Address, invalidCreateAddressDTO);

            // Act
            var response = await OrganisationsController.ChangeOrganisationAddressResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                invalidCreateAddressDTO);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Dictionary<string, string[]> modelStateErrors = ModelStateTools.ParseModelStateErrors(response.ResponseBody);

            DictionaryResources.AssertEqual(expectedModelStateErrors, modelStateErrors);
        }

        [Fact]
        public async Task BlockedOrganisationAdminUserCannotChangeOrganisationAddress()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            CreateAddressDTO newAddress = DataGenerator.Addresses.GenerateCreateAddressDTO();

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            _fixture.BlockTestUserAccess();

            // Act
            var response = await OrganisationsController.ChangeOrganisationAddressResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                newAddress);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.AzureUserNotRegistered, response.ErrorResponseCode);
        }

        [Theory]
        [MemberData(nameof(UserRoleResources.NonOrganisationAdminRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task NonOrganisationAdminUserCannotChangeOrganisationAddress(string roleName)
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            CreateAddressDTO newAddress = DataGenerator.Addresses.GenerateCreateAddressDTO();

            // This test is specifically for the OrganisationAdmin role, so the user can be admin of an
            // organisation, but they still must have this role assigned to them to carry out functions
            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            var response = await OrganisationsController.ChangeOrganisationAddressResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                newAddress);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.InsufficientUserRole, response.ErrorResponseCode);
        }

        [Fact]
        public async Task ChangeOrganisationAddressForNonExistentOrganisationReturnsNotFound()
        {
            // Arrange
            var invalidOrganisationId = Guid.NewGuid();
            CreateAddressDTO newAddress = DataGenerator.Addresses.GenerateCreateAddressDTO();

            OrganisationResources.AssertDoesNotExist(invalidOrganisationId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            var response = await OrganisationsController.ChangeOrganisationAddressResponseAsync(
                _fixture.Client,
                invalidOrganisationId.ToString(),
                newAddress);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Organisation ID '{invalidOrganisationId}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task ChangeOrganisationAddressWithNonExistentCountryCodeReturnsNotFound()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            CreateAddressDTO newAddress = DataGenerator.Addresses.GenerateCreateAddressDTO();
            string invalidCountryCode = "XXX";

            CountryResources.AssertDoesNotExist(invalidCountryCode);

            newAddress.CountryCode = invalidCountryCode;

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            var response = await OrganisationsController.ChangeOrganisationAddressResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                newAddress);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Country code '{invalidCountryCode}' not found.", response.ResponseBody);
        }

        [Fact]
        public async Task SuperUserCanChangeOrganisationAddressWhenTheyAreNotAMember()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            CreateAddressDTO newAddress = DataGenerator.Addresses.GenerateCreateAddressDTO();

            AddressResources.AssertNotEqual(testOrganisation.Address, newAddress);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            AddressDTO createdAddress = await OrganisationsController.ChangeOrganisationAddressAsync(
                _fixture.Client, testOrganisation.Id.ToString(), newAddress);

            // Assert
            AddressResources.AssertEqual(newAddress, createdAddress);
            AddressResources.AssertExists(createdAddress, DbIdentifier);

            // The organisation address details have been changed, but the ID will stay the same, so this
            // method will attempt to find the address by the details but not the ID
            Assert.False(AddressResources.CanFindByDetailsOnly(testOrganisation.Address, DbIdentifier));
        }

        [Fact]
        public async Task SuperUserCanChangeOrganisationAddressWhenTheyAreNotAnAdmin()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            CreateAddressDTO newAddress = DataGenerator.Addresses.GenerateCreateAddressDTO();

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, UserRoles.Thanos, DbIdentifier);

            // Act
            AddressDTO createdAddress = await OrganisationsController.ChangeOrganisationAddressAsync(
                _fixture.Client, testOrganisation.Id.ToString(), newAddress);

            // Assert
            AddressResources.AssertEqual(newAddress, createdAddress);
            AddressResources.AssertExists(createdAddress, DbIdentifier);

            // The organisation address details have been changed, but the ID will stay the same, so this
            // method will attempt to find the address by the details but not the ID
            Assert.False(AddressResources.CanFindByDetailsOnly(testOrganisation.Address, DbIdentifier));
        }

        [Fact]
        public async Task OrganisationAdminUserCannotChangeOrganisationAddressWhenTheyAreNotAMemberOfTheOrganisation()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            CreateAddressDTO newAddress = DataGenerator.Addresses.GenerateCreateAddressDTO();

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            var response = await OrganisationsController.ChangeOrganisationAddressResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                newAddress);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        [Fact]
        public async Task OrganisationAdminUserCannotChangeOrganisationAddressWhenTheyAreNotAnAdminOfTheOrganisation()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            CreateAddressDTO newAddress = DataGenerator.Addresses.GenerateCreateAddressDTO();

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            var response = await OrganisationsController.ChangeOrganisationAddressResponseAsync(
                _fixture.Client,
                testOrganisation.Id.ToString(),
                newAddress);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorResponseCode.NotOrganisationAdmin, response.ErrorResponseCode);
        }

        [Fact]
        public async Task OrganisationAdminUserCanChangeOrganisationAddress()
        {
            // Arrange
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            CreateAddressDTO newAddress = DataGenerator.Addresses.GenerateCreateAddressDTO();

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id, TestUserPersonId, DbIdentifier, true);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                TestUserId, UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            AddressDTO createdAddress = await OrganisationsController.ChangeOrganisationAddressAsync(
                _fixture.Client, testOrganisation.Id.ToString(), newAddress);

            // Assert
            AddressResources.AssertEqual(newAddress, createdAddress);
            AddressResources.AssertExists(createdAddress, _fixture.DbIdentifier);

            // The organisation address details have been changed, but the ID will stay the same, so this
            // method will attempt to find the address by the details but not the ID
            Assert.False(AddressResources.CanFindByDetailsOnly(testOrganisation.Address, DbIdentifier));
        }
    }
}
