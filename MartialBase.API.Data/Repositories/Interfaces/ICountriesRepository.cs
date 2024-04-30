// <copyright file="ICountriesRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.Models.DTOs.Countries;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    public interface ICountriesRepository
    {
        bool Exists(string countryCode);

        List<CountryDTO> GetAll(long? populationLimit = null);

        CountryDTO Get(string countryCode);
    }
}
