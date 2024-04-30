// <copyright file="IArtsRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MartialBase.API.Models.DTOs.Arts;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    public interface IArtsRepository
    {
        Task<bool> ExistsAsync(Guid id);

        Task<List<ArtDTO>> GetAllAsync();

        Task<ArtDTO> GetAsync(Guid id);
    }
}
