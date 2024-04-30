// <copyright file="GetTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.OrganisationsRepositoryTests
{
    public class GetTests : BaseTestClass
    {
        [Test]
        public async Task CanGetOrganisation()
        {
            Organisation testParentOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier, testParentOrganisation.Id);

            OrganisationDTO retrievedOrganisation = await OrganisationsRepository.GetAsync(testOrganisation.Id);

            OrganisationResources.AssertEqual(testOrganisation, retrievedOrganisation);
        }
    }
}
