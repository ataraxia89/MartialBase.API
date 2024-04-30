// <copyright file="GetAllTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.MartialBaseUsers;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.MartialBaseUsersRepositoryTests
{
    public class GetAllTests : BaseTestClass
    {
        [Test]
        public async Task CanGetAllMartialBaseUsers()
        {
            // Arrange
            List<MartialBaseUser> testUsers = MartialBaseUserResources.CreateMartialBaseUsers(10, DbIdentifier);

            // Act
            List<MartialBaseUserDTO> retrievedUsers = await MartialBaseUsersRepository.GetAllAsync();

            // Assert
            MartialBaseUserResources.AssertEqual(testUsers, retrievedUsers);
            MartialBaseUserResources.AssertExist(testUsers, DbIdentifier);
        }
    }
}
