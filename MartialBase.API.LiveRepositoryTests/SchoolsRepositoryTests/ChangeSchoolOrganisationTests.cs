// <copyright file="ChangeSchoolOrganisationTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.SchoolsRepositoryTests
{
    public class ChangeSchoolOrganisationTests : BaseTestClass
    {
        [Test]
        public async Task CanChangeSchoolOrganisation()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Organisation newOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            SchoolResources.EnsureSchoolBelongsToOrganisation(
                testSchool, testOrganisation, DbIdentifier);

            await SchoolsRepository.ChangeSchoolOrganisationAsync(testSchool.Id, newOrganisation.Id);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            Assert.IsTrue(SchoolResources.CheckSchoolBelongsToOrganisation(
                testSchool.Id, newOrganisation.Id, DbIdentifier));
        }
    }
}
