// <copyright file="DisassociateAzureUserTests.cs" company="Martialtech®">
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
    public class DisassociateAzureUserTests : BaseTestClass
    {
        [Test]
        public async Task CanDisassociateAzureUser()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            // Act
            await MartialBaseUsersRepository.DisassociateAzureUserAsync(testUser.Id);

            // Assert
            Assert.True(await MartialBaseUsersRepository.SaveChangesAsync());

            MartialBaseUser checkUser = MartialBaseUserResources.GetUser(testUser.Id, DbIdentifier);

            Assert.Null(checkUser.AzureId);
            Assert.Null(checkUser.InvitationCode);
        }

        [Test]
        public async Task DisassociateNonExistentAzureUserThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;

            // Act
            try
            {
                await MartialBaseUsersRepository.DisassociateAzureUserAsync(Guid.NewGuid());
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
