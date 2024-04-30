// <copyright file="ArtsRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Arts;

using Microsoft.EntityFrameworkCore;

namespace MartialBase.API.Data.Repositories
{
    public class ArtsRepository : IArtsRepository
    {
        private readonly MartialBaseDbContext _context;

        public ArtsRepository(MartialBaseDbContext context) => _context = context;

        public async Task<bool> ExistsAsync(Guid id) => await _context.Arts.AnyAsync(a => a.Id == id);

        public async Task<List<ArtDTO>> GetAllAsync()
        {
            var artDTOs = new List<ArtDTO>();

            foreach (Art art in await _context.Arts.ToListAsync())
            {
                artDTOs.Add(ModelMapper.GetArtDTO(art));
            }

            return artDTOs;
        }

        public async Task<ArtDTO> GetAsync(Guid id)
        {
            Art art = await _context.Arts.FirstAsync(a => a.Id == id);

            return ModelMapper.GetArtDTO(art);
        }
    }
}
