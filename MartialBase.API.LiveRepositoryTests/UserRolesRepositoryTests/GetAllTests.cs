// <copyright file="GetAllTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.UserRolesRepositoryTests
{
    public class GetAllTests : BaseTestClass
    {
        [Test]
        public async Task CanGetAllUserRoles()
        {
            // Arrange
            var testRoles = UserRoleResources.GetUserRoles(DbIdentifier).ToList();

            // Act
            List<UserRole> retrievedRoles = await UserRolesRepository.GetAllAsync();

            // Assert
            UserRoleResources.AssertEqual(testRoles, retrievedRoles);
        }
    }
}
