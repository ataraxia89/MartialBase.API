// <copyright file="GetPersonIdForUserTests.cs" company="Martialtech®">
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
    public class GetPersonIdForUserTests : BaseTestClass
    {
        [Test]
        public async Task CanGetPersonIdForUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            // Act
            // Assert
            Assert.AreEqual(testUser.PersonId, await MartialBaseUsersRepository.GetPersonIdForUserAsync(testUser.Id));
        }

        [Test]
        public async Task GetPersonIdForNonExistentUserReturnsNull() =>

            // Act
            // Assert
            Assert.Null(await MartialBaseUsersRepository.GetPersonIdForUserAsync(Guid.NewGuid()));
    }
}
