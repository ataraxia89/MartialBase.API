// <copyright file="IOrganisationsRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MartialBase.API.Data.Models.InternalDTOs.Organisations;
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.DTOs.People;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    public interface IOrganisationsRepository : IRepository<OrganisationDTO, CreateOrganisationInternalDTO, UpdateOrganisationDTO>
    {
        Task<PersonOrganisationDTO> CreateOrganisationWithNewPersonAsAdminAsync(
            CreateOrganisationInternalDTO createOrganisationDTO, CreatePersonInternalDTO createPersonDTO, Guid azureId);

        Task<List<OrganisationDTO>> GetChildOrganisationsAsync(Guid parentOrganisationId);

        Task<Guid?> GetOrganisationParentIdAsync(Guid organisationId);

        Task<List<OrganisationPersonDTO>> GetPeopleAsync(Guid organisationId);

        Task ChangeOrganisationParentAsync(Guid organisationId, Guid? parentOrganisationId);

        Task<AddressDTO> ChangeOrganisationAddressAsync(Guid organisationId, CreateAddressDTO createAddressDTO);

        Task<bool> PersonCanAccessOrganisationAsync(Guid organisationId, Guid personId);

        Task AddOrganisationPersonAsync(Guid organisationId, Guid personId, bool isAdmin);

        Task RemoveOrganisationPersonAsync(Guid organisationId, Guid personId);

        Task DemoteOrganisationAdminAsync(Guid organisationId, Guid personId);

        Task<bool> PersonIsOrganisationAdminAsync(Guid organisationId, Guid personId);

        Task<List<PersonOrganisationDTO>> GetOrganisationsForPersonAsync(Guid personId);
    }
}
