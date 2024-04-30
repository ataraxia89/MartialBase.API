// <copyright file="UpdateTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Schools;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.SchoolsRepositoryTests
{
    public class UpdateTests : BaseTestClass
    {
        [Test]
        public async Task CanUpdateSchool()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            UpdateSchoolDTO updateSchoolDTO = DataGenerator.Schools.GenerateUpdateSchoolDTO();

            SchoolResources.AssertNotEqual(testSchool, updateSchoolDTO);

            SchoolDTO updatedSchool = await SchoolsRepository.UpdateAsync(testSchool.Id, updateSchoolDTO);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            SchoolResources.AssertEqual(updateSchoolDTO, updatedSchool);
            SchoolResources.AssertExists(updatedSchool, DbIdentifier);
        }

        [Test]
        public async Task CanUpdateSchoolAndRemoveAdditionalTrainingVenues()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            List<Address> testAddresses = AddressResources.CreateTestAddresses(10, DbIdentifier);

            SchoolResources.EnsureSchoolHasAddresses(
                testSchool.Id,
                testAddresses.Select(p => p.Id).ToList(),
                DbIdentifier);

            UpdateSchoolDTO updateSchoolDTO = DataGenerator.Schools.GenerateUpdateSchoolDTO();

            updateSchoolDTO.AdditionalTrainingVenues.Clear();

            SchoolDTO updatedSchool = await SchoolsRepository.UpdateAsync(testSchool.Id, updateSchoolDTO);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            SchoolResources.AssertEqual(updateSchoolDTO, updatedSchool);
            SchoolResources.AssertExists(updatedSchool, DbIdentifier);
            AddressResources.AssertDoNotExist(
                testAddresses.Select(a => a.Id).ToList(),
                DbIdentifier);
        }
    }
}
