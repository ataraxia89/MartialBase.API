// <copyright file="ArtGradesController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using MartialBase.API.Models.DTOs.ArtGrades;
using MartialBase.API.TestTools.Http;
using MartialBase.API.TestTools.Models;

namespace MartialBase.API.TestTools.ControllerMethods
{
    public class ArtGradesController
    {
        public static async Task<List<ArtGradeDTO>> GetArtGradesAsync(HttpClient client, string artId, string organisationId) =>
            await HttpClientMethods<List<ArtGradeDTO>>.GetAsync(
                client,
                $"artgrades?artId={artId}&organisationId={organisationId}");

        public static async Task<HttpResponseModel> GetArtGradesResponseAsync(
            HttpClient client,
            string artId,
            string organisationId) => await HttpClientMethods.GetResponseAsync(
            client,
            $"artgrades?artId={artId}&organisationId={organisationId}");

        public static async Task<ArtGradeDTO> GetArtGradeAsync(HttpClient client, string artGradeId) =>
            await HttpClientMethods<ArtGradeDTO>.GetAsync(client, $"artgrades/{artGradeId}");

        public static async Task<HttpResponseModel> GetArtGradeResponseAsync(HttpClient client, string artGradeId) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"artgrades/{artGradeId}");

        public static async Task<ArtGradeDTO> CreateArtGradeAsync(HttpClient client, CreateArtGradeDTO createArtGradeDTO) =>
            await HttpClientMethods<ArtGradeDTO>.PostAsync(client, "artgrades", createArtGradeDTO);

        public static async Task<HttpResponseModel> CreateArtGradeResponseAsync(
            HttpClient client,
            CreateArtGradeDTO createArtGradeDTO) =>
            await HttpClientMethods.PostResponseAsync(
                client,
                "artgrades",
                createArtGradeDTO);

        public static async Task<ArtGradeDTO> UpdateArtGradeAsync(
            HttpClient client,
            string artGradeId,
            UpdateArtGradeDTO updateArtGradeDTO) => await HttpClientMethods<ArtGradeDTO>.PutAsync(
            client,
            $"artgrades/{artGradeId}",
            updateArtGradeDTO);

        public static async Task<HttpResponseModel> UpdateArtGradeResponseAsync(
            HttpClient client,
            string artGradeId,
            UpdateArtGradeDTO updateArtGradeDTO) => await HttpClientMethods.PutResponseAsync(
            client,
            $"artgrades/{artGradeId}",
            updateArtGradeDTO);

        public static async Task<bool> DeleteArtGradeAsync(HttpClient client, string artGradeId) =>
            await HttpClientMethods.DeleteAsync(client, $"artgrades/{artGradeId}");

        public static async Task<HttpResponseModel> DeleteArtGradeResponseAsync(HttpClient client, string artGradeId) =>
            await HttpClientMethods.DeleteResponseAsync(
                client,
                $"artgrades/{artGradeId}");
    }
}
