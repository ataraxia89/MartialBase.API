// <copyright file="SchoolAddressExistsTests.cs" company="Martialtech®">
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
    public class SchoolAddressExistsTests : BaseTestClass
    {
        [Test]
        public async Task CanCheckSchoolAddressExists()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Address testAddress = AddressResources.CreateTestAddress(DbIdentifier);

            SchoolResources.EnsureSchoolHasAddress(testSchool.Id, testAddress.Id, DbIdentifier);

            Assert.IsTrue(await SchoolsRepository.SchoolAddressExistsAsync(testSchool.Id, testAddress.Id));
        }
    }
}
