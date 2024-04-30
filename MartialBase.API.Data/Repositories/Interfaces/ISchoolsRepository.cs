// <copyright file="ISchoolsRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MartialBase.API.Data.Models.InternalDTOs.Documents;
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Data.Models.InternalDTOs.Schools;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.DTOs.Schools;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    public interface ISchoolsRepository : IRepository<SchoolDTO, CreateSchoolInternalDTO, UpdateSchoolDTO>
    {
        Task<bool> SchoolAddressExistsAsync(Guid schoolId, Guid addressId);

        Task<List<SchoolDTO>> GetAllAsync(Guid? artId, Guid? organisationId);

        Task<List<SchoolStudentDTO>> GetStudentsAsync(Guid schoolId);

        Task<AddressDTO> AddNewAddressToSchoolAsync(Guid schoolId, CreateAddressDTO createAddressDTO);

        Task RemoveAddressFromSchoolAsync(Guid schoolId, Guid addressId);

        Task ChangeSchoolOrganisationAsync(Guid schoolId, Guid organisationId);

        Task ChangeSchoolArtAsync(Guid schoolId, Guid artId);

        Task ChangeSchoolHeadInstructorAsync(Guid schoolId, Guid studentId, bool retainSecretary);

        Task<PersonDTO> AddNewPersonToSchoolAsync(Guid schoolId, CreatePersonInternalDTO createPersonDTO, bool isInstructor = false, bool isSecretary = false);

        Task AddExistingPersonToSchoolAsync(Guid schoolId, Guid studentId, bool isInstructor = false, bool isSecretary = false);

        Task<DocumentDTO> GetStudentInsuranceAsync(Guid schoolId, Guid studentId);

        Task<DocumentDTO> AddStudentInsuranceAsync(Guid schoolId, Guid studentId, CreateDocumentInternalDTO createDocumentDTO, bool archiveExisting = true);

        Task<DocumentDTO> GetStudentLicenceAsync(Guid schoolId, Guid studentId);

        Task<DocumentDTO> AddStudentLicenceAsync(Guid schoolId, Guid studentId, CreateDocumentInternalDTO createDocumentDTO, bool archiveExisting = true);

        Task RemoveStudentFromSchoolAsync(Guid schoolId, Guid studentId);

        Task<bool> SchoolHasStudentAsync(Guid schoolId, Guid studentId);

        Task<bool> SchoolHasInstructorAsync(Guid schoolId, Guid studentId);

        Task<bool> SchoolHasSecretaryAsync(Guid schoolId, Guid studentId);

        Task<Guid?> GetHeadInstructorIdAsync(Guid schoolId);

        Task<List<StudentSchoolDTO>> GetSchoolsForPersonAsync(Guid personId);
    }
}
