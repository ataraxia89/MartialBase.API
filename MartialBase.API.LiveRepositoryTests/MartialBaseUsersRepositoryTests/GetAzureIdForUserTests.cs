// <copyright file="GetAzureIdForUserTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.MartialBaseUsersRepositoryTests
{
    public class GetAzureIdForUserTests : BaseTestClass
    {
        [Test]
        public async Task CanGetAzureIdForUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            // Act
            // Assert
            Assert.AreEqual(testUser.AzureId, await MartialBaseUsersRepository.GetAzureIdForUserAsync(testUser.Id));
        }

        [Test]
        public async Task GetAzureIdForNonExistentUserReturnsNull() =>

            // Act
            // Assert
            Assert.Null(await MartialBaseUsersRepository.GetAzureIdForUserAsync(Guid.NewGuid()));
    }
}
