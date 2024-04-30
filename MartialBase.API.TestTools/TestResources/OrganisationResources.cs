// <copyright file="OrganisationResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using MartialBase.API.Data;
using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Organisations;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Tools;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class OrganisationResources
    {
        internal static bool CheckExists(Guid organisationId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.Organisations.Any(o => o.Id == organisationId);
            }
        }

        internal static void AssertDoesNotExist(Guid organisationId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.False(dbContext.Organisations.Any(o => o.Id == organisationId));
            }
        }

        internal static void AssertExists(OrganisationDTO organisation, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Organisation checkOrganisation = dbContext.Organisations
                    .Include(o => o.Address)
                    .Include(o => o.Parent)
                    .ThenInclude(o => o.Address)
                    .FirstOrDefault(o => o.Id == new Guid(organisation.Id));
                AssertEqual(organisation, checkOrganisation);
            }
        }

        internal static Organisation CreateTestOrganisation(
            string dbIdentifier,
            Guid? parentId = null,
            bool realisticData = false) => CreateTestOrganisations(1, dbIdentifier, parentId, realisticData).First();

        internal static List<Organisation> CreateTestOrganisations(int numberToCreate, string dbIdentifier, Guid? parentId = null, bool realisticData = false)
        {
            if (numberToCreate <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberToCreate),
                    numberToCreate,
                    $"Cannot create {numberToCreate} test organisations.");
            }

            var createdOrganisations = new List<Organisation>();
            Organisation parent = null;

            if (parentId != null)
            {
                using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
                {
                    parent = dbContext.Organisations
                        .Include(o => o.Address)
                        .FirstOrDefault(o => o.Id == parentId);
                    Assert.NotNull(parent);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                for (int i = 0; i < numberToCreate; i++)
                {
                    Organisation organisation = DataGenerator.Organisations.GenerateOrganisationObject(parentId?.ToString(), realisticData);

                    Assert.False(dbContext.Organisations.Any(o => o.Id == organisation.Id));

                    dbContext.Organisations.Add(organisation);
                    createdOrganisations.Add(organisation);
                }

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                foreach (Organisation createdOrganisation in createdOrganisations)
                {
                    // This must be assigned here and not above as the parent already exists, so if it's added
                    // to the organisation object before saving to the context, it will throw a duplicate key
                    // exception. It must be added because of the AssertEqual below.
                    createdOrganisation.Parent = parent;

                    Organisation checkOrganisation = dbContext.Organisations
                        .Include(o => o.Address)
                        .Include(o => o.Parent)
                        .ThenInclude(op => op.Address)
                        .FirstOrDefault(o => o.Id == createdOrganisation.Id);
                    AssertEqual(createdOrganisation, checkOrganisation);
                }
            }

            return createdOrganisations;
        }

        internal static void AssertOrganisationsBelongToParent(List<Guid> organisationIds, Guid parentOrganisationId, string dbIdentifier)
        {
            foreach (Guid organisationId in organisationIds)
            {
                Assert.True(CheckOrganisationBelongsToParent(
                    organisationId, parentOrganisationId, dbIdentifier));
            }
        }

        internal static bool CheckOrganisationBelongsToParent(Guid organisationId, Guid parentOrganisationId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.Organisations.Any(o =>
                    o.Id == organisationId && o.ParentId == parentOrganisationId);
            }
        }

        internal static void SetOrganisationParent(Organisation childOrganisation, Organisation parentOrganisation, string dbIdentifier, bool updateInMemoryObject = true)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.Organisations.Any(o => o.Id == childOrganisation.Id));
                Assert.True(dbContext.Organisations.Any(o => o.Id == parentOrganisation.Id));

                Organisation dbOrganisation = dbContext.Organisations.First(o => o.Id == childOrganisation.Id);

                if (dbOrganisation.ParentId != parentOrganisation.Id)
                {
                    dbOrganisation.ParentId = parentOrganisation.Id;

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.Organisations.Any(o =>
                    o.Id == childOrganisation.Id && o.ParentId == parentOrganisation.Id));
            }

            if (updateInMemoryObject)
            {
                childOrganisation.ParentId = parentOrganisation.Id;
                childOrganisation.Parent = parentOrganisation;
            }
        }

        internal static void SetOrganisationPublicStatus(Organisation organisation, bool isPublic, string dbIdentifier, bool updateInMemoryObject = true)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.Organisations.Any(o => o.Id == organisation.Id));

                Organisation dbOrganisation = dbContext.Organisations.First(o => o.Id == organisation.Id);

                if (dbOrganisation.IsPublic != isPublic)
                {
                    dbOrganisation.IsPublic = isPublic;

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.Organisations.Any(o =>
                    o.Id == organisation.Id && o.IsPublic == isPublic));
            }

            if (updateInMemoryObject)
            {
                organisation.IsPublic = isPublic;
            }
        }

        internal static Organisation GetOrganisation(Guid organisationId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.Organisations
                    .Include(o => o.Address)
                    .Include(o => o.Parent)
                    .FirstOrDefault(o => o.Id == organisationId);
            }
        }

        internal static Organisation DuplicateOrganisationObject(Organisation organisation) => new ()
        {
            Id = organisation.Id,
            Initials = organisation.Initials,
            Name = organisation.Name,
            ParentId = organisation.ParentId,
            Parent = organisation.Parent == null ? null : DuplicateOrganisationObject(organisation.Parent),
            AddressId = organisation.AddressId,
            Address = organisation.Address == null ? null : AddressResources.DuplicateAddressObject(organisation.Address),
            IsPublic = organisation.IsPublic
        };

        internal static void AssertEqual(Organisation expected, Organisation actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Initials, actual.Initials);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.ParentId, actual.ParentId);
            Assert.Equal(expected.AddressId, actual.AddressId);
            Assert.Equal(expected.IsPublic, actual.IsPublic);

            if (expected.Parent != null)
            {
                AssertEqual(expected.Parent, actual.Parent);
            }
            else
            {
                Assert.Null(actual.Parent);
            }

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqual(OrganisationDTO expected, Organisation actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(new Guid(expected.Id), actual.Id);
            Assert.Equal(expected.Initials, actual.Initials);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.ParentOrganisationId, actual.ParentId?.ToString());
            Assert.Equal(expected.ParentOrganisationInitials, actual.Parent?.Initials);

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqual(Organisation expected, OrganisationDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id.ToString(), actual.Id);
            Assert.Equal(expected.Initials, actual.Initials);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.ParentId?.ToString(), actual.ParentOrganisationId);

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }

            if (expected.Parent != null)
            {
                Assert.Equal(expected.Parent.Initials, actual.ParentOrganisationInitials);
            }
            else
            {
                Assert.Null(actual.ParentOrganisationInitials);
            }
        }

        internal static void AssertEqual(OrganisationDTO expected, OrganisationDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Initials, actual.Initials);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.ParentOrganisationId, actual.ParentOrganisationId);
            Assert.Equal(expected.ParentOrganisationInitials, actual.ParentOrganisationInitials);

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqual(CreateOrganisationDTO expected, OrganisationDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Initials, actual.Initials);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.ParentId, actual.ParentOrganisationId);

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqual(CreateOrganisationInternalDTO expected, OrganisationDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Initials, actual.Initials);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.ParentId?.ToString(), actual.ParentOrganisationId);

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqual(CreateOrganisationDTO expected, Organisation actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Initials, actual.Initials);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Address.Line1, actual.Address.Line1);
            Assert.Equal(expected.Address.Line2, actual.Address.Line2);
            Assert.Equal(expected.Address.Line3, actual.Address.Line3);
            Assert.Equal(expected.Address.Town, actual.Address.Town);
            Assert.Equal(expected.Address.County, actual.Address.County);
            Assert.Equal(expected.Address.PostCode, actual.Address.PostCode);
            Assert.Equal(expected.Address.CountryCode, actual.Address.CountryCode);
            Assert.Equal(expected.Address.LandlinePhone, actual.Address.LandlinePhone);
            Assert.Equal(expected.ParentId, actual.ParentId?.ToString());
        }

        internal static void AssertEqual(UpdateOrganisationDTO expected, OrganisationDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Initials, actual.Initials);
            Assert.Equal(expected.Name, actual.Name);
        }

        internal static void AssertEqual(UpdateOrganisationDTO expected, Organisation actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Initials, actual.Initials);
            Assert.Equal(expected.Name, actual.Name);
        }

        internal static void AssertEqual(List<Organisation> expected, List<OrganisationDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (Organisation expectedOrganisation in expected)
            {
                OrganisationDTO actualOrganisation = actual
                    .FirstOrDefault(o => o.Id == expectedOrganisation.Id.ToString());

                AssertEqual(expectedOrganisation, actualOrganisation);
            }
        }

        internal static void AssertEqual(List<OrganisationDTO> expected, List<OrganisationDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (OrganisationDTO expectedOrganisation in expected)
            {
                OrganisationDTO actualOrganisation = actual
                    .FirstOrDefault(o => o.Id == expectedOrganisation.Id);

                AssertEqual(expectedOrganisation, actualOrganisation);
            }
        }

        internal static void AssertEqual(List<Organisation> expected, List<PersonOrganisationDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (Organisation expectedOrganisation in expected)
            {
                OrganisationDTO actualOrganisation = actual.FirstOrDefault(po =>
                    po.Organisation.Id == expectedOrganisation.Id.ToString())?.Organisation;

                // When GetOrganisationsForPerson is called, it won't return the organisation address or parent,
                // so the organisation object found on system will need to have it's address and parent set as
                // null so that the AssertEqual below doesn't attempt to check them
                expectedOrganisation.AddressId = null;
                expectedOrganisation.Address = null;
                expectedOrganisation.ParentId = null;
                expectedOrganisation.Parent = null;

                AssertEqual(expectedOrganisation, actualOrganisation);
            }
        }

        internal static void AssertNotEqual(Organisation expected, Organisation actual)
        {
            Assert.NotNull(actual);
            Assert.NotEqual(expected.Id, actual.Id);
            Assert.NotEqual(expected.Initials, actual.Initials);
            Assert.NotEqual(expected.Name, actual.Name);
            Assert.NotEqual(expected.AddressId, actual.AddressId);

            if (expected.ParentId != null && actual.ParentId != null)
            {
                Assert.NotEqual(expected.ParentId, actual.ParentId);
            }

            if (expected.Parent != null && actual.Parent != null)
            {
                AssertNotEqual(expected.Parent, actual.Parent);
            }

            if (expected.Address != null && actual.Address != null)
            {
                AddressResources.AssertNotEqual(expected.Address, actual.Address);
            }
        }

        internal static void AssertNotEqual(Organisation expected, UpdateOrganisationDTO actual)
        {
            Assert.NotEqual(expected.Initials, actual.Initials);
            Assert.NotEqual(expected.Name, actual.Name);
        }
    }
}
