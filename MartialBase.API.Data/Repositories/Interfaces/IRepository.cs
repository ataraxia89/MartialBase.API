// <copyright file="IRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    public interface IRepository<TEntityDTO, TCreateDTO, TUpdateDTO>
    {
        Task<bool> ExistsAsync(Guid id);

        Task<TEntityDTO> CreateAsync(TCreateDTO createDTO);

        Task<List<TEntityDTO>> GetAllAsync();

        Task<TEntityDTO> GetAsync(Guid id);

        Task<TEntityDTO> UpdateAsync(Guid id, TUpdateDTO updateDTO);

        Task DeleteAsync(Guid id);

        Task<bool> SaveChangesAsync();
    }
}
