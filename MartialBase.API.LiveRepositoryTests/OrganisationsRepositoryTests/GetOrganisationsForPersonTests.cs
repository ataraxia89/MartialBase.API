// <copyright file="GetOrganisationsForPersonTests.cs" company="Martialtech®">
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
    public class GetOrganisationsForPersonTests : BaseTestClass
    {
        [Test]
        public async Task CanGetPersonOrganisations()
        {
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);
            List<Organisation> personOrganisations =
                OrganisationResources.CreateTestOrganisations(10, DbIdentifier);

            // These organisations are created as additional system organisations to ensure that only those
            // assigned to the test person are returned
            OrganisationResources.CreateTestOrganisations(10, DbIdentifier);

            OrganisationPersonResources.EnsureOrganisationsHavePerson(
                personOrganisations.Select(o => o.Id).ToList(),
                testPerson.Id,
                DbIdentifier);

            List<PersonOrganisationDTO> retrievedPersonOrganisations =
                await OrganisationsRepository.GetOrganisationsForPersonAsync(testPerson.Id);

            OrganisationResources.AssertEqual(personOrganisations, retrievedPersonOrganisations);
        }
    }
}
