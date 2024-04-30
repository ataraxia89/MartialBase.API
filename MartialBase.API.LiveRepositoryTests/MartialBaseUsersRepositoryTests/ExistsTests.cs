// <copyright file="ExistsTests.cs" company="Martialtech®">
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
    public class ExistsTests : BaseTestClass
    {
        [Test]
        public async Task CanCheckMartialBaseUserExists()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            // Act
            // Assert
            Assert.IsTrue(await MartialBaseUsersRepository.ExistsAsync(testUser.Id));
        }

        [Test]
        public async Task CanCheckMartialBaseUserDoesNotExist() =>

            // Act
            // Assert
            Assert.IsFalse(await MartialBaseUsersRepository.ExistsAsync(Guid.NewGuid()));
    }
}
