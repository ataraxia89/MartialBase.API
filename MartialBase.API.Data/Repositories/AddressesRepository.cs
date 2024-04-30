// <copyright file="AddressesRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Addresses;

using Microsoft.EntityFrameworkCore;

namespace MartialBase.API.Data.Repositories
{
    public class AddressesRepository : IAddressesRepository
    {
        private readonly MartialBaseDbContext _context;

        public AddressesRepository(MartialBaseDbContext context) => _context = context;

        public async Task<bool> ExistsAsync(Guid id) => await _context.Addresses.AnyAsync(a => a.Id == id);

        public async Task<AddressDTO> GetAsync(Guid id)
        {
            Address address = await _context.Addresses
                .FirstAsync(a => a.Id == id);

            return ModelMapper.GetAddressDTO(address);
        }
    }
}
