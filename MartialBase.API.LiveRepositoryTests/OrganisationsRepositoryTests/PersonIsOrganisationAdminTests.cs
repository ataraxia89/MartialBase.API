// <copyright file="PersonIsOrganisationAdminTests.cs" company="Martialtech®">
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
    public class PersonIsOrganisationAdminTests : BaseTestClass
    {
        [Test]
        public async Task CanCheckOrganisationPersonAdmin()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testOrganisation.Id,
                testPerson.Id,
                DbIdentifier,
                true);

            Assert.IsTrue(await OrganisationsRepository.PersonIsOrganisationAdminAsync(
                testOrganisation.Id, testPerson.Id));
        }
    }
}
