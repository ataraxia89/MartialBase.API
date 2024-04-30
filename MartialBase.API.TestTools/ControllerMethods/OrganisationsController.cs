// <copyright file="OrganisationsController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.DocumentTypes;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.TestTools.Http;
using MartialBase.API.TestTools.Models;

namespace MartialBase.API.TestTools.ControllerMethods
{
    public class OrganisationsController
    {
        public static async Task<List<OrganisationDTO>> GetOrganisationsAsync(HttpClient client, string parentId) =>
            await HttpClientMethods<List<OrganisationDTO>>.GetAsync(
                client,
                $"organisations{(parentId != null ? $"?parentId={parentId}" : string.Empty)}");

        public static async Task<HttpResponseModel> GetOrganisationsResponseAsync(HttpClient client, string parentId) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"organisations{(parentId != null ? $"?parentId={parentId}" : string.Empty)}");

        public static async Task<OrganisationDTO> GetOrganisationAsync(HttpClient client, string organisationId) =>
            await HttpClientMethods<OrganisationDTO>.GetAsync(client, $"organisations/{organisationId}");

        public static async Task<HttpResponseModel> GetOrganisationResponseAsync(
            HttpClient client,
            string organisationId) => await HttpClientMethods.GetResponseAsync(
            client,
            $"organisations/{organisationId}");

        public static async Task<OrganisationDTO> CreateOrganisationAsync(
            HttpClient client,
            CreateOrganisationDTO createOrganisationDTO) =>
            await HttpClientMethods<OrganisationDTO>.PostAsync(client, "organisations", createOrganisationDTO);

        public static async Task<HttpResponseModel> CreateOrganisationResponseAsync(
            HttpClient client,
            CreateOrganisationDTO createOrganisationDTO) => await HttpClientMethods.PostResponseAsync(
            client,
            "organisations",
            createOrganisationDTO);

        public static async Task<PersonOrganisationDTO> CreateOrganisationWithNewPersonAsAdminAsync(
            HttpClient client,
            CreatePersonOrganisationDTO createPersonOrganisationDTO) =>
            await HttpClientMethods<PersonOrganisationDTO>.PostAsync(
                client,
                "organisations/newperson",
                createPersonOrganisationDTO);

        public static async Task<HttpResponseModel> CreateOrganisationWithNewPersonAsAdminResponseAsync(
            HttpClient client,
            CreatePersonOrganisationDTO createPersonOrganisationDTO) => await HttpClientMethods.PostResponseAsync(
            client,
            "organisations/newperson",
            createPersonOrganisationDTO);

        public static async Task<OrganisationDTO> UpdateOrganisationAsync(
            HttpClient client,
            string organisationId,
            UpdateOrganisationDTO updateOrganisationDTO) => await HttpClientMethods<OrganisationDTO>.PutAsync(
            client,
            $"organisations/{organisationId}",
            updateOrganisationDTO);

        public static async Task<HttpResponseModel> UpdateOrganisationResponseAsync(
            HttpClient client,
            string organisationId,
            UpdateOrganisationDTO updateOrganisationDTO) => await HttpClientMethods.PutResponseAsync(
            client,
            $"organisations/{organisationId}",
            updateOrganisationDTO);

        public static async Task<bool> ChangeOrganisationParentAsync(
            HttpClient client,
            string organisationId,
            string parentId) => await HttpClientMethods.PutAsync(
            client,
            $"organisations/{organisationId}/parent{(parentId != null ? $"?parentId={parentId}" : string.Empty)}");

        public static async Task<HttpResponseModel> ChangeOrganisationParentResponseAsync(
            HttpClient client,
            string organisationId,
            string parentId) => await HttpClientMethods.PutResponseAsync(
            client,
            $"organisations/{organisationId}/parent{(parentId != null ? $"?parentId={parentId}" : string.Empty)}");

        public static async Task<bool> RemoveOrganisationParentAsync(HttpClient client, string organisationId) =>
            await HttpClientMethods.DeleteAsync(client, $"organisations/{organisationId}/parent");

