// <copyright file="MartialBaseUserRoleResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data;
using MartialBase.API.Data.Models.EntityFramework;

using Microsoft.EntityFrameworkCore;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class MartialBaseUserRoleResources
    {
        internal static void AssertUserHasRole(Guid userId, Guid roleId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.MartialBaseUserRoles.Any(mbu =>
                    mbu.MartialBaseUserId == userId &&
                    mbu.UserRoleId == roleId));
            }
        }

        internal static void AssertUserDoesNotHaveRole(Guid userId, Guid roleId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.False(dbContext.MartialBaseUserRoles.Any(mbu =>
                    mbu.MartialBaseUserId == userId &&
                    mbu.UserRoleId == roleId));
            }
        }

        internal static void EnsureUserHasRole(Guid userId, string roleName, string dbIdentifier, bool removeOtherRoles = true)
        {
            Guid roleId;

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.UserRoles.Any(ur =>
                    ur.Name == roleName));

                roleId = dbContext.UserRoles.First(ur =>
                    ur.Name == roleName).Id;
            }

            EnsureUserHasRole(userId, roleId, dbIdentifier, removeOtherRoles);
        }

        internal static async Task EnsureUserHasRolesAsync(Guid userId, IEnumerable<string> roleNames, string dbIdentifier, bool removeOtherRoles = true)
        {
            var roleIds = new List<Guid>();

            await using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                foreach (string roleName in roleNames)
                {
                    Assert.True(await dbContext.UserRoles.AnyAsync(ur =>
                        ur.Name == roleName));

                    roleIds.Add((await dbContext.UserRoles.FirstAsync(ur =>
                        ur.Name == roleName)).Id);
                }
            }

            await EnsureUserHasRolesAsync(userId, roleIds, dbIdentifier, removeOtherRoles);
        }

        internal static void EnsureUserHasRole(Guid userId, Guid roleId, string dbIdentifier, bool removeOtherRoles = true)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                if (removeOtherRoles)
                {
                    dbContext.MartialBaseUserRoles.RemoveRange(
                        dbContext.MartialBaseUserRoles.Where(mur => mur.MartialBaseUserId == userId));
                }

                dbContext.MartialBaseUserRoles.Add(new MartialBaseUserRole
                {
                    Id = Guid.NewGuid(),
                    MartialBaseUserId = userId,
                    UserRoleId = roleId
                });

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.MartialBaseUserRoles.Any(mur =>
                    mur.MartialBaseUserId == userId &&
                    mur.UserRoleId == roleId));
            }
        }

        internal static async Task EnsureUserHasRolesAsync(Guid userId, IEnumerable<Guid> roleIds, string dbIdentifier, bool removeOtherRoles = true)
        {
            var roleIdsList = roleIds.ToList();

            await using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                if (removeOtherRoles)
                {
                    dbContext.MartialBaseUserRoles.RemoveRange(
                        await dbContext.MartialBaseUserRoles
                            .Where(mur => mur.MartialBaseUserId == userId)
                            .ToListAsync());
                }

                foreach (Guid roleId in roleIdsList)
                {
                    await dbContext.MartialBaseUserRoles.AddAsync(new MartialBaseUserRole
                    {
                        Id = Guid.NewGuid(),
                        MartialBaseUserId = userId,
                        UserRoleId = roleId
                    });
                }

                Assert.True(await dbContext.SaveChangesAsync() > 0);
            }

            await using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                foreach (Guid roleId in roleIdsList)
                {
                    Assert.True(await dbContext.MartialBaseUserRoles.AnyAsync(mur =>
                        mur.MartialBaseUserId == userId &&
                        mur.UserRoleId == roleId));
                }
            }
        }

        internal static void EnsureUserDoesNotHaveRole(Guid userId, string roleName, string dbIdentifier)
        {
            Guid roleId;

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.UserRoles.Any(ur =>
                    ur.Name == roleName));

                roleId = dbContext.UserRoles.First(ur =>
                    ur.Name == roleName).Id;
            }

            EnsureUserDoesNotHaveRole(userId, roleId, dbIdentifier);
        }

        internal static void EnsureUserDoesNotHaveRole(Guid userId, Guid roleId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                MartialBaseUserRole userRole = dbContext.MartialBaseUserRoles.FirstOrDefault(mbur =>
                    mbur.MartialBaseUserId == userId &&
                    mbur.UserRoleId == roleId);

                if (userRole != null)
                {
                    dbContext.Remove(userRole);

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.False(dbContext.MartialBaseUserRoles.Any(mbur =>
                    mbur.MartialBaseUserId == userId &&
                    mbur.UserRoleId == roleId));
            }
        }
    }
}
