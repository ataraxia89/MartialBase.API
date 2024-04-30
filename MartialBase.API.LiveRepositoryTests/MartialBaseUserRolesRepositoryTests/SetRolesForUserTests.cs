// <copyright file="SetRolesForUserTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Linq;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.MartialBaseUserRolesRepositoryTests
{
    public class SetRolesForUserTests : BaseTestClass
    {
        [Test]
        public async Task CanSetRolesForUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            var testUserRoles = UserRoleResources.GetUserRoles(DbIdentifier).ToList();

            foreach (UserRole testUserRole in testUserRoles)
            {
                MartialBaseUserRoleResources.EnsureUserDoesNotHaveRole(
                    testUser.Id, testUserRole.Id, DbIdentifier);
            }

            // Act
            await MartialBaseUserRolesRepository.SetRolesForUserAsync(testUser.Id, testUserRoles.Select(ur => ur.Id).ToList());

            // Assert
            Assert.True(await MartialBaseUserRolesRepository.SaveChangesAsync());

            foreach (UserRole testUserRole in testUserRoles)
            {
                MartialBaseUserRoleResources.AssertUserHasRole(testUser.Id, testUserRole.Id, DbIdentifier);
            }
        }

        [Test]
        public async Task CanAddRoleToUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            var testUserRoles = UserRoleResources.GetUserRoles(DbIdentifier).ToList();

            foreach (UserRole testUserRole in testUserRoles)
            {
                MartialBaseUserRoleResources.EnsureUserHasRole(testUser.Id, testUserRole.Id, DbIdentifier);
            }

            MartialBaseUserRoleResources.EnsureUserDoesNotHaveRole(
                testUser.Id, testUserRoles[0].Id, DbIdentifier);

            // Act
            await MartialBaseUserRolesRepository.SetRolesForUserAsync(testUser.Id, testUserRoles.Select(ur => ur.Id).ToList());

            // Assert
            Assert.True(await MartialBaseUserRolesRepository.SaveChangesAsync());

            MartialBaseUserRoleResources.AssertUserHasRole(testUser.Id, testUserRoles[0].Id, DbIdentifier);
        }

        [Test]
        public async Task CanRemoveRoleFromUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            var testUserRoles = UserRoleResources.GetUserRoles(DbIdentifier).ToList();

            foreach (UserRole testUserRole in testUserRoles)
            {
                MartialBaseUserRoleResources.EnsureUserHasRole(testUser.Id, testUserRole.Id, DbIdentifier);
            }

            UserRole testRemoveRole = testUserRoles[0];

            testUserRoles.Remove(testRemoveRole);

            // Act
            await MartialBaseUserRolesRepository.SetRolesForUserAsync(testUser.Id, testUserRoles.Select(ur => ur.Id).ToList());

            // Assert
            Assert.True(await MartialBaseUserRolesRepository.SaveChangesAsync());

            MartialBaseUserRoleResources.AssertUserDoesNotHaveRole(testUser.Id, testRemoveRole.Id, DbIdentifier);
        }

        [Test]
        public async Task CanSetRolesForUserWhenRoleAlreadyExistsForUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            var testUserRoles = UserRoleResources.GetUserRoles(DbIdentifier).ToList();

            foreach (UserRole testRole in testUserRoles)
            {
                MartialBaseUserRoleResources.EnsureUserHasRole(testUser.Id, testRole.Id, DbIdentifier);
            }

            // Act
            await MartialBaseUserRolesRepository.SetRolesForUserAsync(testUser.Id, testUserRoles.Select(ur => ur.Id).ToList());

            // Assert
            Assert.True(await MartialBaseUserRolesRepository.SaveChangesAsync());

            MartialBaseUserRoleResources.AssertUserHasRole(testUser.Id, testUserRoles[0].Id, DbIdentifier);
        }

        [Test]
        public async Task SetNonExistentRoleIdForUserThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            // Act
            try
            {
                await MartialBaseUserRolesRepository.SetRolesForUserAsync(testUser.Id, new() { Guid.NewGuid() });
            }
            catch (InvalidOperationException ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.NotNull(expectedException);
        }

        [Test]
        public async Task SetRolesForNonExistentUserThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;
            var testUserRoles = UserRoleResources.GetUserRoles(DbIdentifier).ToList();

            // Act
            try
            {
                await MartialBaseUserRolesRepository.SetRolesForUserAsync(
                    Guid.NewGuid(), testUserRoles.Select(ur => ur.Id).ToList());
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
