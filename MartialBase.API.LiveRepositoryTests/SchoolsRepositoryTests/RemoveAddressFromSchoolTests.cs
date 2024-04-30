﻿// <copyright file="RemoveAddressFromSchoolTests.cs" company="Martialtech®">
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
    public class RemoveAddressFromSchoolTests : BaseTestClass
    {
        [Test]
        public async Task CanRemoveAddressFromSchool()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Address testAddress = AddressResources.CreateTestAddress(DbIdentifier);

            SchoolResources.EnsureSchoolHasAddress(testSchool.Id, testAddress.Id, DbIdentifier);

            await SchoolsRepository.RemoveAddressFromSchoolAsync(testSchool.Id, testAddress.Id);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            Assert.IsFalse(SchoolResources.CheckSchoolHasAddress(
                testSchool.Id, testAddress.Id, DbIdentifier));
            Assert.IsFalse(AddressResources.CheckExists(testAddress.Id, DbIdentifier));
        }
    }
}
