// <copyright file="SchoolsController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.DTOs.Schools;
using MartialBase.API.TestTools.Http;
using MartialBase.API.TestTools.Models;

using Xunit;

namespace MartialBase.API.TestTools.ControllerMethods
{
    [Collection("LiveControllerTests")]
    public static class SchoolsController
    {
        public static async Task<List<SchoolDTO>> GetSchoolsAsync(
            HttpClient client,
            string artId,
            string organisationId) => await HttpClientMethods<List<SchoolDTO>>.GetAsync(
            client,
            $"schools?artId={artId}&organisationId={organisationId}");

        public static async Task<HttpResponseModel> GetSchoolsResponseAsync(
            HttpClient client,
            string artId,
            string organisationId) => await HttpClientMethods.GetResponseAsync(
            client,
            $"schools?artId={artId}&organisationId={organisationId}");

        public static async Task<SchoolDTO> GetSchoolAsync(
            HttpClient client,
            string schoolId) =>
            await HttpClientMethods<SchoolDTO>.GetAsync(
                client,
                $"schools/{schoolId}");

        public static async Task<HttpResponseModel> GetSchoolResponseAsync(
            HttpClient client,
            string schoolId) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"schools/{schoolId}");

        public static async Task<SchoolDTO> CreateSchoolAsync(
            HttpClient client,
            CreateSchoolDTO createSchoolDTO) =>
            await HttpClientMethods<SchoolDTO>.PostAsync(
                client,
                "schools",
                createSchoolDTO);

        public static async Task<HttpResponseModel> CreateSchoolResponseAsync(
            HttpClient client,
            CreateSchoolDTO createSchoolDTO) =>
            await HttpClientMethods.PostResponseAsync(
                client,
                "schools",
                createSchoolDTO);

        public static async Task<SchoolDTO> UpdateSchoolAsync(
            HttpClient client,
            string schoolId,
            UpdateSchoolDTO updateSchoolDTO) =>
            await HttpClientMethods<SchoolDTO>.PutAsync(
                client,
                $"schools/{schoolId}",
                updateSchoolDTO);

        public static async Task<HttpResponseModel> UpdateSchoolResponseAsync(
            HttpClient client,
            string schoolId,
            UpdateSchoolDTO updateSchoolDTO) =>
            await HttpClientMethods.PutResponseAsync(
                client,
                $"schools/{schoolId}",
                updateSchoolDTO);

        public static async Task<AddressDTO> AddNewAddressToSchoolAsync(
            HttpClient client,
            string schoolId,
            CreateAddressDTO createAddressDTO) =>
            await HttpClientMethods<AddressDTO>.PostAsync(
                client,
                $"schools/{schoolId}/addresses",
                createAddressDTO);

        public static async Task<HttpResponseModel> AddNewAddressToSchoolResponseAsync(
            HttpClient client,
            string schoolId,
            CreateAddressDTO createAddressDTO) =>
            await HttpClientMethods.PostResponseAsync(
                client,
                $"schools/{schoolId}/addresses",
                createAddressDTO);

        public static async Task RemoveAddressFromSchoolAsync(
            HttpClient client,
            string schoolId,
            string addressId) =>
            await HttpClientMethods.DeleteAsync(
                client,
                $"schools/{schoolId}/addresses/{addressId}");

        public static async Task<HttpResponseModel> RemoveAddressFromSchoolResponseAsync(
            HttpClient client,
            string schoolId,
            string addressId) =>
            await HttpClientMethods.DeleteResponseAsync(
                client,
                $"schools/{schoolId}/addresses/{addressId}");

        public static async Task AddStudentToSchoolAsync(
            HttpClient client,
            string schoolId,
            string personId,
            string isInstructor,
            string isSecretary)
        {
            string parameters = $"?personId={personId}";

            if (isInstructor != null)
            {
                parameters += $"&isInstructor={isInstructor}";
            }
            else if (isSecretary != null)
            {
                parameters += $"&isSecretary={isSecretary}";
            }

            await HttpClientMethods.PostAsync(client, $"schools/{schoolId}/students{parameters}");
        }

