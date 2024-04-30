// <copyright file="RemoveOrganisationPersonTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.OrganisationsRepositoryTests
{
    public class RemoveOrganisationPersonTests : BaseTestClass
    {
        [Test]
        public async Task CanRemoveOrganisationPersonWhenPersonIsMemberOfMultipleOrganisations()
        {
            List<Organisation> testOrganisations =
                OrganisationResources.CreateTestOrganisations(10, DbIdentifier);
            Organisation testRemoveOrganisation = testOrganisations[0];
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            OrganisationPersonResources.EnsureOrganisationsHavePerson(
                testOrganisations.Select(o => o.Id).ToList(),
                testPerson.Id,
                DbIdentifier);

            await OrganisationsRepository.RemoveOrganisationPersonAsync(testRemoveOrganisation.Id, testPerson.Id);

            Assert.IsTrue(await OrganisationsRepository.SaveChangesAsync());

            (bool Exists, bool IsAdmin) organisationPerson = OrganisationPersonResources.CheckOrganisationHasPerson(
                testRemoveOrganisation.Id,
                testPerson.Id,
                DbIdentifier);

            Assert.IsFalse(organisationPerson.Exists);

            // Check remaining organisations with the exception of the first one (index 0)
            for (int i = 1; i < testOrganisations.Count; i++)
            {
                organisationPerson = OrganisationPersonResources.CheckOrganisationHasPerson(
                    testOrganisations[i].Id,
                    testPerson.Id,
                    DbIdentifier);

                Assert.IsTrue(organisationPerson.Exists);
            }
        }

        [Test]
        public async Task RemoveSingleOrganisationPersonThrowsOrphanEntityException()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);

            (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);

            Assert.IsTrue(personExists);

            Assert.That(
                async () => await OrganisationsRepository.RemoveOrganisationPersonAsync(testOrganisation.Id, testPerson.Id),
                Throws.Exception.TypeOf<OrphanPersonEntityException>());
        }
    }
}
