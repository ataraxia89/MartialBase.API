// <copyright file="IAddressesRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using MartialBase.API.Models.DTOs.Addresses;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    public interface IAddressesRepository
    {
        Task<bool> ExistsAsync(Guid id);

        Task<AddressDTO> GetAsync(Guid id);
    }
}
