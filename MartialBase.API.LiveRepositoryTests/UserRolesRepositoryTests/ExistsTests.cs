// <copyright file="ExistsTests.cs" company="Martialtech®">
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

namespace MartialBase.API.LiveRepositoryTests.UserRolesRepositoryTests
{
    public class ExistsTests : BaseTestClass
    {
        [Test]
        public async Task CanCheckUserRoleExists()
        {
            // Arrange
            var testRoles = UserRoleResources.GetUserRoles(DbIdentifier).ToList();

            foreach (UserRole testRole in testRoles)
            {
                // Act
                // Assert
                Assert.True(await UserRolesRepository.ExistsAsync(testRole.Id));
            }
        }

        [Test]
        public async Task CanCheckUserRoleDoesNotExist() =>

            // Act
            // Assert
            Assert.False(await UserRolesRepository.ExistsAsync(Guid.NewGuid()));
    }
}
