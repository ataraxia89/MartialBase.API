// <copyright file="CountriesRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Countries;

namespace MartialBase.API.Data.Repositories
{
    public class CountriesRepository : ICountriesRepository
    {
        public bool Exists(string countryCode) => Countries.Exists(countryCode);

        public List<CountryDTO> GetAll(long? populationLimit = null) => Countries.GetAllCountryDTOs(populationLimit);

        public CountryDTO Get(string countryCode) => Countries.GetCountryDTO(countryCode);
    }
}
