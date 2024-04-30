// <copyright file="UpdateTests.cs" company="Martialtech®">
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
    public class UpdateTests : BaseTestClass
    {
        [Test]
        public async Task CanUpdateOrganisation()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            UpdateOrganisationDTO updateOrganisationDTO =
                DataGenerator.Organisations.GenerateUpdateOrganisationDTOObject();

            OrganisationResources.AssertNotEqual(testOrganisation, updateOrganisationDTO);

            OrganisationDTO updatedOrganisation =
                await OrganisationsRepository.UpdateAsync(testOrganisation.Id, updateOrganisationDTO);

            Assert.IsTrue(await OrganisationsRepository.SaveChangesAsync());

            OrganisationResources.AssertEqual(updateOrganisationDTO, updatedOrganisation);
            OrganisationResources.AssertExists(updatedOrganisation, DbIdentifier);
        }
    }
}
