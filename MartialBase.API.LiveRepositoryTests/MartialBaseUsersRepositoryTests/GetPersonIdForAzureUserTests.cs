// <copyright file="GetPersonIdForAzureUserTests.cs" company="Martialtech®">
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
    public class GetPersonIdForAzureUserTests : BaseTestClass
    {
        [Test]
        public async Task CanGetPersonIdForAzureUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            Assert.NotNull(testUser.AzureId);

            // Act
            Guid? retrievedId = await MartialBaseUsersRepository.GetPersonIdForAzureUserAsync(
                    (Guid)testUser.AzureId, testUser.InvitationCode);

            // Assert
            Assert.AreEqual(testUser.PersonId, retrievedId);
        }

        [Test]
        public async Task CanGetPersonIdForAzureUserWithInvitationCode()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserResources.RemoveAzureId(testUser, DbIdentifier);

            // Act
            Guid? retrievedId = await MartialBaseUsersRepository.GetPersonIdForAzureUserAsync(
                Guid.NewGuid(), testUser.InvitationCode);

            // Assert
            Assert.AreEqual(testUser.PersonId, retrievedId);
            Assert.Null(MartialBaseUserResources.GetUser(testUser.Id, DbIdentifier).InvitationCode);
        }

        [Test]
        public async Task GetPersonIdForAzureUserReturnsNullWhenNoAzureIdOrInvitationCodeStored()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            Guid? testAzureId = testUser.AzureId;
            string testInvitationCode = testUser.InvitationCode;

            MartialBaseUserResources.RemoveAzureIdAndInvitationCode(testUser, DbIdentifier);

            // Act
            // Assert
            Assert.NotNull(testAzureId);
            Assert.Null(await MartialBaseUsersRepository.GetPersonIdForAzureUserAsync(
                (Guid)testAzureId, testInvitationCode));
        }
    }
}
