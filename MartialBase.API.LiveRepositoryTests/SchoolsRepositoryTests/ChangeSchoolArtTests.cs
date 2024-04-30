// <copyright file="ChangeSchoolArtTests.cs" company="Martialtech®">
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
    public class ChangeSchoolArtTests : BaseTestClass
    {
        [Test]
        public async Task CanChangeSchoolArt()
        {
            Art testArt = ArtResources.CreateTestArt(DbIdentifier);
            Art newArt = ArtResources.CreateTestArt(DbIdentifier);
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            SchoolResources.EnsureSchoolBelongsToArt(testSchool, testArt, DbIdentifier);

            await SchoolsRepository.ChangeSchoolArtAsync(testSchool.Id, newArt.Id);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            Assert.IsTrue(SchoolResources.CheckSchoolBelongsToArt(
                testSchool.Id, newArt.Id, DbIdentifier));
        }
    }
}
