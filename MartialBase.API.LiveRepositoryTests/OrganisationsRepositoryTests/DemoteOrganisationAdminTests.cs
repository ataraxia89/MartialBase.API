// <copyright file="DemoteOrganisationAdminTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.OrganisationsRepositoryTests
{
    public class DemoteOrganisationAdminTests : BaseTestClass
    {
        [Test]
        public async Task CanDemoteOrganisationAdmin()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier,
                true);

            (bool Exists, bool IsAdmin) organisationPerson = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);

            Assert.IsTrue(organisationPerson.Exists);
            Assert.IsTrue(organisationPerson.IsAdmin);

            await OrganisationsRepository.DemoteOrganisationAdminAsync(testOrganisation.Id, testPerson.Id);

            Assert.IsTrue(await OrganisationsRepository.SaveChangesAsync());

            organisationPerson = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);

            Assert.IsTrue(organisationPerson.Exists);
            Assert.IsFalse(organisationPerson.IsAdmin);
        }
    }
}
