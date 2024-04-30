// <copyright file="OrganisationResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Organisations;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Tools;

namespace MartialBase.API.DataGenerator
{
    public static class Organisations
    {
        public static Organisation GenerateOrganisationObject(string parentId = null, bool realisticData = false)
        {
            var address = Addresses.GenerateAddressObject(realisticData);
            var name = realisticData ? FakeData.Company.Name() : RandomData.GetRandomString(60);
            var initials = realisticData
                ? new Regex("[^A-Z]").Replace(name, string.Empty)
                : RandomData.GetRandomString(8);

            return new Organisation
            {
                Id = Guid.NewGuid(),
                Initials = initials,
                Name = name,
                AddressId = address.Id,
                Address = address,
                ParentId = parentId == null ? null : new Guid(parentId)
            };
        }

        public static OrganisationDTO GenerateOrganisationDTO(string parentId = null, bool realisticData = false) =>
            ModelMapper.GetOrganisationDTO(GenerateOrganisationObject(parentId, realisticData));

        public static List<OrganisationDTO> GenerateOrganisationDTOs(int numberToGenerate, string parentId = null, bool realisticData = false)
        {
            var organisations = new List<OrganisationDTO>();

            for (int i = 0; i < numberToGenerate; i++)
            {
                organisations.Add(GenerateOrganisationDTO(parentId, realisticData));
            }

            return organisations;
        }

        public static CreateOrganisationDTO GenerateCreateOrganisationDTOObject(bool realisticData = false) =>
            ModelMapper.GetCreateOrganisationDTO(GenerateOrganisationObject(realisticData: realisticData));

        public static CreateOrganisationInternalDTO GenerateCreateOrganisationInternalDTOObject(bool realisticData = false) =>
            ModelMapper.GetCreateOrganisationInternalDTO(GenerateOrganisationObject(realisticData: realisticData));

        public static CreatePersonOrganisationDTO GenerateCreatePersonOrganisationDTOObject(Organisation organisation = null, Person person = null, bool realisticData = false)
        {
            if (organisation == null)
            {
                organisation = GenerateOrganisationObject(realisticData: realisticData);
            }

            if (person == null)
            {
                person = People.GeneratePersonObject(realisticData: realisticData);
            }

            return new CreatePersonOrganisationDTO
            {
                Organisation = ModelMapper.GetCreateOrganisationDTO(organisation),
                Person = ModelMapper.GetCreatePersonDTO(person)
            };
        }

        public static UpdateOrganisationDTO GenerateUpdateOrganisationDTOObject(bool realisticData = false)
        {
            var name = realisticData ? FakeData.Company.Name() : RandomData.GetRandomString(60);
            var initials = realisticData
                ? new Regex("[^A-Z]").Replace(name, string.Empty)
                : RandomData.GetRandomString(8);

            return new UpdateOrganisationDTO { Initials = initials, Name = name };
        }
    }
}
