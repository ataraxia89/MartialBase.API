// <copyright file="DeleteTests.cs" company="Martialtech®">
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
    public class DeleteTests : BaseTestClass
    {
        [Test]
        public async Task CanDeleteOrganisation()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            await OrganisationsRepository.DeleteAsync(testOrganisation.Id);

            Assert.IsTrue(await OrganisationsRepository.SaveChangesAsync());

            Assert.IsFalse(AddressResources.CheckExists(testOrganisation.Address.Id, DbIdentifier));
            Assert.IsFalse(OrganisationResources.CheckExists(testOrganisation.Id, DbIdentifier));
        }
    }
}
