// <copyright file="GetUserIdForPersonTests.cs" company="Martialtech®">
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
    public class GetUserIdForPersonTests : BaseTestClass
    {
        [Test]
        public async Task CanGetUserIdForPerson()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            // Act
            // Assert
            Assert.AreEqual(testUser.Id, await MartialBaseUsersRepository.GetUserIdForPersonAsync(testUser.PersonId));
        }

        [Test]
        public async Task GetUserIdForNonExistentPersonThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;

            // Act
            try
            {
                await MartialBaseUsersRepository.GetUserIdForPersonAsync(Guid.NewGuid());
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
