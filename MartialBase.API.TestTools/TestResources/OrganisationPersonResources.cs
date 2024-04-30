// <copyright file="OrganisationPersonResources.cs" company="Martialtech®">
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

namespace MartialBase.API.TestTools.TestResources
{
    internal static class OrganisationPersonResources
    {
        internal static (bool Exists, bool IsAdmin) CheckOrganisationHasPerson(Guid organisationId, Guid personId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                OrganisationPerson organisationPerson = dbContext.OrganisationPeople.FirstOrDefault(op =>
                    op.OrganisationId == organisationId && op.PersonId == personId);

                return (organisationPerson != null, organisationPerson?.IsOrganisationAdmin ?? false);
            }
        }

        internal static void EnsureOrganisationHasPeople(Guid organisationId, List<Guid> peopleIds, string dbIdentifier)
        {
            foreach (Guid personId in peopleIds)
            {
                EnsureOrganisationHasPerson(organisationId, personId, dbIdentifier);
            }
        }

        internal static void EnsureOrganisationDoesNotHavePeople(Guid organisationId, List<Guid> peopleIds, string dbIdentifier)
        {
            foreach (Guid personId in peopleIds)
            {
                EnsureOrganisationDoesNotHavePerson(organisationId, personId, dbIdentifier);
            }
        }

        internal static void EnsureOrganisationsHavePerson(List<Guid> organisationIds, Guid personId, string dbIdentifier, bool isAdmin = false)
        {
            foreach (Guid organisationId in organisationIds)
            {
                EnsureOrganisationHasPerson(organisationId, personId, dbIdentifier, isAdmin);
            }
        }

        internal static void EnsureOrganisationHasPerson(Guid organisationId, Guid personId, string dbIdentifier, bool isAdmin = false)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                OrganisationPerson organisationPerson = dbContext.OrganisationPeople
                    .FirstOrDefault(op =>
                        op.OrganisationId == organisationId &&
                        op.PersonId == personId);

                if (organisationPerson == null)
                {
                    dbContext.OrganisationPeople.Add(new OrganisationPerson
                    {
                        Id = Guid.NewGuid(),
                        OrganisationId = organisationId,
                        PersonId = personId,
                        IsOrganisationAdmin = isAdmin
                    });

                    Assert.True(dbContext.SaveChanges() > 0);
                }
                else if (organisationPerson.IsOrganisationAdmin != isAdmin)
                {
                    organisationPerson.IsOrganisationAdmin = isAdmin;

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.OrganisationPeople.Any(op =>
                    op.OrganisationId == organisationId &&
                    op.PersonId == personId &&
                    op.IsOrganisationAdmin == isAdmin));
            }
        }

        internal static void EnsureOrganisationsDoNotHavePerson(List<Guid> organisationIds, Guid personId, string dbIdentifier)
        {
            foreach (Guid organisationId in organisationIds)
            {
                EnsureOrganisationDoesNotHavePerson(organisationId, personId, dbIdentifier);
            }
        }

        internal static void EnsureOrganisationDoesNotHavePerson(Guid organisationId, Guid personId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext =
                DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                OrganisationPerson organisationPerson = dbContext.OrganisationPeople
                    .FirstOrDefault(op =>
                        op.OrganisationId == organisationId &&
                        op.PersonId == personId);

                if (organisationPerson != null)
                {
                    dbContext.OrganisationPeople.Remove(organisationPerson);

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext =
                DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                OrganisationPerson organisationPerson = dbContext.OrganisationPeople
                    .FirstOrDefault(op =>
                        op.OrganisationId == organisationId &&
                        op.PersonId == personId);

                Assert.Null(organisationPerson);
            }
        }

        internal static OrganisationPerson GetOrganisationPerson(Guid organisationId, Guid personId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext =
                DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.OrganisationPeople
                    .Include(op => op.Organisation)
                    .ThenInclude(o => o.Address)
                    .Include(op => op.Person)
                    .ThenInclude(p => p.Address)
                    .FirstOrDefault(op =>
                        op.OrganisationId == organisationId &&
                        op.PersonId == personId);
            }
        }
    }
}