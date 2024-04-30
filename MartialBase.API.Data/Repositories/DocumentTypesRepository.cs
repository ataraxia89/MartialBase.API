// <copyright file="DocumentTypesRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.DocumentTypes;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.DocumentTypes;

using Microsoft.EntityFrameworkCore;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.Data.Repositories
{
    public class DocumentTypesRepository : IDocumentTypesRepository
    {
        private readonly MartialBaseDbContext _context;

        public DocumentTypesRepository(MartialBaseDbContext context) => _context = context;

        public async Task<bool> ExistsAsync(Guid id) => await _context.DocumentTypes.AnyAsync(dt => dt.Id == id);

        public async Task<DocumentTypeDTO> CreateAsync(CreateDocumentTypeInternalDTO createDTO)
        {
            var documentType = (await _context.DocumentTypes.AddAsync(ModelMapper.GetDocumentType(createDTO))).Entity;

            documentType.Organisation = await _context.Organisations.FirstAsync(o => o.Id == documentType.OrganisationId);

            return ModelMapper.GetDocumentTypeDTO(documentType);
        }

        public async Task<List<DocumentTypeDTO>> GetAllAsync() => await GetAllAsync(null);

        public async Task<List<DocumentTypeDTO>> GetAllAsync(Guid? organisationId)
        {
            List<DocumentType> documentTypes;
            var documentTypeDTOs = new List<DocumentTypeDTO>();

            if (organisationId != null)
            {
                documentTypes = await _context.DocumentTypes
                    .Include(dt => dt.Organisation)
                    .Where(dt => dt.OrganisationId == organisationId)
                    .ToListAsync();
            }
            else
            {
                documentTypes = await _context.DocumentTypes
                    .Include(dt => dt.Organisation)
                    .ToListAsync();
            }

            foreach (DocumentType documentType in documentTypes)
            {
                documentTypeDTOs.Add(ModelMapper.GetDocumentTypeDTO(documentType));
            }

            return documentTypeDTOs;
        }

        public async Task<DocumentTypeDTO> GetAsync(Guid id)
        {
            DocumentType documentType = await _context.DocumentTypes
                .Include(dt => dt.Organisation)
                .FirstAsync(dt => dt.Id == id);

            return ModelMapper.GetDocumentTypeDTO(documentType);
        }

        public async Task<Guid> GetDocumentTypeOrganisationIdAsync(Guid documentTypeId) =>
            await _context.DocumentTypes.Where(dt => dt.Id == documentTypeId).Select(dt => dt.OrganisationId)
                .FirstAsync();

        public async Task<DocumentTypeDTO> UpdateAsync(Guid id, UpdateDocumentTypeDTO updateDTO)
        {
            if (updateDTO == null)
            {
                throw new ArgumentNullException(nameof(updateDTO));
            }

            DocumentType documentType = await _context.DocumentTypes
                .Include(dt => dt.Organisation)
                .FirstAsync(dt => dt.Id == id);

            documentType.Description = updateDTO.Name;
            documentType.ReferenceNo = updateDTO.ReferenceNo;
            documentType.DefaultExpiryDays = updateDTO.DefaultExpiryDays;
            documentType.URL = updateDTO.URL;

            return ModelMapper.GetDocumentTypeDTO(documentType);
        }

        public async Task ChangeDocumentTypeOrganisationAsync(Guid documentTypeId, Guid newOrganisationId)
        {
            DocumentType documentType = await _context.DocumentTypes.FirstAsync(dt => dt.Id == documentTypeId);

            documentType.OrganisationId = newOrganisationId;
        }

        public async Task DeleteAsync(Guid id) =>
            _context.DocumentTypes.Remove(await _context.DocumentTypes.FirstAsync(dt => dt.Id == id));

        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() >= 0;
    }
}
