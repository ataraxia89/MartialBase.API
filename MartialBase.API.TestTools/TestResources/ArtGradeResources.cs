// <copyright file="ArtGradeResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.ArtGrades;
using MartialBase.API.Models.DTOs.ArtGrades;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class ArtGradeResources
    {
        internal static bool CheckExists(Guid artGradeId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.ArtGrades.Any(ag => ag.Id == artGradeId);
            }
        }

        internal static void AssertExists(ArtGradeDTO artGrade, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                ArtGrade checkArtGrade = dbContext.ArtGrades
                    .Include(ag => ag.Art)
                    .Include(ag => ag.Organisation)
                    .FirstOrDefault(ag => ag.Id == new Guid(artGrade.Id));
                AssertEqual(artGrade, checkArtGrade);
            }
        }

        internal static void AssertDoesNotExist(Guid artGradeId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.False(dbContext.ArtGrades.Any(ag => ag.Id == artGradeId));
            }
        }

        internal static void EnsureArtHasArtGrades(Art art, List<ArtGrade> artGrades, string dbIdentifier, bool updateInMemoryObjects = true)
        {
            foreach (ArtGrade artGrade in artGrades)
            {
                EnsureArtHasArtGrades(art, artGrade, dbIdentifier, updateInMemoryObjects);
            }
        }

        internal static void EnsureArtHasArtGrades(Art art, ArtGrade artGrade, string dbIdentifier, bool updateInMemoryObject = true)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                ArtGrade dbArtGrade = dbContext.ArtGrades
                    .First(ag => ag.Id == artGrade.Id);

                if (dbArtGrade.ArtId != art.Id)
                {
                    dbArtGrade.ArtId = art.Id;

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.ArtGrades.Any(ag =>
                    ag.Id == artGrade.Id &&
                    ag.ArtId == art.Id));
            }

            if (updateInMemoryObject)
            {
                artGrade.ArtId = art.Id;
                artGrade.Art = art;
            }
        }

        internal static ArtGrade CreateTestArtGrade(
            string dbIdentifier,
            Guid? artId = null,
            Guid? organisationId = null,
            bool realisticData = false) =>
            CreateTestArtGrades(1, dbIdentifier, artId, organisationId, realisticData).First();

        internal static List<ArtGrade> CreateTestArtGrades(int numberToCreate, string dbIdentifier, Guid? artId = null, Guid? organisationId = null, bool realisticData = false)
        {
            if (numberToCreate <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberToCreate),
                    numberToCreate,
                    $"Cannot create {numberToCreate} test art grades.");
            }

            var createdArtGrades = new List<ArtGrade>();

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Art art = artId != null ?
                    dbContext.Arts.FirstOrDefault(a => a.Id == artId) :
                    null;

                Organisation organisation = organisationId != null
                    ? dbContext.Organisations
                        .Include(o => o.Address)
                        .FirstOrDefault(o => o.Id == organisationId)
                    : null;

                for (int i = 0; i < numberToCreate; i++)
                {
                    ArtGrade artGrade = DataGenerator.ArtGrades.GenerateArtGradeObject(art, organisation, realisticData);

                    Assert.False(dbContext.ArtGrades.Any(ag => ag.Id == artGrade.Id));

                    dbContext.ArtGrades.Add(artGrade);
                    createdArtGrades.Add(artGrade);
                }

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                foreach (ArtGrade createdArtGrade in createdArtGrades)
                {
                    ArtGrade checkArtGrade = dbContext.ArtGrades
                        .Include(ag => ag.Art)
                        .Include(ag => ag.Organisation)
                        .ThenInclude(o => o.Address)
                        .Include(ag => ag.Organisation.Parent)
                        .ThenInclude(op => op.Address)
                        .FirstOrDefault(ag => ag.Id == createdArtGrade.Id);

                    AssertEqual(createdArtGrade, checkArtGrade);
                }
            }

            return createdArtGrades;
        }

        internal static ArtGrade GetArtGrade(Guid artGradeId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.ArtGrades
                    .Include(ag => ag.Art)
                    .Include(ag => ag.Organisation)
                    .ThenInclude(o => o.Address)
                    .Include(ag => ag.Organisation.Parent)
                    .ThenInclude(o => o.Address)
                    .FirstOrDefault(ag => ag.Id == artGradeId);
            }
        }

        internal static void AssertEqual(ArtGrade expected, ArtGrade actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.ArtId, actual.ArtId);
            ArtResources.AssertEqual(expected.Art, actual.Art);
            Assert.Equal(expected.OrganisationId, actual.OrganisationId);
            OrganisationResources.AssertEqual(expected.Organisation, actual.Organisation);
            Assert.Equal(expected.GradeLevel, actual.GradeLevel);
            Assert.Equal(expected.Description, actual.Description);
        }

        internal static void AssertEqual(ArtGradeDTO expected, ArtGrade actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(new Guid(expected.Id), actual.Id);
            Assert.Equal(new Guid(expected.ArtId), actual.ArtId);
            Assert.Equal(expected.Art, actual.Art.Name);
            Assert.Equal(new Guid(expected.OrganisationId), actual.OrganisationId);
            Assert.Equal(expected.Organisation, actual.Organisation.Initials);
            Assert.Equal(expected.GradeLevel, actual.GradeLevel);
            Assert.Equal(expected.Description, actual.Description);
        }

        internal static void AssertEqual(ArtGrade expected, ArtGradeDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id.ToString(), actual.Id);
            Assert.Equal(expected.ArtId.ToString(), actual.ArtId);
            Assert.Equal(expected.Art.Name, actual.Art);
            Assert.Equal(expected.OrganisationId.ToString(), actual.OrganisationId);
            Assert.Equal(expected.Organisation.Initials, actual.Organisation);
            Assert.Equal(expected.GradeLevel, actual.GradeLevel);
            Assert.Equal(expected.Description, actual.Description);
        }

        internal static void AssertEqual(ArtGradeDTO expected, ArtGradeDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.ArtId, actual.ArtId);
            Assert.Equal(expected.Art, actual.Art);
            Assert.Equal(expected.OrganisationId, actual.OrganisationId);
            Assert.Equal(expected.Organisation, actual.Organisation);
            Assert.Equal(expected.GradeLevel, actual.GradeLevel);
            Assert.Equal(expected.Description, actual.Description);
        }

        internal static void AssertEqual(CreateArtGradeDTO expected, ArtGradeDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.ArtId, actual.ArtId);
            Assert.Equal(expected.OrganisationId, actual.OrganisationId);
            Assert.Equal(expected.GradeLevel, actual.GradeLevel);
            Assert.Equal(expected.Description, actual.Description);
        }

        internal static void AssertEqual(CreateArtGradeInternalDTO expected, ArtGradeDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.ArtId.ToString(), actual.ArtId);
            Assert.Equal(expected.OrganisationId.ToString(), actual.OrganisationId);
            Assert.Equal(expected.GradeLevel, actual.GradeLevel);
            Assert.Equal(expected.Description, actual.Description);
        }

        internal static void AssertEqual(UpdateArtGradeDTO expected, ArtGradeDTO actual)
        {
            Assert.Equal(expected.GradeLevel, actual.GradeLevel);
            Assert.Equal(expected.Description, actual.Description);
        }

        internal static void AssertEqual(List<ArtGrade> expected, List<ArtGradeDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (ArtGrade expectedArtGrade in expected)
            {
                ArtGradeDTO actualArtGrade = actual.FirstOrDefault(dt =>
                    dt.Id == expectedArtGrade.Id.ToString());

                AssertEqual(expectedArtGrade, actualArtGrade);
            }
        }

        internal static void AssertEqual(List<ArtGradeDTO> expected, List<ArtGradeDTO> actual)
        {
            foreach (ArtGradeDTO expectedArtGrade in expected)
            {
                ArtGradeDTO actualArtGrade = actual.FirstOrDefault(dt =>
                    dt.Id == expectedArtGrade.Id);

                AssertEqual(expectedArtGrade, actualArtGrade);
            }
        }

        internal static void AssertNotEqual(ArtGrade expected, UpdateArtGradeDTO actual)
        {
            Assert.NotEqual(expected.GradeLevel, actual.GradeLevel);
            Assert.NotEqual(expected.Description, actual.Description);
        }
    }
}
