// <copyright file="ArtsController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using MartialBase.API.Models.DTOs.Arts;
using MartialBase.API.TestTools.Http;
using MartialBase.API.TestTools.Models;

namespace MartialBase.API.TestTools.ControllerMethods
{
    public static class ArtsController
    {
        public static async Task<List<ArtDTO>> GetArtsAsync(HttpClient client) => await HttpClientMethods<List<ArtDTO>>.GetAsync(client, "arts");

        public static async Task<HttpResponseModel> GetArtsResponseAsync(HttpClient client) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                "arts");

        public static async Task<ArtDTO> GetArtAsync(HttpClient client, string artId) =>
            await HttpClientMethods<ArtDTO>.GetAsync(client, $"arts/{artId}");

        public static async Task<HttpResponseModel> GetArtResponseAsync(HttpClient client, string artId) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"arts/{artId}");
    }
}
