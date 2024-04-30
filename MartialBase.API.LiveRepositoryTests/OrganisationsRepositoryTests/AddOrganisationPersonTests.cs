// <copyright file="AddOrganisationPersonTests.cs" company="Martialtech®">
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
    public class AddOrganisationPersonTests : BaseTestClass
    {
        [Test]
        public async Task CanSetExistingOrganisationPersonAsAdmin()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);

            (bool Exists, bool IsAdmin) organisationPerson = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);

            Assert.IsTrue(organisationPerson.Exists);
            Assert.IsFalse(organisationPerson.IsAdmin);

            await OrganisationsRepository.AddOrganisationPersonAsync(testOrganisation.Id, testPerson.Id, true);

            Assert.IsTrue(await OrganisationsRepository.SaveChangesAsync());

            organisationPerson = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);

            Assert.IsTrue(organisationPerson.Exists);
            Assert.IsTrue(organisationPerson.IsAdmin);
        }

        [Test]
        public async Task CanAddNewOrganisationPersonAsMember()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            (bool Exists, bool IsAdmin) organisationPerson = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);

            Assert.IsFalse(organisationPerson.Exists);

            OrganisationsRepository.AddOrganisationPersonAsync(testOrganisation.Id, testPerson.Id, false);

            Assert.IsTrue(await OrganisationsRepository.SaveChangesAsync());

            organisationPerson = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);

            Assert.IsTrue(organisationPerson.Exists);
            Assert.IsFalse(organisationPerson.IsAdmin);
        }

        [Test]
        public async Task CanAddNewOrganisationPersonAsAdmin()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            (bool Exists, bool IsAdmin) organisationPerson = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);

            Assert.IsFalse(organisationPerson.Exists);

            OrganisationsRepository.AddOrganisationPersonAsync(testOrganisation.Id, testPerson.Id, true);

            Assert.IsTrue(await OrganisationsRepository.SaveChangesAsync());

            organisationPerson = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);

            Assert.IsTrue(organisationPerson.Exists);
            Assert.IsTrue(organisationPerson.IsAdmin);
        }
    }
}
