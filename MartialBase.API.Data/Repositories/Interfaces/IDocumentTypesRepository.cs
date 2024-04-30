// <copyright file="IDocumentTypesRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MartialBase.API.Data.Models.InternalDTOs.DocumentTypes;
using MartialBase.API.Models.DTOs.DocumentTypes;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    public interface IDocumentTypesRepository : IRepository<DocumentTypeDTO, CreateDocumentTypeInternalDTO, UpdateDocumentTypeDTO>
    {
        Task<List<DocumentTypeDTO>> GetAllAsync(Guid? organisationId);

        Task<Guid> GetDocumentTypeOrganisationIdAsync(Guid documentTypeId);

        Task ChangeDocumentTypeOrganisationAsync(Guid documentTypeId, Guid newOrganisationId);
    }
}
