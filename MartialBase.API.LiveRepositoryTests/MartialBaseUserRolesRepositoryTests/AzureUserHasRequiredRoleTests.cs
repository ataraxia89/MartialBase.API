// <copyright file="AzureUserHasRequiredRoleTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.MartialBaseUserRolesRepositoryTests
{
    public class AzureUserHasRequiredRoleTests : BaseTestClass
    {
        [Test]
        public async Task CanCheckAzureUserHasRequiredRoleWithOneRoleProvided()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                testUser.Id, UserRoles.OrganisationAdmin, DbIdentifier);

            Assert.NotNull(testUser.AzureId);

            // Act
            // Assert
            Assert.True(await MartialBaseUserRolesRepository.AzureUserHasRequiredRoleAsync(
                (Guid)testUser.AzureId, UserRoles.OrganisationAdmin));
        }

        [Test]
        public async Task CanCheckAzureUserHasRequiredRoleWithMultipleRolesProvided()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                testUser.Id, UserRoles.OrganisationAdmin, DbIdentifier);

            Assert.NotNull(testUser.AzureId);

            var allowedRoles = new List<string> { UserRoles.OrganisationAdmin, UserRoles.OrganisationMember };

            // Act
            // Assert
            Assert.True(await MartialBaseUserRolesRepository.AzureUserHasRequiredRoleAsync(
                (Guid)testUser.AzureId, allowedRoles));
        }

        [Test]
        public async Task AzureUserHasRequiredRoleReturnsTrueForSuperUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                testUser.Id, UserRoles.Thanos, DbIdentifier);

            Assert.NotNull(testUser.AzureId);

            // Act
            // Assert
            Assert.True(await MartialBaseUserRolesRepository.AzureUserHasRequiredRoleAsync(
                (Guid)testUser.AzureId, string.Empty));
        }

        [Test]
        public async Task AzureUserHasRequiredRoleReturnsTrueForSuperUserWithRoleListProvided()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                testUser.Id, UserRoles.Thanos, DbIdentifier);

            Assert.NotNull(testUser.AzureId);

            // Act
            // Assert
            Assert.True(await MartialBaseUserRolesRepository.AzureUserHasRequiredRoleAsync(
                (Guid)testUser.AzureId, new List<string> { string.Empty }));
        }

        [Test]
        public async Task CanCheckAzureUserDoesNotHaveRequiredRoleWithOneRoleProvided()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                testUser.Id, UserRoles.OrganisationMember, DbIdentifier);

            Assert.NotNull(testUser.AzureId);

            // Act
            // Assert
            Assert.False(await MartialBaseUserRolesRepository.AzureUserHasRequiredRoleAsync(
                (Guid)testUser.AzureId, UserRoles.OrganisationAdmin));
        }

        [Test]
        public async Task CanCheckAzureUserDoesNotHaveRequiredRoleWithMultipleRolesProvided()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                testUser.Id, UserRoles.OrganisationMember, DbIdentifier);

            Assert.NotNull(testUser.AzureId);

            var allowedRoles = new List<string> { UserRoles.OrganisationAdmin, UserRoles.SchoolSecretary };

            // Act
            // Assert
            Assert.False(await MartialBaseUserRolesRepository.AzureUserHasRequiredRoleAsync(
                (Guid)testUser.AzureId, allowedRoles));
        }

        [Test]
        public async Task AzureUserHasRequiredRoleReturnsFalseForNonExistentUser() =>

            // Act
            // Assert
            Assert.False(await MartialBaseUserRolesRepository.AzureUserHasRequiredRoleAsync(
                Guid.NewGuid(), UserRoles.OrganisationAdmin));
    }
}
