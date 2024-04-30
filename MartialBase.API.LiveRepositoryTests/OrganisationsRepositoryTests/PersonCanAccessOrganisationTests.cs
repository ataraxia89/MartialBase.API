// <copyright file="PersonCanAccessOrganisationTests.cs" company="Martialtech®">
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
    public class PersonCanAccessOrganisationTests : BaseTestClass
    {
        [Test]
        public async Task CanCheckPrivateOrganisationPersonAccess()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);
            OrganisationResources.SetOrganisationPublicStatus(
                testOrganisation, false, DbIdentifier);

            Assert.IsTrue(await OrganisationsRepository.PersonCanAccessOrganisationAsync(
                testOrganisation.Id, testPerson.Id));
        }

        [Test]
        public async Task CanCheckPublicOrganisationPersonAccess()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            OrganisationPersonResources.EnsureOrganisationDoesNotHavePerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier);
            OrganisationResources.SetOrganisationPublicStatus(
                testOrganisation, true, DbIdentifier);

            Assert.IsTrue(await OrganisationsRepository.PersonCanAccessOrganisationAsync(
                testOrganisation.Id, testPerson.Id));
        }
    }
}
