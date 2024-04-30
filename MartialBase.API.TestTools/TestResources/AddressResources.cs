// <copyright file="AddressResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Addresses;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class AddressResources
    {
        internal static bool CheckExists(Guid addressId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.Addresses.Any(a => a.Id == addressId);
            }
        }

        internal static bool CanFindByDetailsOnly(Address address, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.Addresses.Any(a =>
                    a.Line1 == address.Line1 &&
                    a.Line2 == address.Line2 &&
                    a.Line3 == address.Line3 &&
                    a.Town == address.Town &&
                    a.County == address.County &&
                    a.PostCode == address.PostCode &&
                    a.CountryCode == address.CountryCode &&
                    a.LandlinePhone == address.LandlinePhone);
            }
        }

        internal static void AssertDoNotExist(List<Guid> addressIds, string dbIdentifier)
        {
            foreach (Guid addressId in addressIds)
            {
                Assert.False(CheckExists(addressId, dbIdentifier));
            }
        }

        internal static void AssertExists(AddressDTO address, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Address checkAddress = dbContext.Addresses
                    .FirstOrDefault(o => o.Id == new Guid(address.Id));
                AssertEqual(address, checkAddress);
            }
        }

        internal static Address CreateTestAddress(string dbIdentifier, bool realisticData = false) =>
            CreateTestAddresses(1, dbIdentifier, realisticData).First();

        internal static List<Address> CreateTestAddresses(int numberToCreate, string dbIdentifier, bool realisticData = false)
        {
            if (numberToCreate <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberToCreate),
                    numberToCreate,
                    $"Cannot create {numberToCreate} test addresses.");
            }

            var createdAddresses = new List<Address>();

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                for (int i = 0; i < numberToCreate; i++)
                {
                    Address address = DataGenerator.Addresses.GenerateAddressObject(realisticData);

                    Assert.False(dbContext.Addresses.Any(a => a.Id == address.Id));

                    dbContext.Addresses.Add(address);
                    createdAddresses.Add(address);
                }

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                foreach (Address createdAddress in createdAddresses)
                {
                    Address checkAddress = dbContext.Addresses
                        .FirstOrDefault(a => a.Id == createdAddress.Id);
                    AssertEqual(createdAddress, checkAddress);
                }
            }

            return createdAddresses;
        }

        internal static Address DuplicateAddressObject(Address address) => new ()
        {
            Id = address.Id,
            Line1 = address.Line1,
            Line2 = address.Line2,
            Line3 = address.Line3,
            Town = address.Town,
            County = address.County,
            PostCode = address.PostCode,
            CountryCode = address.CountryCode,
            LandlinePhone = address.LandlinePhone
        };

        internal static void AssertEqual(Address expected, Address actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Line1, actual.Line1);
            Assert.Equal(expected.Line2, actual.Line2);
            Assert.Equal(expected.Line3, actual.Line3);
            Assert.Equal(expected.Town, actual.Town);
            Assert.Equal(expected.County, actual.County);
            Assert.Equal(expected.PostCode, actual.PostCode);
            Assert.Equal(expected.LandlinePhone, actual.LandlinePhone);
        }

        internal static void AssertEqual(Address expected, AddressDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id.ToString(), actual.Id);
            Assert.Equal(expected.Line1, actual.Line1);
            Assert.Equal(expected.Line2, actual.Line2);
            Assert.Equal(expected.Line3, actual.Line3);
            Assert.Equal(expected.Town, actual.Town);
            Assert.Equal(expected.County, actual.County);
            Assert.Equal(expected.PostCode, actual.PostCode);
            Assert.Equal(expected.LandlinePhone, actual.LandlinePhone);
        }

        internal static void AssertEqual(AddressDTO expected, Address actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(new Guid(expected.Id), actual.Id);
            Assert.Equal(expected.Line1, actual.Line1);
            Assert.Equal(expected.Line2, actual.Line2);
            Assert.Equal(expected.Line3, actual.Line3);
            Assert.Equal(expected.Town, actual.Town);
            Assert.Equal(expected.County, actual.County);
            Assert.Equal(expected.PostCode, actual.PostCode);
            Assert.Equal(expected.LandlinePhone, actual.LandlinePhone);
        }

        internal static void AssertEqual(AddressDTO expected, AddressDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Line1, actual.Line1);
            Assert.Equal(expected.Line2, actual.Line2);
            Assert.Equal(expected.Line3, actual.Line3);
            Assert.Equal(expected.Town, actual.Town);
            Assert.Equal(expected.County, actual.County);
            Assert.Equal(expected.PostCode, actual.PostCode);
            Assert.Equal(expected.LandlinePhone, actual.LandlinePhone);
        }

        internal static void AssertEqual(CreateAddressDTO expected, AddressDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Line1, actual.Line1);
            Assert.Equal(expected.Line2, actual.Line2);
            Assert.Equal(expected.Line3, actual.Line3);
            Assert.Equal(expected.Town, actual.Town);
            Assert.Equal(expected.County, actual.County);
            Assert.Equal(expected.PostCode, actual.PostCode);
            Assert.Equal(expected.LandlinePhone, actual.LandlinePhone);
        }

        internal static void AssertEqual(CreateAddressDTO expected, Address actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Line1, actual.Line1);
            Assert.Equal(expected.Line2, actual.Line2);
            Assert.Equal(expected.Line3, actual.Line3);
            Assert.Equal(expected.Town, actual.Town);
            Assert.Equal(expected.County, actual.County);
            Assert.Equal(expected.PostCode, actual.PostCode);
            Assert.Equal(expected.LandlinePhone, actual.LandlinePhone);
        }

        internal static void AssertEqual(UpdateAddressDTO expected, AddressDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Line1, actual.Line1);
            Assert.Equal(expected.Line2, actual.Line2);
            Assert.Equal(expected.Line3, actual.Line3);
            Assert.Equal(expected.Town, actual.Town);
            Assert.Equal(expected.County, actual.County);
            Assert.Equal(expected.PostCode, actual.PostCode);
            Assert.Equal(expected.LandlinePhone, actual.LandlinePhone);
        }

        internal static void AssertNotEqual(Address expected, Address actual)
        {
            Assert.NotNull(actual);
            Assert.NotEqual(expected.Id, actual.Id);
            Assert.NotEqual(expected.Line1, actual.Line1);
            Assert.NotEqual(expected.Line2, actual.Line2);
            Assert.NotEqual(expected.Line3, actual.Line3);
            Assert.NotEqual(expected.Town, actual.Town);
            Assert.NotEqual(expected.County, actual.County);
            Assert.NotEqual(expected.PostCode, actual.PostCode);
            Assert.NotEqual(expected.LandlinePhone, actual.LandlinePhone);
        }

        internal static void AssertNotEqual(Address expected, CreateAddressDTO actual)
        {
            Assert.NotNull(actual);
            Assert.NotEqual(expected.Line1, actual.Line1);
            Assert.NotEqual(expected.Line2, actual.Line2);
            Assert.NotEqual(expected.Line3, actual.Line3);
            Assert.NotEqual(expected.Town, actual.Town);
            Assert.NotEqual(expected.County, actual.County);
            Assert.NotEqual(expected.PostCode, actual.PostCode);
            Assert.NotEqual(expected.LandlinePhone, actual.LandlinePhone);
        }

        internal static void AssertNotEqual(Address expected, UpdateAddressDTO actual)
        {
            Assert.NotNull(actual);
            Assert.NotEqual(expected.Line1, actual.Line1);
            Assert.NotEqual(expected.Line2, actual.Line2);
            Assert.NotEqual(expected.Line3, actual.Line3);
            Assert.NotEqual(expected.Town, actual.Town);
            Assert.NotEqual(expected.County, actual.County);
            Assert.NotEqual(expected.PostCode, actual.PostCode);
            Assert.NotEqual(expected.LandlinePhone, actual.LandlinePhone);
        }
    }
}
