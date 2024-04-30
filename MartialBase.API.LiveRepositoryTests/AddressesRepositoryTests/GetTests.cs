// <copyright file="GetTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.AddressesRepositoryTests
{
    public class GetTests : BaseTestClass
    {
        [Test]
        public async Task CanGetAddress()
        {
            Address address = AddressResources.CreateTestAddress(DbIdentifier);

            AddressDTO retrievedAddress = await AddressesRepository.GetAsync(address.Id);

            AddressResources.AssertEqual(address, retrievedAddress);
        }
    }
}
