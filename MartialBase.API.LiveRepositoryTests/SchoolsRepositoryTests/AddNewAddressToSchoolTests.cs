// <copyright file="AddNewAddressToSchoolTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.SchoolsRepositoryTests
{
    public class AddNewAddressToSchoolTests : BaseTestClass
    {
        [Test]
        public async Task CanAddNewAddressToSchool()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            Assert.AreEqual(1, testSchool.SchoolAddresses.Count);

            CreateAddressDTO createAddressDTO = DataGenerator.Addresses.GenerateCreateAddressDTO();

            AddressDTO createdAddress = await SchoolsRepository.AddNewAddressToSchoolAsync(testSchool.Id, createAddressDTO);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            AddressResources.AssertEqual(createAddressDTO, createdAddress);
            AddressResources.AssertExists(createdAddress, DbIdentifier);
            Assert.IsTrue(
                SchoolResources.CheckSchoolHasAddress(
                    testSchool.Id, new Guid(createdAddress.Id), DbIdentifier));
        }

        [Test]
        public async Task AddDuplicateAddressToSchoolThrowsEntityAlreadyExistsException()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Address testAddress = AddressResources.CreateTestAddress(DbIdentifier);

            SchoolResources.EnsureSchoolHasAddress(testSchool.Id, testAddress.Id, DbIdentifier);

            var duplicateAddress = new CreateAddressDTO
            {
                Line1 = testAddress.Line1,
                Line2 = testAddress.Line2,
                Line3 = testAddress.Line3,
                Town = testAddress.Town,
                County = testAddress.County,
                PostCode = testAddress.PostCode,
                CountryCode = testAddress.CountryCode,
                LandlinePhone = testAddress.LandlinePhone
            };

            Assert.That(
                async () => await SchoolsRepository.AddNewAddressToSchoolAsync(testSchool.Id, duplicateAddress),
                Throws.Exception.TypeOf<EntityAlreadyExistsException>());
        }
    }
}
