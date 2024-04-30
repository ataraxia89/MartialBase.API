// <copyright file="AddressResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Tools;

namespace MartialBase.API.DataGenerator
{
    public static class Addresses
    {
        public static Address GenerateAddressObject(bool realisticData = false)
        {
            Country country = Countries.GetRandomCountry();

            return new Address
            {
                Id = Guid.NewGuid(),
                Line1 = realisticData ? FakeData.Address.StreetAddress() : RandomData.GetRandomString(50),
                Line2 = realisticData ? FakeData.Address.SecondaryAddress() : RandomData.GetRandomString(30),
                Line3 = realisticData ? FakeData.Address.SecondaryAddress() : RandomData.GetRandomString(30),
                Town = realisticData ? FakeData.Address.City() : RandomData.GetRandomString(80),
                County = realisticData ? FakeData.Address.UkCounty() : RandomData.GetRandomString(30),
                PostCode = realisticData ? FakeData.Address.UkPostCode() : RandomData.GetRandomString(8),
                CountryCode = country.Code,
                LandlinePhone = realisticData ? FakeData.Phone.Number() : RandomData.GetRandomString(30)
            };
        }

        public static AddressDTO GenerateAddressDTO(bool realisticData = false) =>
            ModelMapper.GetAddressDTO(GenerateAddressObject(realisticData));

        public static CreateAddressDTO GenerateCreateAddressDTO(bool realisticData = false) =>
            ModelMapper.GetCreateAddressDTO(GenerateAddressObject(realisticData));

        internal static UpdateAddressDTO GenerateUpdateAddressDTO(bool realisticData = false) =>
            ModelMapper.GetUpdateAddressDTO(GenerateAddressObject(realisticData));
    }
}
