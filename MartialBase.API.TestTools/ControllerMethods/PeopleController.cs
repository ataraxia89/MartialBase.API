// <copyright file="PeopleController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using MartialBase.API.Models.DTOs.People;
using MartialBase.API.TestTools.Http;
using MartialBase.API.TestTools.Models;

namespace MartialBase.API.TestTools.ControllerMethods
{
    public static class PeopleController
    {
        public static async Task<Guid?> GetPersonIdForCurrentMartialBaseUserAsync(HttpClient client) =>
            await HttpClientMethods<Guid?>.GetAsync(client, "people/getmyid");

        public static async Task<HttpResponseModel> GetPersonIdForCurrentMartialBaseUserResponseAsync(HttpClient client) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                "people/getmyid");

        public static async Task<Guid?> GetPersonIdForMartialBaseUserAsync(HttpClient client, string userId) =>
            await HttpClientMethods<Guid?>.GetAsync(
                client,
                $"people/getid{(userId != null ? "?userId=" + userId : string.Empty)}");

        public static async Task<HttpResponseModel> GetPersonIdForMartialBaseUserResponseAsync(
            HttpClient client,
            string userId) => await HttpClientMethods.GetResponseAsync(
            client,
            $"people/getid{(userId != null ? "?userId=" + userId : string.Empty)}");

        public static async Task<PersonDTO> GetPersonAsync(HttpClient client, string personId) =>
            await HttpClientMethods<PersonDTO>.GetAsync(client, $"people/{personId}");

        public static async Task<HttpResponseModel> GetPersonResponseAsync(HttpClient client, string personId) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"people/{personId}");

        public static async Task<List<PersonDTO>> FindPeopleAsync(
            HttpClient client,
            string email,
            string firstName,
            string middleName,
            string lastName,
            string returnAddresses)
        {
            var queryParameters = new List<string>();

            if (email != null)
            {
                queryParameters.Add($"email={email}");
            }

            if (firstName != null)
            {
                queryParameters.Add($"firstName={firstName}");
            }

            if (middleName != null)
            {
                queryParameters.Add($"middleName={middleName}");
            }

            if (lastName != null)
            {
                queryParameters.Add($"lastName={lastName}");
            }

            if (returnAddresses != null)
            {
                queryParameters.Add($"returnAddresses={returnAddresses}");
            }

            string queryString =
                queryParameters.Count > 0 ? $"?{string.Join("&", queryParameters)}" : string.Empty;

            return await HttpClientMethods<List<PersonDTO>>.GetAsync(client, $"people{queryString}");
        }

        public static async Task<HttpResponseModel> FindPeopleResponseAsync(
            HttpClient client,
            string email,
            string firstName,
            string middleName,
            string lastName,
            string returnAddresses)
        {
            var queryParameters = new List<string>();

            if (email != null)
            {
                queryParameters.Add($"email={email}");
            }

            if (firstName != null)
            {
                queryParameters.Add($"firstName={firstName}");
            }

            if (middleName != null)
            {
                queryParameters.Add($"middleName={middleName}");
            }

            if (lastName != null)
            {
                queryParameters.Add($"lastName={lastName}");
            }

            if (returnAddresses != null)
            {
                queryParameters.Add($"returnAddresses={returnAddresses}");
            }

            string queryString =
                queryParameters.Count > 0 ? $"?{string.Join("&", queryParameters)}" : string.Empty;

            return await HttpClientMethods.GetResponseAsync(
                client,
                $"people{queryString}");
        }

        public static async Task<CreatedPersonDTO> CreatePersonAsync(
            HttpClient client,
            CreatePersonDTO createPersonDTO,
            string organisationId,
            string schoolId) => await HttpClientMethods<CreatedPersonDTO>.PostAsync(
            client,
            $"people?organisationId={organisationId}&schoolId={schoolId}",
            createPersonDTO);

        public static async Task<HttpResponseModel> CreatePersonResponseAsync(
            HttpClient client,
            CreatePersonDTO createPersonDTO,
            string organisationId,
            string schoolId) => await HttpClientMethods.PostResponseAsync(
            client,
            $"people?organisationId={organisationId}&schoolId={schoolId}",
            createPersonDTO);

        public static async Task<PersonDTO> UpdatePersonAsync(
            HttpClient client,
            string personId,
            UpdatePersonDTO updatePersonDTO) => await HttpClientMethods<PersonDTO>.PutAsync(
            client,
            $"people/{personId}",
            updatePersonDTO);

        public static async Task<HttpResponseModel> UpdatePersonResponseAsync(
            HttpClient client,
            string personId,
            UpdatePersonDTO updatePersonDTO) => await HttpClientMethods.PutResponseAsync(
            client,
            $"people/{personId}",
            updatePersonDTO);

        public static async Task<List<PersonOrganisationDTO>> GetPersonOrganisationsAsync(HttpClient client, string personId) =>
            await HttpClientMethods<List<PersonOrganisationDTO>>.GetAsync(
                client,
                $"people/{personId}/organisations");

        public static async Task<HttpResponseModel> GetPersonOrganisationsResponseAsync(
            HttpClient client,
            string personId) => await HttpClientMethods.GetResponseAsync(
            client,
            $"people/{personId}/organisations");

        public static async Task<List<StudentSchoolDTO>> GetPersonSchoolsAsync(HttpClient client, string personId) =>
            await HttpClientMethods<List<StudentSchoolDTO>>.GetAsync(client, $"people/{personId}/schools");

        public static async Task<HttpResponseModel> GetPersonSchoolsResponseAsync(HttpClient client, string personId) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"people/{personId}/schools");

        public static async Task DeletePersonAsync(
            HttpClient client,
            string personId) =>
            await HttpClientMethods.DeleteAsync(
                client,
                $"people/{personId}");

        public static async Task<HttpResponseModel> DeletePersonResponseAsync(
            HttpClient client,
            string personId) =>
            await HttpClientMethods.DeleteResponseAsync(
                client,
                $"people/{personId}");
    }
}
