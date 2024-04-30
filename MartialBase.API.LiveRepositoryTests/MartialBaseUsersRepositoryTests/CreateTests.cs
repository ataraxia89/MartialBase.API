// <copyright file="CreateTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.MartialBaseUsers;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.MartialBaseUsersRepositoryTests
{
    public class CreateTests : BaseTestClass
    {
        [Test]
        public async Task CanCreateMartialBaseUser()
        {
            // Arrange
            CreateMartialBaseUserDTO createDTO =
                DataGenerator.MartialBaseUsers.GenerateCreateMartialBaseUserDTOObject();

            // Act
            MartialBaseUserDTO createdMartialBaseUser = await MartialBaseUsersRepository.CreateAsync(createDTO);

            // Assert
            Assert.IsTrue(await MartialBaseUsersRepository.SaveChangesAsync());

            MartialBaseUserResources.AssertEqual(createDTO, createdMartialBaseUser);
        }

        [Test]
        public async Task CanCreateMartialBaseUserWithExistingPerson()
        {
            // Arrange
            Person person = PersonResources.CreateTestPerson(DbIdentifier, false);

            CreateMartialBaseUserDTO createDTO =
                DataGenerator.MartialBaseUsers.GenerateCreateMartialBaseUserDTOObject(person);

            // Act
            MartialBaseUserDTO createdMartialBaseUser = await MartialBaseUsersRepository.CreateAsync(createDTO);

            // Assert
            Assert.IsTrue(await MartialBaseUsersRepository.SaveChangesAsync());

            MartialBaseUserResources.AssertEqual(createDTO, createdMartialBaseUser);
        }
    }
}
