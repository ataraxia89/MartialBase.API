// <copyright file="SchoolResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Schools;
using MartialBase.API.Models.DTOs.Schools;
using MartialBase.API.Tools;

namespace MartialBase.API.DataGenerator
{
    public static class Schools
    {
        public static School GenerateSchoolObject(bool realisticData = false)
        {
            var schoolId = Guid.NewGuid();
            Art art = Arts.GenerateArtObject();
            Organisation organisation = Organisations.GenerateOrganisationObject(realisticData: realisticData);
            Person headInstructor = People.GeneratePersonObject(realisticData: realisticData);
            Address address = Addresses.GenerateAddressObject(realisticData);

            var schoolAddress = new SchoolAddress
            {
                Id = Guid.NewGuid(),
                SchoolId = schoolId,
                AddressId = address.Id,
                Address = address
            };

            return new School
            {
                Id = schoolId,
                ArtId = art.Id,
                Art = art,
                OrganisationId = organisation.Id,
                Organisation = organisation,
                Name = realisticData ? FakeData.Company.Name() : RandomData.GetRandomString(20),
                HeadInstructorId = headInstructor.Id,
                HeadInstructor = headInstructor,
                DefaultAddressId = address.Id,
                DefaultAddress = address,
                PhoneNo = realisticData ? FakeData.Phone.Number() : RandomData.GetRandomString(30),
                Email = realisticData ? FakeData.Internet.Email() : RandomData.GetRandomString(50),
                Website = realisticData ? FakeData.Internet.SecureUrl() : RandomData.GetRandomString(70),
                SchoolAddresses = new List<SchoolAddress> { schoolAddress }
            };
        }

        public static CreateSchoolInternalDTO GenerateCreateSchoolInternalDTO(
            Guid artId,
            Guid organisationId,
            Guid headInstructorId,
            bool realisticData = false)
        {
            var school = GenerateSchoolObject(realisticData);

            school.ArtId = artId;
            school.Art = null;
            school.OrganisationId = organisationId;
            school.Organisation = null;
            school.HeadInstructorId = headInstructorId;
            school.HeadInstructor = null;

            return ModelMapper.GetCreateSchoolInternalDTO(school);
        }

        public static UpdateSchoolDTO GenerateUpdateSchoolDTO(bool realisticData = false) =>
            ModelMapper.GetUpdateSchoolDTO(GenerateSchoolObject(realisticData));
    }
}
