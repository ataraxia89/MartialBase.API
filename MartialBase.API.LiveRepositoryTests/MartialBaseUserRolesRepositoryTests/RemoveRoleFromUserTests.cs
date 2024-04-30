// <copyright file="RemoveRoleFromUserTests.cs" company="Martialtech®">
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
    public class RemoveRoleFromUserTests : BaseTestClass
    {
        [Test]
        public async Task CanRemoveRoleFromUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            Guid testRoleId = UserRoleResources.GetUserRoleId(UserRoles.OrganisationAdmin, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(testUser.Id, testRoleId, DbIdentifier);

            // Act
            await MartialBaseUserRolesRepository.RemoveRoleFromUserAsync(testUser.Id, testRoleId);

            // Assert
            Assert.True(await MartialBaseUserRolesRepository.SaveChangesAsync());

            MartialBaseUserRoleResources.AssertUserDoesNotHaveRole(testUser.Id, testRoleId, DbIdentifier);
        }

        [Test]
        public async Task CanRemoveRoleFromUserWhenUserAlreadyDoesNotHaveRole()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            Guid testRoleId = UserRoleResources.GetUserRoleId(UserRoles.OrganisationAdmin, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserDoesNotHaveRole(testUser.Id, testRoleId, DbIdentifier);

            // Act
            await MartialBaseUserRolesRepository.RemoveRoleFromUserAsync(testUser.Id, testRoleId);

            // Assert
            Assert.True(await MartialBaseUserRolesRepository.SaveChangesAsync());

            MartialBaseUserRoleResources.AssertUserDoesNotHaveRole(testUser.Id, testRoleId, DbIdentifier);
        }

        [Test]
        public async Task RemoveRoleFromNonExistentUserThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;
            Guid testRoleId = UserRoleResources.GetUserRoleId(UserRoles.OrganisationAdmin, DbIdentifier);

            // Act
            try
            {
                await MartialBaseUserRolesRepository.RemoveRoleFromUserAsync(Guid.NewGuid(), testRoleId);
            }
            catch (InvalidOperationException ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.NotNull(expectedException);
        }

        [Test]
        public async Task RemoveNonExistentRoleFromUserThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            // Act
            try
            {
                await MartialBaseUserRolesRepository.RemoveRoleFromUserAsync(testUser.Id, Guid.NewGuid());
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
