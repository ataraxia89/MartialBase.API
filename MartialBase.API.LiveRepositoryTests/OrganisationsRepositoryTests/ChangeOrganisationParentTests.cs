// <copyright file="ChangeOrganisationParentTests.cs" company="Martialtech®">
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
    public class ChangeOrganisationParentTests : BaseTestClass
    {
        [Test]
        public async Task CanChangeOrganisationParent()
        {
            Organisation testParentOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation testOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier, testParentOrganisation.Id);
            Organisation newParentOrganisation =
                OrganisationResources.CreateTestOrganisation(DbIdentifier);

            OrganisationResources.AssertNotEqual(testParentOrganisation, newParentOrganisation);

            Assert.IsTrue(OrganisationResources.CheckOrganisationBelongsToParent(
                testOrganisation.Id, testParentOrganisation.Id, DbIdentifier));
            Assert.IsFalse(OrganisationResources.CheckOrganisationBelongsToParent(
                testOrganisation.Id, newParentOrganisation.Id, DbIdentifier));

            await OrganisationsRepository.ChangeOrganisationParentAsync(testOrganisation.Id, newParentOrganisation.Id);

            Assert.IsTrue(await OrganisationsRepository.SaveChangesAsync());

            Assert.IsFalse(OrganisationResources.CheckOrganisationBelongsToParent(
                testOrganisation.Id, testParentOrganisation.Id, DbIdentifier));
            Assert.IsTrue(OrganisationResources.CheckOrganisationBelongsToParent(
                testOrganisation.Id, newParentOrganisation.Id, DbIdentifier));
        }
    }
}
