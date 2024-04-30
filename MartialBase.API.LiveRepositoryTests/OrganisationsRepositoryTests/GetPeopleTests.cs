// <copyright file="GetPeopleTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.OrganisationsRepositoryTests
{
    public class GetPeopleTests : BaseTestClass
    {
        [Test]
        public async Task CanGetOrganisationPeople()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationHasPeople(
                testOrganisation.Id,
                testPeople.Select(p => p.Id).ToList(),
                DbIdentifier);

            List<OrganisationPersonDTO> retrievedOrganisationPeople =
                await OrganisationsRepository.GetPeopleAsync(testOrganisation.Id);

            PersonResources.AssertEqual(testPeople, retrievedOrganisationPeople);
        }
    }
}
