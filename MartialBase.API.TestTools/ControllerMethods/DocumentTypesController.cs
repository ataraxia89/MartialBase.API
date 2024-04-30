// <copyright file="DocumentTypesController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using MartialBase.API.Models.DTOs.DocumentTypes;
using MartialBase.API.TestTools.Http;
using MartialBase.API.TestTools.Models;

namespace MartialBase.API.TestTools.ControllerMethods
{
    public class DocumentTypesController
    {
        public static async Task<List<DocumentTypeDTO>> GetDocumentTypesAsync(
            HttpClient client,
            string organisationId) =>
            await HttpClientMethods<List<DocumentTypeDTO>>.GetAsync(
                client,
                $"documenttypes?organisationId={organisationId}");

        public static async Task<HttpResponseModel> GetDocumentTypesResponseAsync(
            HttpClient client,
            string organisationId) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"documenttypes?organisationId={organisationId}");

        public static async Task<List<DocumentTypeDTO>> GetDocumentTypeAsync(
            HttpClient client,
            string documentTypeId) =>
            await HttpClientMethods<List<DocumentTypeDTO>>.GetAsync(
                client,
                $"documenttypes/{documentTypeId}");

        public static async Task<HttpResponseModel> GetDocumentTypeResponseAsync(
            HttpClient client,
            string documentTypeId) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"documenttypes/{documentTypeId}");

        public static async Task<DocumentTypeDTO> CreateDocumentTypeAsync(
            HttpClient client,
            CreateDocumentTypeDTO createDocumentTypeDTO) =>
            await HttpClientMethods<DocumentTypeDTO>.PostAsync(
                client,
                "documenttypes",
                createDocumentTypeDTO);

        public static async Task<HttpResponseModel> CreateDocumentTypeResponseAsync(
            HttpClient client,
            CreateDocumentTypeDTO createDocumentTypeDTO) =>
            await HttpClientMethods.PostResponseAsync(
                client,
                "documenttypes",
                createDocumentTypeDTO);

        public static async Task<DocumentTypeDTO> UpdateDocumentTypeAsync(
            HttpClient client,
            string documentTypeId,
            UpdateDocumentTypeDTO updateDocumentTypeDTO) =>
            await HttpClientMethods<DocumentTypeDTO>.PutAsync(
                client,
                $"documenttypes?documentTypeId={documentTypeId}",
                updateDocumentTypeDTO);

        public static async Task<HttpResponseModel> UpdateDocumentTypeResponseAsync(
            HttpClient client,
            string documentTypeId,
            UpdateDocumentTypeDTO updateDocumentTypeDTO) =>
            await HttpClientMethods.PutResponseAsync(
                client,
                $"documenttypes?documentTypeId={documentTypeId}",
                updateDocumentTypeDTO);

        public static async Task ChangeDocumentTypeOrganisationAsync(
            HttpClient client,
            string documentTypeId,
            string organisationId) =>
            await HttpClientMethods.PutAsync(
                client,
                $"documenttypes/{documentTypeId}/organisation?organisationId={organisationId}");

        public static async Task<HttpResponseModel> ChangeDocumentTypeOrganisationResponseAsync(
            HttpClient client,
            string documentTypeId,
            string organisationId) =>
            await HttpClientMethods.PutResponseAsync(
                client,
                $"documenttypes/{documentTypeId}/organisation?organisationId={organisationId}");

        public static async Task DeleteDocumentTypeAsync(
            HttpClient client,
            string documentTypeId) =>
            await HttpClientMethods.DeleteAsync(
                client,
                $"documenttypes/{documentTypeId}");

        public static async Task<HttpResponseModel> DeleteDocumentTypeResponseAsync(
            HttpClient client,
            string documentTypeId) =>
            await HttpClientMethods.DeleteResponseAsync(
                client,
                $"documenttypes/{documentTypeId}");
    }
}
