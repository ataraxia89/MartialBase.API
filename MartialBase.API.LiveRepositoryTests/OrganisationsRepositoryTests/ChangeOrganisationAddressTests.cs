// <copyright file="ChangeOrganisationAddressTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.OrganisationsRepositoryTests
{
    public class ChangeOrganisationAddressTests : BaseTestClass
    {
        [Test]
        public async Task CanChangeOrganisationAddress()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            CreateAddressDTO createAddressDTO = DataGenerator.Addresses.GenerateCreateAddressDTO();
            createAddressDTO.CountryCode =
                CountryResources.GetRandomCountry().Code;

            AddressResources.AssertNotEqual(testOrganisation.Address, createAddressDTO);

            AddressDTO updatedAddress =
                await OrganisationsRepository.ChangeOrganisationAddressAsync(testOrganisation.Id, createAddressDTO);

            Assert.IsTrue(await OrganisationsRepository.SaveChangesAsync());

            AddressResources.AssertEqual(createAddressDTO, updatedAddress);
            AddressResources.AssertExists(updatedAddress, DbIdentifier);
        }
    }
}
