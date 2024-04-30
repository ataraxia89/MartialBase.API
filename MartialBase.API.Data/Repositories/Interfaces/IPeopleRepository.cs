// <copyright file="IPeopleRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Models.DTOs.People;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    public interface IPeopleRepository : IRepository<PersonDTO, CreatePersonInternalDTO, UpdatePersonDTO>, IPersonDocumentsRepository
    {
        new Task<CreatedPersonDTO> CreateAsync(CreatePersonInternalDTO createDTO);

        Task<CreatedPersonDTO> CreateAsync(CreatePersonInternalDTO createDTO, Guid? organisationId, Guid? schoolId);

        Task<List<PersonDTO>> GetAllAsync(Guid? organisationId, Guid? schoolId);

        Task<Guid?> FindPersonIdByEmailAsync(string personEmail);

        Task<string> GetPersonNameFromIdAsync(Guid personId);

        Task<List<PersonDTO>> FindAsync(string email = null, string firstName = null, string middleName = null, string lastName = null, bool returnAddresses = false);
    }
}
