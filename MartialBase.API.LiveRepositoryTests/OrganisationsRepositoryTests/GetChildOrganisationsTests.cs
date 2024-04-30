// <copyright file="GetChildOrganisationsTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.OrganisationsRepositoryTests
{
    public class GetChildOrganisationsTests : BaseTestClass
    {
        [Test]
        public async Task CanGetChildOrganisations()
        {
            Organisation parentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            List<Organisation> childOrganisations =
                OrganisationResources.CreateTestOrganisations(10, DbIdentifier, parentOrganisation.Id);

            OrganisationResources.AssertOrganisationsBelongToParent(
                childOrganisations.Select(o => o.Id).ToList(),
                parentOrganisation.Id,
                DbIdentifier);

            List<OrganisationDTO> retrievedOrganisations =
                await OrganisationsRepository.GetChildOrganisationsAsync(parentOrganisation.Id);

            OrganisationResources.AssertEqual(childOrganisations, retrievedOrganisations);
        }
    }
}