        public static async Task<HttpResponseModel> RemoveOrganisationParentResponseAsync(
            HttpClient client,
            string organisationId) => await HttpClientMethods.DeleteResponseAsync(
            client,
            $"organisations/{organisationId}/parent");

        public static async Task<AddressDTO> ChangeOrganisationAddressAsync(
            HttpClient client,
            string organisationId,
            CreateAddressDTO createAddressDTO) => await HttpClientMethods<AddressDTO>.PostAsync(
            client,
            $"organisations/{organisationId}/address",
            createAddressDTO);

        public static async Task<HttpResponseModel> ChangeOrganisationAddressResponseAsync(
            HttpClient client,
            string organisationId,
            CreateAddressDTO createAddressDTO) => await HttpClientMethods.PostResponseAsync(
            client,
            $"organisations/{organisationId}/address",
            createAddressDTO);

        public static async Task<bool> AddExistingPersonToOrganisationAsync(
            HttpClient client,
            string organisationId,
            string personId,
            string isAdmin)
        {
            string parameters = string.Empty;

            if (personId != null && isAdmin != null)
            {
                parameters = $"?personId={personId}&isAdmin={isAdmin}";
            }
            else if (personId != null)
            {
                parameters = $"?personId={personId}";
            }

            return await HttpClientMethods.PostAsync(client, $"organisations/{organisationId}/people{parameters}");
        }

        public static async Task<HttpResponseModel> AddExistingPersonToOrganisationResponseAsync(
            HttpClient client,
            string organisationId,
            string personId,
            string isAdmin)
        {
            string parameters = string.Empty;

            if (personId != null && isAdmin != null)
            {
                parameters = $"?personId={personId}&isAdmin={isAdmin}";
            }
            else if (personId != null)
            {
                parameters = $"?personId={personId}";
            }
            else if (isAdmin != null)
            {
                parameters = $"?isAdmin={isAdmin}";
            }

            return await HttpClientMethods.PostResponseAsync(
                client,
                $"organisations/{organisationId}/people{parameters}");
        }

        public static async Task<bool> RemoveOrganisationPersonAsync(
            HttpClient client,
            string organisationId,
            string personId) => await HttpClientMethods.DeleteAsync(
            client,
            $"organisations/{organisationId}/people/{personId}");

        public static async Task<HttpResponseModel> RemoveOrganisationPersonResponseAsync(
            HttpClient client,
            string organisationId,
            string personId) => await HttpClientMethods.DeleteResponseAsync(
            client,
            $"organisations/{organisationId}/people/{personId}");

        public static async Task<List<OrganisationPersonDTO>> GetOrganisationPeopleAsync(
            HttpClient client,
            string organisationId) => await HttpClientMethods<List<OrganisationPersonDTO>>.GetAsync(
            client,
            $"organisations/{organisationId}/people");

        public static async Task<HttpResponseModel> GetOrganisationPeopleResponseAsync(
            HttpClient client,
            string organisationId) => await HttpClientMethods.GetResponseAsync(
            client,
            $"organisations/{organisationId}/people");

        public static async Task<List<DocumentTypeDTO>> GetOrganisationDocumentTypesAsync(
            HttpClient client,
            string organisationId) => await HttpClientMethods<List<DocumentTypeDTO>>.GetAsync(
            client,
            $"organisations/{organisationId}/documenttypes");

        public static async Task<HttpResponseModel> GetOrganisationDocumentTypesResponseAsync(
            HttpClient client,
            string organisationId) => await HttpClientMethods.GetResponseAsync(
            client,
            $"organisations/{organisationId}/documenttypes");

        public static async Task<bool> DeleteOrganisationAsync(HttpClient client, string organisationId) =>
            await HttpClientMethods.DeleteAsync(client, $"organisations/{organisationId}");

        public static async Task<HttpResponseModel> DeleteOrganisationResponseAsync(
            HttpClient client,
            string organisationId) => await HttpClientMethods.DeleteResponseAsync(
            client,
            $"organisations/{organisationId}");
    }
}
