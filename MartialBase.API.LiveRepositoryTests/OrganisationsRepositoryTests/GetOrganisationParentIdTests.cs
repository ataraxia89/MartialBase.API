// <copyright file="GetOrganisationParentIdTests.cs" company="Martialtech®">
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
    public class GetOrganisationParentIdTests : BaseTestClass
    {
        [Test]
        public async Task CanGetOrganisationParentId()
        {
            Organisation parentOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation childOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier, parentOrganisation.Id);

            Assert.IsTrue(OrganisationResources.CheckOrganisationBelongsToParent(
                childOrganisation.Id, parentOrganisation.Id, DbIdentifier));

            Assert.AreEqual(
                parentOrganisation.Id,
                await OrganisationsRepository.GetOrganisationParentIdAsync(childOrganisation.Id));
        }
    }
}
