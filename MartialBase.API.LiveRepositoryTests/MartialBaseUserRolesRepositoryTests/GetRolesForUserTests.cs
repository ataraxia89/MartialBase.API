// <copyright file="GetRolesForUserTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.UserRoles;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.MartialBaseUserRolesRepositoryTests
{
    public class GetRolesForUserTests : BaseTestClass
    {
        [Test]
        public async Task CanGetRolesForUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            var testUserRoles = UserRoleResources.GetUserRoles(DbIdentifier).ToList();

            foreach (UserRole testUserRole in testUserRoles)
            {
                MartialBaseUserRoleResources.EnsureUserHasRole(
                    testUser.Id, testUserRole.Id, DbIdentifier, false);
            }

            // Act
            List<UserRoleDTO> retrievedRoles = await MartialBaseUserRolesRepository.GetRolesForUserAsync(testUser.Id);

            // Assert
            UserRoleResources.AssertEqual(testUserRoles, retrievedRoles);
        }

        [Test]
        public async Task GetRolesForUserReturnsEmptyListForNonExistentUser() =>

            // Act
            // Assert
            Assert.IsEmpty(await MartialBaseUserRolesRepository.GetRolesForUserAsync(Guid.NewGuid()));
    }
}
