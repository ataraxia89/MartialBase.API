// <copyright file="AddRoleToUserTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.MartialBaseUserRolesRepositoryTests
{
    public class AddRoleToUserTests : BaseTestClass
    {
        [Test]
        public async Task CanAddRoleToUserUsingRoleId()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserDoesNotHaveRole(
                testUser.Id, UserRoles.OrganisationAdmin, DbIdentifier);

            Guid roleId = UserRoleResources.GetUserRoleId(UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            await MartialBaseUserRolesRepository.AddRoleToUserAsync(testUser.Id, roleId);

            // Assert
            Assert.True(await MartialBaseUserRolesRepository.SaveChangesAsync());

            MartialBaseUserRoleResources.AssertUserHasRole(testUser.Id, roleId, DbIdentifier);
        }

        [Test]
        public async Task CanAddRoleToUserUsingRoleName()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserDoesNotHaveRole(
                testUser.Id, UserRoles.OrganisationAdmin, DbIdentifier);

            Guid roleId = UserRoleResources.GetUserRoleId(UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            await MartialBaseUserRolesRepository.AddRoleToUserAsync(testUser.Id, UserRoles.OrganisationAdmin);

            // Assert
            Assert.True(await MartialBaseUserRolesRepository.SaveChangesAsync());

            MartialBaseUserRoleResources.AssertUserHasRole(testUser.Id, roleId, DbIdentifier);
        }

        [Test]
        public async Task CanAddRoleToUserUsingRoleIdWhenRoleAlreadyExistsForUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                testUser.Id, UserRoles.OrganisationAdmin, DbIdentifier);

            Guid roleId = UserRoleResources.GetUserRoleId(UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            await MartialBaseUserRolesRepository.AddRoleToUserAsync(testUser.Id, roleId);

            // Assert
            MartialBaseUserRoleResources.AssertUserHasRole(testUser.Id, roleId, DbIdentifier);
        }

        [Test]
        public async Task CanAddRoleToUserUsingRoleNameWhenRoleAlreadyExistsForUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(
                testUser.Id, UserRoles.OrganisationAdmin, DbIdentifier);

            Guid roleId = UserRoleResources.GetUserRoleId(UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            await MartialBaseUserRolesRepository.AddRoleToUserAsync(testUser.Id, UserRoles.OrganisationAdmin);

            // Assert
            MartialBaseUserRoleResources.AssertUserHasRole(testUser.Id, roleId, DbIdentifier);
        }

        [Test]
        public async Task AddNonExistentRoleNameToUserThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            // Act
            try
            {
                await MartialBaseUserRolesRepository.AddRoleToUserAsync(testUser.Id, "NotARole");
            }
            catch (InvalidOperationException ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.NotNull(expectedException);
        }

        [Test]
        public async Task AddNonExistentRoleIdToUserThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            // Act
            try
            {
                await MartialBaseUserRolesRepository.AddRoleToUserAsync(testUser.Id, Guid.NewGuid());
            }
            catch (InvalidOperationException ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.NotNull(expectedException);
        }

        [Test]
        public async Task AddRoleNameToNonExistentUserThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;

            // Act
            try
            {
                await MartialBaseUserRolesRepository.AddRoleToUserAsync(Guid.NewGuid(), UserRoles.OrganisationAdmin);
            }
            catch (InvalidOperationException ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.NotNull(expectedException);
        }

        [Test]
        public async Task AddRoleIdToNonExistentUserThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;
            Guid testRoleId = UserRoleResources.GetUserRoleId(UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            try
            {
                await MartialBaseUserRolesRepository.AddRoleToUserAsync(Guid.NewGuid(), testRoleId);
            }
            catch (InvalidOperationException ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.NotNull(expectedException);
        }
    }
}
