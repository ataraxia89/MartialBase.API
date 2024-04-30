// <copyright file="GenerateInvitationCodeTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Text.RegularExpressions;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.MartialBaseUsersRepositoryTests
{
    public class GenerateInvitationCodeTests : BaseTestClass
    {
        [Test]
        public async Task CanGenerateInvitationCode()
        {
            // Arrange
            MartialBaseUser testUser = MartialBaseUserResources.CreateMartialBaseUser(DbIdentifier);

            MartialBaseUserResources.RemoveAzureIdAndInvitationCode(testUser, DbIdentifier);

            // Act
            string invitationCode = await MartialBaseUsersRepository.GenerateInvitationCodeAsync(testUser.Id);

            // Assert
            Assert.True(!string.IsNullOrEmpty(invitationCode));
            Assert.AreEqual(7, invitationCode.Length);
            Assert.True(Regex.Match(invitationCode, "^[A-Z0-9]+$").Success);
            Assert.True(await MartialBaseUsersRepository.SaveChangesAsync());

            MartialBaseUser checkUser = MartialBaseUserResources.GetUser(testUser.Id, DbIdentifier);

            Assert.Null(checkUser.AzureId);
            Assert.AreEqual(invitationCode, checkUser.InvitationCode);
        }

        [Test]
        public async Task GenerateInvitationCodeForNonExistentMartialBaseUserThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;

            // Act
            try
            {
                await MartialBaseUsersRepository.GenerateInvitationCodeAsync(Guid.NewGuid());
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
