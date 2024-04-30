// <copyright file="ArtResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Arts;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class ArtResources
    {
        internal static void AssertDoesNotExist(Guid artId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.False(dbContext.Arts.Any(a => a.Id == artId));
            }
        }

        internal static void AssertExist(List<ArtDTO> arts, string dbIdentifier)
        {
            foreach (ArtDTO art in arts)
            {
                AssertExists(art, dbIdentifier);
            }
        }

        internal static void AssertExists(ArtDTO art, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Art checkArt = dbContext.Arts.FirstOrDefault(a => a.Id == new Guid(art.Id));
                AssertEqual(art, checkArt);
            }
        }

        internal static Art CreateTestArt(string dbIdentifier) => CreateTestArts(1, dbIdentifier).First();

        internal static List<Art> CreateTestArts(int numberToCreate, string dbIdentifier)
        {
            if (numberToCreate <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberToCreate),
                    numberToCreate,
                    $"Cannot create {numberToCreate} test arts.");
            }

            var createdArts = new List<Art>();

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                for (int i = 0; i < numberToCreate; i++)
                {
                    Art art = DataGenerator.Arts.GenerateArtObject();

                    Assert.False(dbContext.Arts.Any(a => a.Id == art.Id));

                    dbContext.Arts.Add(art);
                    createdArts.Add(art);
                }

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                foreach (Art createdArt in createdArts)
                {
                    Art checkArt = dbContext.Arts.FirstOrDefault(a => a.Id == createdArt.Id);
                    AssertEqual(createdArt, checkArt);
                }
            }

            return createdArts;
        }

        internal static List<Art> GetAllArts(string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.Arts.ToList();
            }
        }

        internal static void AssertEqual(Art expected, Art actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
        }

        internal static void AssertEqual(ArtDTO expected, Art actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(new Guid(expected.Id), actual.Id);
            Assert.Equal(expected.Name, actual.Name);
        }

        internal static void AssertEqual(Art expected, ArtDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id.ToString(), actual.Id);
            Assert.Equal(expected.Name, actual.Name);
        }

        internal static void AssertEqual(ArtDTO expected, ArtDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
        }

        internal static void AssertEqual(List<Art> expected, List<ArtDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (Art expectedArt in expected)
            {
                ArtDTO actualArt = actual.FirstOrDefault(a =>
                    a.Id == expectedArt.Id.ToString());

                AssertEqual(expectedArt, actualArt);
            }
        }

        internal static void AssertEqual(List<ArtDTO> expected, List<ArtDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (ArtDTO expectedArt in expected)
            {
                ArtDTO actualArt = actual.FirstOrDefault(a =>
                    a.Id == expectedArt.Id);

                AssertEqual(expectedArt, actualArt);
            }
        }
    }
}
