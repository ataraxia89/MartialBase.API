// <copyright file="GetTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.MartialBaseUsers;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.MartialBaseUsersRepositoryTests
{
    public class GetTests : BaseTestClass
    {
        [Test]
        public async Task CanGetMartialBaseUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            // Act
            MartialBaseUserDTO retrievedUser = await MartialBaseUsersRepository.GetAsync(testUser.Id);

            // Assert
            MartialBaseUserResources.AssertEqual(testUser, retrievedUser);
        }

        [Test]
        public async Task GetNonExistentUserThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;

            // Act
            try
            {
                await MartialBaseUsersRepository.GetAsync(Guid.NewGuid());
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
