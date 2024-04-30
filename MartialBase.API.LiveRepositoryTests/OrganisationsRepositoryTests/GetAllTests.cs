// <copyright file="GetAllTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.OrganisationsRepositoryTests
{
    public class GetAllTests : BaseTestClass
    {
        [Test]
        public async Task CanGetAllOrganisations()
        {
            int numberToCreate = 10;

            List<Organisation> testParentOrganisations =
                OrganisationResources.CreateTestOrganisations(numberToCreate, DbIdentifier);
            List<Organisation> testChildOrganisations =
                OrganisationResources.CreateTestOrganisations(numberToCreate, DbIdentifier);

            for (int i = 0; i < numberToCreate; i++)
            {
                OrganisationResources.SetOrganisationParent(
                    testChildOrganisations[i],
                    testParentOrganisations[i],
                    DbIdentifier);
            }

            var testOrganisations = new List<Organisation>(testParentOrganisations);
            testOrganisations.AddRange(testChildOrganisations);

            List<OrganisationDTO> retrievedOrganisations = await OrganisationsRepository.GetAllAsync();

            OrganisationResources.AssertEqual(testOrganisations, retrievedOrganisations);
        }
    }
}
