// <copyright file="MartialBaseUserResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MartialBase.API.Data;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.MartialBaseUsers;
using MartialBase.API.Tools;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class MartialBaseUserResources
    {
        internal static bool CheckExists(string userId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.MartialBaseUsers.Any(u => u.Id == new Guid(userId));
            }
        }

        internal static void AssertExist(List<MartialBaseUser> users, string dbIdentifier)
        {
            foreach (MartialBaseUser user in users)
            {
                AssertExists(user, dbIdentifier);
            }
        }

        internal static void AssertExists(MartialBaseUser user, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                MartialBaseUser checkUser = dbContext.MartialBaseUsers
                    .Include(mbu => mbu.Person)
                    .ThenInclude(p => p.Address)
                    .FirstOrDefault(mbu => mbu.Id == user.Id);

                AssertEqual(checkUser, user);
            }
        }

        internal static List<MartialBaseUser> CreateMartialBaseUsers(int numberToCreate, string dbIdentifier, bool azureRegistered = true, bool realisticData = false)
        {
            var users = new List<MartialBaseUser>();

            for (int i = 0; i < numberToCreate; i++)
            {
                MartialBaseUser newUser = CreateMartialBaseUser(dbIdentifier, azureRegistered, realisticData);

                users.Add(newUser);
            }

            return users;
        }

        internal static MartialBaseUser CreateMartialBaseUser(string dbIdentifier, bool azureRegistered = true, bool realisticData = false)
        {
            MartialBaseUser user = DataGenerator.MartialBaseUsers.GenerateMartialBaseUserObject(azureRegistered, realisticData);

            using (MartialBaseDbContext context = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                context.Addresses.Add(user.Person.Address);
                context.People.Add(user.Person);
                context.MartialBaseUsers.Add(user);

                Assert.True(context.SaveChanges() > 0);
            }

            using (MartialBaseDbContext context = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(context.MartialBaseUsers.Any(u => u.Id == user.Id));
            }

            return user;
        }

        internal static async Task<MartialBaseUser> CreateMartialBaseUserAsync(string dbIdentifier, bool azureRegistered = true, bool realisticData = false)
        {
            MartialBaseUser user = DataGenerator.MartialBaseUsers.GenerateMartialBaseUserObject(azureRegistered, realisticData);

            await using (MartialBaseDbContext context = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                await context.Addresses.AddAsync(user.Person.Address);
                await context.People.AddAsync(user.Person);
                await context.MartialBaseUsers.AddAsync(user);

                Assert.True(await context.SaveChangesAsync() > 0);
            }

            await using (MartialBaseDbContext context = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(await context.MartialBaseUsers.AnyAsync(u => u.Id == user.Id));
            }

            return user;
        }

        internal static MartialBaseUser GetUser(string userId, string dbIdentifier)
        {
            using (MartialBaseDbContext context = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return context.MartialBaseUsers.FirstOrDefault(u => u.Id == new Guid(userId));
            }
        }

        internal static MartialBaseUser GetUser(Guid userId, string dbIdentifier)
        {
            using (MartialBaseDbContext context = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return context.MartialBaseUsers
                    .Include(u => u.Person)
                    .ThenInclude(p => p.Address)
                    .FirstOrDefault(u => u.Id == userId);
            }
        }

        internal static MartialBaseUser GetUserFromPersonId(Guid personId, string dbIdentifier)
        {
            using (MartialBaseDbContext context = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return context.MartialBaseUsers
                    .Include(u => u.Person)
                    .ThenInclude(p => p.Address)
                    .FirstOrDefault(u => u.PersonId == personId);
            }
        }

        internal static void AssertEqual(MartialBaseUser expected, MartialBaseUser actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.AzureId, actual.AzureId);
            Assert.Equal(expected.InvitationCode, actual.InvitationCode);

            if (expected.Person != null)
            {
                Assert.NotNull(actual.Person);
                PersonResources.AssertEqual(expected.Person, actual.Person);
            }
            else
            {
                Assert.Null(actual.Person);
            }
        }

        internal static void AssertEqual(MartialBaseUser expected, MartialBaseUserDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id.ToString(), actual.Id);
            Assert.Equal(expected.AzureId?.ToString(), actual.AzureId);
            Assert.Equal(expected.InvitationCode, actual.InvitationCode);

            if (expected.Person != null)
            {
                Assert.NotNull(actual.Person);
                PersonResources.AssertEqual(expected.Person, actual.Person);
            }
            else
            {
                Assert.Null(actual.Person);
            }
        }

        internal static void AssertEqual(CreateMartialBaseUserDTO expected, MartialBaseUser actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.AzureId, actual.AzureId?.ToString());
            Assert.Equal(expected.InvitationCode, actual.InvitationCode);

            if (expected.Person != null)
            {
                Assert.NotNull(actual.Person);
                PersonResources.AssertEqual(expected.Person, actual.Person);
            }
            else
            {
                Assert.Null(actual.Person);
            }
        }

        internal static void AssertEqual(CreateMartialBaseUserDTO expected, MartialBaseUserDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.AzureId, actual.AzureId);
            Assert.Equal(expected.InvitationCode, actual.InvitationCode);

            if (expected.Person != null)
            {
                Assert.NotNull(actual.Person);
                PersonResources.AssertEqual(expected.Person, actual.Person);
            }
        }

        internal static void AssertEqual(List<MartialBaseUser> expected, List<MartialBaseUserDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (MartialBaseUser expectedUser in expected)
            {
                MartialBaseUserDTO actualUser = actual.FirstOrDefault(mbu =>
                    mbu.Id == expectedUser.Id.ToString());

                AssertEqual(expectedUser, actualUser);
            }
        }

        internal static void RemoveAzureId(MartialBaseUser user, string dbIdentifier)
        {
            using (MartialBaseDbContext context = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                MartialBaseUser dbUser = context.MartialBaseUsers.First(mbu => mbu.Id == user.Id);

                dbUser.AzureId = null;

                Assert.True(context.SaveChanges() > 0);
            }

            using (MartialBaseDbContext context = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(context.MartialBaseUsers.Any(mbu =>
                    mbu.Id == user.Id &&
                    mbu.AzureId == null));
            }
        }

        internal static void RemoveAzureIdAndInvitationCode(MartialBaseUser user, string dbIdentifier)
        {
            using (MartialBaseDbContext context = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                MartialBaseUser dbUser = context.MartialBaseUsers.First(mbu => mbu.Id == user.Id);

                dbUser.AzureId = null;
                dbUser.InvitationCode = null;

                Assert.True(context.SaveChanges() > 0);
            }

            using (MartialBaseDbContext context = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(context.MartialBaseUsers.Any(mbu =>
                    mbu.Id == user.Id &&
                    mbu.AzureId == null &&
                    mbu.InvitationCode == null));
            }
        }
    }
}
