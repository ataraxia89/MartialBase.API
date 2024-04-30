// <copyright file="ArtGradesRepository.cs" company="Martialtech®">
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
using MartialBase.API.Data.Models.InternalDTOs.ArtGrades;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.ArtGrades;

using Microsoft.EntityFrameworkCore;

namespace MartialBase.API.Data.Repositories
{
    public class ArtGradesRepository : IArtGradesRepository
    {
        private readonly MartialBaseDbContext _context;

        public ArtGradesRepository(MartialBaseDbContext context) => _context = context;

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(Guid id) => await _context.ArtGrades.AnyAsync(ag => ag.Id == id);

        /// <inheritdoc />
        public async Task<ArtGradeDTO> CreateAsync(CreateArtGradeInternalDTO createDTO)
        {
            var artGrade = (await _context.ArtGrades.AddAsync(ModelMapper.GetArtGrade(createDTO))).Entity;

            artGrade.Art = await _context.Arts.FirstAsync(a => a.Id == artGrade.ArtId);
            artGrade.Organisation = await _context.Organisations.FirstAsync(o => o.Id == artGrade.OrganisationId);

            return ModelMapper.GetArtGradeDTO(artGrade);
        }

        /// <inheritdoc />
        public Task<List<ArtGradeDTO>> GetAllAsync() => throw new NotImplementedException();

        /// <inheritdoc />
        public async Task<List<ArtGradeDTO>> GetAllAsync(Guid artId, Guid organisationId)
        {
            if (!await _context.Arts.AnyAsync(a => a.Id == artId))
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            if (!await _context.Organisations.AnyAsync(o => o.Id == organisationId))
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            var artGradeDTOs = new List<ArtGradeDTO>();

            var artGrades = await _context.ArtGrades
                .Include(ag => ag.Art)
                .Include(ag => ag.Organisation)
                .Where(ag => ag.ArtId == artId && ag.OrganisationId == organisationId)
                .OrderBy(ag => ag.GradeLevel)
                .ToListAsync();

            foreach (ArtGrade artGrade in artGrades)
            {
                artGradeDTOs.Add(ModelMapper.GetArtGradeDTO(artGrade));
            }

            return artGradeDTOs;
        }

        /// <inheritdoc />
        public async Task<Guid> GetArtGradeOrganisationIdAsync(Guid artGradeId) => await _context.ArtGrades
            .Where(ag => ag.Id == artGradeId).Select(ag => ag.OrganisationId).FirstAsync();

        /// <inheritdoc />
        public async Task<ArtGradeDTO> GetAsync(Guid id)
        {
            ArtGrade artGrade = await _context.ArtGrades
                .Include(ag => ag.Art)
                .Include(ag => ag.Organisation)
                .FirstAsync(ag => ag.Id == id);

            return ModelMapper.GetArtGradeDTO(artGrade);
        }

        /// <inheritdoc />
        public async Task<ArtGradeDTO> UpdateAsync(Guid id, UpdateArtGradeDTO updateDTO)
        {
            if (updateDTO == null)
            {
                throw new ArgumentNullException(nameof(updateDTO));
            }

            ArtGrade artGrade = await _context.ArtGrades
                .Include(ag => ag.Art)
                .Include(ag => ag.Organisation)
                .FirstAsync(ag => ag.Id == id);

            artGrade.GradeLevel = updateDTO.GradeLevel;
            artGrade.Description = updateDTO.Description;

            return ModelMapper.GetArtGradeDTO(artGrade);
        }

        /// <inheritdoc />
        public async System.Threading.Tasks.Task DeleteAsync(Guid id) => _context.ArtGrades.Remove(await _context.ArtGrades.FirstAsync(ag => ag.Id == id));

        /// <inheritdoc />
        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() >= 0;
    }
}