        public static async Task<HttpResponseModel> AddStudentToSchoolResponseAsync(
            HttpClient client,
            string schoolId,
            string personId,
            string isInstructor,
            string isSecretary) => await HttpClientMethods.PostResponseAsync(
            client,
            $"schools/{schoolId}/students?personId={personId}&isInstructor={isInstructor}&isSecretary={isSecretary}");

        public static async Task<DocumentDTO> GetSchoolStudentInsuranceAsync(
            HttpClient client,
            string schoolId,
            string personId) =>
            await HttpClientMethods<DocumentDTO>.GetAsync(
                client,
                $"schools/{schoolId}/students/{personId}/insurance");

        public static async Task<HttpResponseModel> GetSchoolStudentInsuranceResponseAsync(
            HttpClient client,
            string schoolId,
            string personId) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"schools/{schoolId}/students/{personId}/insurance");

        public static async Task<DocumentDTO> UpdateSchoolStudentInsuranceAsync(
            HttpClient client,
            string schoolId,
            string personId,
            CreateDocumentDTO createDocumentDTO) =>
            await HttpClientMethods<DocumentDTO>.PutAsync(
                client,
                $"schools/{schoolId}/students/{personId}/insurance",
                createDocumentDTO);

        public static async Task<HttpResponseModel> UpdateSchoolStudentInsuranceResponseAsync(
            HttpClient client,
            string schoolId,
            string personId,
            CreateDocumentDTO createDocumentDTO) =>
            await HttpClientMethods.PutResponseAsync(
                client,
                $"schools/{schoolId}/students/{personId}/insurance",
                createDocumentDTO);

        public static async Task<DocumentDTO> GetSchoolStudentLicenceAsync(
            HttpClient client,
            string schoolId,
            string personId) =>
            await HttpClientMethods<DocumentDTO>.GetAsync(
                client,
                $"schools/{schoolId}/students/{personId}/licence");

        public static async Task<HttpResponseModel> GetSchoolStudentLicenceResponseAsync(
            HttpClient client,
            string schoolId,
            string personId) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"schools/{schoolId}/students/{personId}/licence");

        public static async Task<DocumentDTO> UpdateSchoolStudentLicenceAsync(
            HttpClient client,
            string schoolId,
            string personId,
            CreateDocumentDTO createDocumentDTO) =>
            await HttpClientMethods<DocumentDTO>.PutAsync(
                client,
                $"schools/{schoolId}/students/{personId}/licence",
                createDocumentDTO);

        public static async Task<HttpResponseModel> UpdateSchoolStudentLicenceResponseAsync(
            HttpClient client,
            string schoolId,
            string personId,
            CreateDocumentDTO createDocumentDTO) =>
            await HttpClientMethods.PutResponseAsync(
                client,
                $"schools/{schoolId}/students/{personId}/licence",
                createDocumentDTO);

        public static async Task<DocumentDTO> CreateStudentDocumentAsync(
            HttpClient client,
            string schoolId,
            string studentId,
            CreateDocumentDTO createDocumentDTO) => await HttpClientMethods<DocumentDTO>.PostAsync(
            client,
            $"schools/{schoolId}/students/{studentId}/documents",
            createDocumentDTO);

        public static async Task<HttpResponseModel> CreateStudentDocumentResponseAsync(
            HttpClient client,
            string schoolId,
            string studentId,
            CreateDocumentDTO createDocumentDTO) => await HttpClientMethods.PostResponseAsync(
            client,
            $"schools/{schoolId}/students/{studentId}/documents",
            createDocumentDTO);

        public static async Task<DocumentDTO> GetStudentDocumentAsync(
            HttpClient client,
            string schoolId,
            string studentId,
            string documentId) => await HttpClientMethods<DocumentDTO>.GetAsync(
            client,
            $"schools/{schoolId}/students/{studentId}/documents/{documentId}");

        public static async Task<HttpResponseModel> GetStudentDocumentResponseAsync(
            HttpClient client,
            string schoolId,
            string studentId,
            string documentId) => await HttpClientMethods.GetResponseAsync(
            client,
            $"schools/{schoolId}/students/{studentId}/documents/{documentId}");

        public static async Task<List<DocumentDTO>> GetStudentDocumentsAsync(
            HttpClient client,
            string schoolId,
            string studentId,
            string includeInactive) =>
            await HttpClientMethods<List<DocumentDTO>>.GetAsync(
                client,
                $"schools/{schoolId}/students/{studentId}/documents{(includeInactive != null ? $"?includeInactive={includeInactive}" : string.Empty)}");

        public static async Task<HttpResponseModel> GetStudentDocumentsResponseAsync(
            HttpClient client,
            string schoolId,
            string studentId,
            string includeInactive) => await HttpClientMethods.GetResponseAsync(
            client,
            $"schools/{schoolId}/students/{studentId}/documents{(includeInactive != null ? $"?includeInactive={includeInactive}" : string.Empty)}");

        public static async Task RemoveStudentFromSchoolAsync(
            HttpClient client,
            string schoolId,
            string personId) =>
            await HttpClientMethods.DeleteAsync(
                client,
                $"schools/{schoolId}/students/{personId}");

        public static async Task<HttpResponseModel> RemoveStudentFromSchoolResponseAsync(
            HttpClient client,
            string schoolId,
            string personId) =>
            await HttpClientMethods.DeleteResponseAsync(
                client,
                $"schools/{schoolId}/students/{personId}");

        public static async Task<List<SchoolStudentDTO>> GetSchoolStudentsAsync(
            HttpClient client,
            string schoolId) =>
            await HttpClientMethods<List<SchoolStudentDTO>>.GetAsync(
                client,
                $"schools/{schoolId}/students");

        public static async Task ChangeSchoolOrganisationAsync(
            HttpClient client,
            string schoolId,
            string organisationId) =>
            await HttpClientMethods.PutAsync(
                client,
                $"schools/{schoolId}/organisation?organisationId={organisationId}");

        public static async Task<HttpResponseModel> ChangeSchoolOrganisationResponseAsync(
            HttpClient client,
            string schoolId,
            string organisationId) =>
            await HttpClientMethods.PutResponseAsync(
                client,
                $"schools/{schoolId}/organisation?organisationId={organisationId}");

        public static async Task ChangeSchoolHeadInstructorAsync(
            HttpClient client,
            string schoolId,
            string personId,
            string retainSecretary)
        {
            string endpoint = $"schools/{schoolId}/headinstructor?personId={personId}";

            if (retainSecretary != null)
            {
                endpoint += $"&retainSecretary{retainSecretary}";
            }

            await HttpClientMethods.PutAsync(client, endpoint);
        }

        public static async Task<HttpResponseModel> ChangeSchoolHeadInstructorResponseAsync(
            HttpClient client,
            string schoolId,
            string personId,
            string retainSecretary)
        {
            string endpoint = $"schools/{schoolId}/headinstructor?personId={personId}";

            if (retainSecretary != null)
            {
                endpoint += $"&retainSecretary{retainSecretary}";
            }

            return await HttpClientMethods.PutResponseAsync(client, endpoint);
        }

        public static async Task ChangeSchoolArtAsync(
            HttpClient client,
            string schoolId,
            string artId) =>
            await HttpClientMethods.PutAsync(
                client,
                $"schools/{schoolId}/art?artId={artId}");

        public static async Task<HttpResponseModel> ChangeSchoolArtResponseAsync(
            HttpClient client,
            string schoolId,
            string artId) =>
            await HttpClientMethods.PutResponseAsync(
                client,
                $"schools/{schoolId}/art?artId={artId}");

        public static async Task DeleteSchoolAsync(
            HttpClient client,
            string schoolId) =>
            await HttpClientMethods.DeleteAsync(
                client,
                $"schools/{schoolId}");

        public static async Task<HttpResponseModel> DeleteSchoolResponseAsync(
            HttpClient client,
            string schoolId) =>
            await HttpClientMethods.DeleteResponseAsync(
                client,
                $"schools/{schoolId}");
    }
}
