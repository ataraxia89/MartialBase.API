// <copyright file="GetTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Schools;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.SchoolsRepositoryTests
{
    public class GetTests : BaseTestClass
    {
        [Test]
        public async Task CanRetrieveSingleSchool()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            SchoolDTO retrievedSchool = await SchoolsRepository.GetAsync(testSchool.Id);

            SchoolResources.AssertEqual(testSchool, retrievedSchool);
        }
    }
}
