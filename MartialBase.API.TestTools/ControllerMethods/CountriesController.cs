// <copyright file="CountriesController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using MartialBase.API.Models.DTOs.Countries;
using MartialBase.API.TestTools.Http;
using MartialBase.API.TestTools.Models;

namespace MartialBase.API.TestTools.ControllerMethods
{
    public static class CountriesController
    {
        public static async Task<List<CountryDTO>> GetCountriesAsync(HttpClient client) =>
            await HttpClientMethods<List<CountryDTO>>.GetAsync(client, "countries");

        public static async Task<HttpResponseModel> GetCountriesResponseAsync(HttpClient client) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                "countries");

        public static async Task<CountryDTO> GetCountryAsync(HttpClient client, string countryCode) =>
            await HttpClientMethods<CountryDTO>.GetAsync(client, $"countries/{countryCode}");

        public static async Task<HttpResponseModel> GetCountryResponseAsync(HttpClient client, string countryCode) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"countries/{countryCode}");
    }
}
