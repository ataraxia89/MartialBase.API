// <copyright file="SchoolsController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Net;

using MartialBase.API.AuthTools.Attributes;
using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Exceptions;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Documents;
using MartialBase.API.Data.Models.InternalDTOs.Schools;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.DTOs.Schools;
using MartialBase.API.Models.Enums;
using MartialBase.API.Tools;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace MartialBase.API.Controllers
{
    [Authorize]
    [PersonIdRequiredAsync]
    [Route("schools")]
    [RequiredScope("query")]
    public class SchoolsController : MartialBaseControllerBase
    {
        private readonly IAddressesRepository _addressesRepository;
        private readonly IArtsRepository _artsRepository;
        private readonly ICountriesRepository _countriesRepository;
        private readonly IDocumentsRepository _documentsRepository;
        private readonly IDocumentTypesRepository _documentTypesRepository;
        private readonly IMartialBaseUserHelper _martialBaseUserHelper;
        private readonly IOrganisationsRepository _organisationsRepository;
        private readonly IPeopleRepository _peopleRepository;
        private readonly ISchoolsRepository _schoolsRepository;

        public SchoolsController(
            IAddressesRepository addressesRepository,
            IArtsRepository artsRepository,
            ICountriesRepository countriesRepository,
            IDocumentsRepository documentsRepository,
            IDocumentTypesRepository documentTypesRepository,
            IMartialBaseUserHelper martialBaseUserHelper,
            IOrganisationsRepository organisationsRepository,
            IPeopleRepository peopleRepository,
            ISchoolsRepository schoolsRepository,
            IWebHostEnvironment hostEnvironment)
        {
            _addressesRepository = addressesRepository;
            _artsRepository = artsRepository;
            _countriesRepository = countriesRepository;
            _documentsRepository = documentsRepository;
            _documentTypesRepository = documentTypesRepository;
            _martialBaseUserHelper = martialBaseUserHelper;
            _organisationsRepository = organisationsRepository;
            _peopleRepository = peopleRepository;
            _schoolsRepository = schoolsRepository;
            HostEnvironment = hostEnvironment;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<SchoolDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSchoolsAsync(string artId, string organisationId)
        {
            try
            {
                if (!RequestingUserRoles.Any(
                        role =>
                            role is UserRoles.SchoolMember
                                or UserRoles.SchoolInstructor
                                or UserRoles.SchoolHeadInstructor
                                or UserRoles.SchoolSecretary
                                or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(
                        ErrorResponseCode.InsufficientUserRole,
                        HttpStatusCode.Forbidden);
                }

                Guid? artIdGuid = null;
                Guid? organisationIdGuid = null;

                if (artId != null)
                {
                    artIdGuid = new Guid(artId);

                    if (!await _artsRepository.ExistsAsync((Guid)artIdGuid))
                    {
                        throw new EntityIdNotFoundException("Art", (Guid)artIdGuid);
                    }
                }

                if (organisationId != null)
                {
                    organisationIdGuid = new Guid(organisationId);

                    if (!await _organisationsRepository.ExistsAsync((Guid)organisationIdGuid))
                    {
                        throw new EntityIdNotFoundException("Organisation", (Guid)organisationIdGuid);
                    }
                }

                List<SchoolDTO> schools = await _schoolsRepository.GetAllAsync(artIdGuid, organisationIdGuid);

                var allowedSchools = new List<SchoolDTO>();

                if (RequestingUserRoles.Contains(UserRoles.Thanos))
                {
                    allowedSchools = schools;
                }
                else
                {
                    foreach (SchoolDTO school in schools)
                    {
                        if (await _schoolsRepository.SchoolHasStudentAsync(school.Id, RequestingPersonId))
                        {
                            allowedSchools.Add(school);
                        }
                    }
                }

                return Ok(allowedSchools);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("{schoolId}")]
        [ProducesResponseType(typeof(SchoolDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSchoolAsync(string schoolId)
        {
            try
            {
                var schoolIdGuid = new Guid(schoolId);

                await _martialBaseUserHelper.ValidateUserHasMemberAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                return Ok(await _schoolsRepository.GetAsync(schoolIdGuid));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(SchoolDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateSchoolAsync([FromBody] CreateSchoolDTO createSchoolDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                if (!RequestingUserRoles.Any(role =>
                        role is UserRoles.OrganisationAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                var artId = new Guid(createSchoolDTO.ArtId);
                Guid? headInstructorId = null;

                if (!await _artsRepository.ExistsAsync(artId))
                {
                    throw new EntityIdNotFoundException("Art", artId);
                }

                var organisationId = new Guid(createSchoolDTO.OrganisationId);

                if (!await _organisationsRepository.ExistsAsync(organisationId))
                {
                    throw new EntityIdNotFoundException("Organisation", organisationId);
                }

                if (!RequestingUserRoles.Contains(UserRoles.Thanos))
                {
                    if (!await _organisationsRepository.PersonIsOrganisationAdminAsync(
                            organisationId, RequestingPersonId))
                    {
                        throw new ErrorResponseCodeException(
                            ErrorResponseCode.NotOrganisationAdmin,
                            HttpStatusCode.Forbidden);
                    }
                }

                if (createSchoolDTO.HeadInstructorId != null)
                {
                    headInstructorId = new Guid(createSchoolDTO.HeadInstructorId);

                    if (!await _peopleRepository.ExistsAsync((Guid)headInstructorId))
                    {
                        throw new EntityIdNotFoundException("Head instructor person", (Guid)headInstructorId);
                    }
                }

                if (!_countriesRepository.Exists(createSchoolDTO.Address.CountryCode))
                {
                    throw new EntityNotFoundException(
                        $"Country code '{createSchoolDTO.Address.CountryCode}' not found.");
                }

                if (createSchoolDTO.AdditionalTrainingVenues != null)
                {
                    foreach (CreateAddressDTO schoolAddress in createSchoolDTO.AdditionalTrainingVenues)
                    {
                        if (!_countriesRepository.Exists(schoolAddress.CountryCode))
                        {
                            throw new EntityNotFoundException(
                                $"Country code '{schoolAddress.CountryCode}' not found.");
                        }
                    }
                }

                var createSchoolInternalDTO = new CreateSchoolInternalDTO
                {
                    ArtId = artId,
                    OrganisationId = organisationId,
                    Name = createSchoolDTO.Name,
                    HeadInstructorId = headInstructorId,
                    Address = createSchoolDTO.Address,
                    PhoneNo = createSchoolDTO.PhoneNo,
                    EmailAddress = createSchoolDTO.EmailAddress,
                    WebsiteURL = createSchoolDTO.WebsiteURL,
                    AdditionalTrainingVenues = createSchoolDTO.AdditionalTrainingVenues
                };

                SchoolDTO createdSchool = await _schoolsRepository.CreateAsync(createSchoolInternalDTO);

                if (!await _schoolsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return StatusCode(StatusCodes.Status201Created, createdSchool);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPut("{schoolId}")]
        [ProducesResponseType(typeof(SchoolDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSchoolAsync(string schoolId, [FromBody] UpdateSchoolDTO updateSchoolDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                var schoolIdGuid = new Guid(schoolId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!_countriesRepository.Exists(updateSchoolDTO.Address.CountryCode))
                {
                    throw new EntityNotFoundException(
                        $"Country code '{updateSchoolDTO.Address.CountryCode}' not found.");
                }

                if (updateSchoolDTO.AdditionalTrainingVenues != null)
                {
                    foreach (KeyValuePair<string, UpdateAddressDTO> schoolAddress in updateSchoolDTO.AdditionalTrainingVenues)
                    {
                        var addressId = new Guid(schoolAddress.Key);

                        if (!await _addressesRepository.ExistsAsync(addressId))
                        {
                            throw new EntityIdNotFoundException("School address", addressId);
                        }

                        if (!_countriesRepository.Exists(schoolAddress.Value.CountryCode))
                        {
                            throw new EntityNotFoundException(
                                $"Country code '{schoolAddress.Value.CountryCode}' not found.");
                        }
                    }
                }

                SchoolDTO updatedSchool = await _schoolsRepository.UpdateAsync(schoolIdGuid, updateSchoolDTO);

                if (!await _schoolsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return Ok(updatedSchool);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("{schoolId}/addresses")]
        [ProducesResponseType(typeof(AddressDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> AddNewAddressToSchoolAsync(string schoolId, [FromBody] CreateAddressDTO createAddressDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                var schoolIdGuid = new Guid(schoolId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!_countriesRepository.Exists(createAddressDTO.CountryCode))
                {
                    throw new EntityNotFoundException($"Country code '{createAddressDTO.CountryCode}' not found.");
                }

                AddressDTO addedAddress =
                    await _schoolsRepository.AddNewAddressToSchoolAsync(schoolIdGuid, createAddressDTO);

                if (!await _schoolsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return StatusCode(StatusCodes.Status201Created, addedAddress);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpDelete("{schoolId}/addresses/{addressId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveAddressFromSchoolAsync(string schoolId, string addressId)
        {
            try
            {
                var schoolIdGuid = new Guid(schoolId);
                var addressIdGuid = new Guid(addressId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!await _addressesRepository.ExistsAsync(addressIdGuid))
                {
                    throw new EntityIdNotFoundException("Address", addressIdGuid);
                }

                if (!await _schoolsRepository.SchoolAddressExistsAsync(schoolIdGuid, addressIdGuid))
                {
                    throw new EntityNotFoundException(
                        $"Address '{addressIdGuid}' does not currently belong to school '{schoolIdGuid}'");
                }

                await _schoolsRepository.RemoveAddressFromSchoolAsync(schoolIdGuid, addressIdGuid);

                if (!await _schoolsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("{schoolId}/students")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddStudentToSchoolAsync(string schoolId, string personId, string isInstructor, string isSecretary)
        {
            try
            {
                if (personId == null)
                {
                    return BadRequest("No person ID parameter specified.");
                }

                bool isInstructorBool = false;
                bool isSecretaryBool = false;

                if (isInstructor != null)
                {
                    try
                    {
                        isInstructorBool = Convert.ToBoolean(isInstructor);
                    }
                    catch
                    {
                        return BadRequest("Invalid value provided for isInstructor parameter.");
                    }
                }

                if (isSecretary != null)
                {
                    try
                    {
                        isSecretaryBool = Convert.ToBoolean(isSecretary);
                    }
                    catch
                    {
                        return BadRequest("Invalid value provided for isSecretary parameter.");
                    }
                }

                var schoolIdGuid = new Guid(schoolId);
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Person", personIdGuid);
                }

                await _schoolsRepository.AddExistingPersonToSchoolAsync(
                    schoolIdGuid, personIdGuid, isInstructorBool, isSecretaryBool);

                if (!await _schoolsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return StatusCode(StatusCodes.Status201Created);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("{schoolId}/students/{personId}/insurance")]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSchoolStudentInsuranceAsync(string schoolId, string personId)
        {
            try
            {
                var schoolIdGuid = new Guid(schoolId);
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Person", personIdGuid);
                }

                return Ok(await _schoolsRepository.GetStudentInsuranceAsync(schoolIdGuid, personIdGuid));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("{schoolId}/students/{personId}/insurance")]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSchoolStudentInsuranceAsync(string schoolId, string personId, [FromBody] CreateDocumentDTO createDocumentDTO, string archiveExisting)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                bool archiveExistingBool = true;

                if (archiveExisting != null)
                {
                    try
                    {
                        archiveExistingBool = Convert.ToBoolean(archiveExisting);
                    }
                    catch
                    {
                        return BadRequest("Invalid value provided for archiveExisting parameter.");
                    }
                }

                var schoolIdGuid = new Guid(schoolId);
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(RequestingPersonId, schoolIdGuid);

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Person", personIdGuid);
                }

                var documentTypeId = new Guid(createDocumentDTO.DocumentTypeId);

                if (!await _documentTypesRepository.ExistsAsync(documentTypeId))
                {
                    throw new EntityIdNotFoundException("Document type", documentTypeId);
                }

                var createDocumentInternalDTO = new CreateDocumentInternalDTO
                {
                    DocumentTypeId = documentTypeId,
                    DocumentDate = createDocumentDTO.DocumentDate,
                    Reference = createDocumentDTO.Reference,
                    URL = createDocumentDTO.URL,
                    ExpiryDate = createDocumentDTO.ExpiryDate
                };

                DocumentDTO createdDocument = await _schoolsRepository.AddStudentInsuranceAsync(
                    schoolIdGuid, personIdGuid, createDocumentInternalDTO, archiveExistingBool);

                if (!await _schoolsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return StatusCode(StatusCodes.Status201Created, createdDocument);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("{schoolId}/students/{personId}/licence")]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSchoolStudentLicenceAsync(string schoolId, string personId)
        {
            try
            {
                var schoolIdGuid = new Guid(schoolId);
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Person", personIdGuid);
                }

                return Ok(await _schoolsRepository.GetStudentLicenceAsync(schoolIdGuid, personIdGuid));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("{schoolId}/students/{personId}/licence")]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSchoolStudentLicenceAsync(string schoolId, string personId, [FromBody] CreateDocumentDTO createDocumentDTO, string archiveExisting)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                bool archiveExistingBool = true;

                if (archiveExisting != null)
                {
                    try
                    {
                        archiveExistingBool = Convert.ToBoolean(archiveExisting);
                    }
                    catch
                    {
                        return BadRequest("Invalid value provided for archiveExisting parameter.");
                    }
                }

                var schoolIdGuid = new Guid(schoolId);
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Person", personIdGuid);
                }

                var documentTypeId = new Guid(createDocumentDTO.DocumentTypeId);

                if (!await _documentTypesRepository.ExistsAsync(documentTypeId))
                {
                    throw new EntityIdNotFoundException("Document type", documentTypeId);
                }

                var createDocumentInternalDTO = new CreateDocumentInternalDTO
                {
                    DocumentTypeId = documentTypeId,
                    DocumentDate = createDocumentDTO.DocumentDate,
                    Reference = createDocumentDTO.Reference,
                    URL = createDocumentDTO.URL,
                    ExpiryDate = createDocumentDTO.ExpiryDate
                };

                DocumentDTO createdDocument = await _schoolsRepository.AddStudentLicenceAsync(
                    schoolIdGuid, personIdGuid, createDocumentInternalDTO, archiveExistingBool);

                if (!await _schoolsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return StatusCode(StatusCodes.Status201Created, createdDocument);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.5">POST</see>
        /// request to create a new <see cref="Document"/> and assign it to an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="schoolId">The ID of the <see cref="School"/> which the <see cref="Person"/> is a student of.</param>
        /// <param name="personId">The ID of the <see cref="Person">Student</see> to which the new <see cref="Document"/> is to be assigned.</param>
        /// <param name="createDocumentDTO">The <see cref="CreateDocumentDTO"/> model to use to create the new <see cref="Document"/>.</param>
        /// <returns>
        /// <para>A <see cref="CreatedResult"/> containing a <see cref="DocumentDTO"/> object.</para>
        /// <para>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if the <see cref="CreateDocumentDTO"/> model state is not valid. Model state errors are returned in the response body.</para>
        /// <para>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</para>
        /// <para>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</para>
        /// <para>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</para>
        /// <para>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see> role.</para>
        /// <para>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not a secretary of the requested <see cref="School"/>.</para>
        /// <para>A <see cref="NotFoundResult"/> if the provided <paramref name="schoolId"/> or <paramref name="personId"/> cannot be found.</para>
        /// <para>A <see cref="NotFoundResult"/> if the provided <see cref="CreateDocumentDTO"/> contains an invalid <see cref="DocumentType"/> ID.</para>
        /// </returns>
        [HttpPost("{schoolId}/students/{personId}/documents")]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateStudentDocumentAsync(string schoolId, string personId, [FromBody] CreateDocumentDTO createDocumentDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                var schoolIdGuid = new Guid(schoolId);
                var personIdGuid = new Guid(personId);
                var documentTypeIdGuid = new Guid(createDocumentDTO.DocumentTypeId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!await _documentTypesRepository.ExistsAsync(documentTypeIdGuid))
                {
                    throw new EntityIdNotFoundException("Document type", documentTypeIdGuid);
                }

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Person", personIdGuid);
                }

                if (!await _schoolsRepository.SchoolHasStudentAsync(schoolIdGuid, personIdGuid))
                {
                    throw new EntityNotFoundException(
                        $"Person '{personIdGuid}' not found in school '{schoolIdGuid}'");
                }

                var createDocumentInternalDTO = new CreateDocumentInternalDTO
                {
                    DocumentDate = createDocumentDTO.DocumentDate,
                    DocumentTypeId = documentTypeIdGuid,
                    ExpiryDate = createDocumentDTO.ExpiryDate,
                    Reference = createDocumentDTO.Reference,
                    URL = createDocumentDTO.URL
                };

                DocumentDTO createdDocument =
                    await _peopleRepository.CreatePersonDocumentAsync(personIdGuid, createDocumentInternalDTO);

                if (!await _peopleRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return StatusCode(StatusCodes.Status201Created, createdDocument);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see> request
        /// for a <see cref="Document"/> which is assigned to a <see cref="Person"/> who is the student of a
        /// <see cref="School"/>.
        /// </summary>
        /// <param name="schoolId">The ID of the <see cref="School"/> which the <see cref="Person"/> is a student of.</param>
        /// <param name="personId">The ID of the <see cref="Person">Student</see> to which the requested <see cref="Document"/> is assigned.</param>
        /// <param name="documentId">The ID of the <see cref="Document"/> to be returned.</param>
        /// <returns>
        /// <para>An <see cref="OkResult"/> containing a <see cref="DocumentDTO"/> object.</para>
        /// <para>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</para>
        /// <para>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</para>
        /// <para>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</para>
        /// <para>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see> role.</para>
        /// <para>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not a secretary of the requested <see cref="School"/>.</para>
        /// <para>A <see cref="NotFoundResult"/> if the provided <paramref name="schoolId"/>, <paramref name="personId"/> or <paramref name="documentId"/> cannot be found.</para>
        /// </returns>
        [HttpGet("{schoolId}/students/{personId}/documents/{documentId}")]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStudentDocumentAsync(string schoolId, string personId, string documentId)
        {
            try
            {
                var schoolIdGuid = new Guid(schoolId);
                var personIdGuid = new Guid(personId);
                var documentIdGuid = new Guid(documentId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Person", personIdGuid);
                }

                if (!await _documentsRepository.ExistsAsync(documentIdGuid))
                {
                    throw new EntityIdNotFoundException("Document", documentIdGuid);
                }

                if (!await _schoolsRepository.SchoolHasStudentAsync(schoolIdGuid, personIdGuid))
                {
                    throw new EntityNotFoundException(
                        $"Person '{personIdGuid}' not found in school '{schoolIdGuid}'");
                }

                return Ok(await _peopleRepository.GetPersonDocumentAsync(personIdGuid, documentIdGuid));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see> request
        /// for a list of <see cref="Document">Documents</see> which are assigned to a <see cref="Person"/> who is
        /// the student of a <see cref="School"/>.
        /// </summary>
        /// <param name="schoolId">The ID of the <see cref="School"/> which the <see cref="Person"/> is a student of.</param>
        /// <param name="personId">The ID of the <see cref="Person">Student</see> to which the requested <see cref="Document"/> is assigned.</param>
        /// <param name="includeInactive">(Optional query parameter) Whether to include and return inactive documents.</param>
        /// <returns>
        /// <para>An <see cref="OkResult"/> containing a <see cref="List{T}"/> of <see cref="DocumentDTO"/> objects.</para>
        /// <para>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</para>
        /// <para>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</para>
        /// <para>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</para>
        /// <para>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see> role.</para>
        /// <para>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not a secretary of the requested <see cref="School"/>.</para>
        /// <para>A <see cref="NotFoundResult"/> if the provided <paramref name="schoolId"/> or <paramref name="personId"/> cannot be found.</para>
        /// </returns>
        [HttpGet("{schoolId}/students/{personId}/documents")]
        [ProducesResponseType(typeof(List<DocumentDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStudentDocumentsAsync(string schoolId, string personId, string includeInactive)
        {
            try
            {
                bool includeInactiveBool = false;

                if (includeInactive != null)
                {
                    try
                    {
                        includeInactiveBool = Convert.ToBoolean(includeInactive);
                    }
                    catch
                    {
                        return BadRequest("Invalid value provided for includeInactive parameter.");
                    }
                }

                var schoolIdGuid = new Guid(schoolId);
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Person", personIdGuid);
                }

                if (!await _schoolsRepository.SchoolHasStudentAsync(schoolIdGuid, personIdGuid))
                {
                    throw new EntityNotFoundException(
                        $"Person '{personIdGuid}' not found in school '{schoolIdGuid}'");
                }

                return Ok(await _peopleRepository.GetPersonDocumentsAsync(personIdGuid, includeInactiveBool));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpDelete("{schoolId}/students/{personId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveStudentFromSchoolAsync(string schoolId, string personId)
        {
            try
            {
                var schoolIdGuid = new Guid(schoolId);
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Person", personIdGuid);
                }

                if (await _schoolsRepository.GetHeadInstructorIdAsync(schoolIdGuid) == personIdGuid)
                {
                    return BadRequest(
                        "Person is currently designated as head instructor, change head instructor before removing person from school.");
                }

                await _schoolsRepository.RemoveStudentFromSchoolAsync(schoolIdGuid, personIdGuid);

                if (!await _schoolsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("{schoolId}/students")]
        [ProducesResponseType(typeof(List<SchoolStudentDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSchoolStudentsAsync(string schoolId)
        {
            try
            {
                var schoolIdGuid = new Guid(schoolId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                return Ok(await _schoolsRepository.GetStudentsAsync(schoolIdGuid));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPut("{schoolId}/organisation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeSchoolOrganisationAsync(string schoolId, string organisationId)
        {
            try
            {
                if (organisationId == null)
                {
                    return BadRequest("No organisation ID parameter specified.");
                }

                var schoolIdGuid = new Guid(schoolId);
                var organisationIdGuid = new Guid(organisationId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!await _organisationsRepository.ExistsAsync(organisationIdGuid))
                {
                    throw new EntityIdNotFoundException("Organisation", organisationIdGuid);
                }

                await _schoolsRepository.ChangeSchoolOrganisationAsync(schoolIdGuid, organisationIdGuid);

                if (!await _schoolsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPut("{schoolId}/headinstructor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeSchoolHeadInstructorAsync(string schoolId, string personId, string retainSecretary)
        {
            try
            {
                bool retainSecretaryBool;

                if (personId == null)
                {
                    return BadRequest("No person ID parameter specified.");
                }

                if (retainSecretary == null)
                {
                    return BadRequest("No retain secretary parameter specified.");
                }

                try
                {
                    retainSecretaryBool = Convert.ToBoolean(retainSecretary);
                }
                catch
                {
                    return BadRequest("Invalid value provided for retainSecretary parameter.");
                }

                var schoolIdGuid = new Guid(schoolId);
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Head instructor person", personIdGuid);
                }

                await _schoolsRepository.ChangeSchoolHeadInstructorAsync(schoolIdGuid, personIdGuid, retainSecretaryBool);

                if (!await _schoolsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPut("{schoolId}/art")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeSchoolArtAsync(string schoolId, string artId)
        {
            try
            {
                var schoolIdGuid = new Guid(schoolId);
                var artIdGuid = new Guid(artId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                if (!await _artsRepository.ExistsAsync(artIdGuid))
                {
                    throw new EntityIdNotFoundException("Art", artIdGuid);
                }

                await _schoolsRepository.ChangeSchoolArtAsync(schoolIdGuid, artIdGuid);

                if (!await _schoolsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpDelete("{schoolId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteSchoolAsync(string schoolId)
        {
            try
            {
                var schoolIdGuid = new Guid(schoolId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                    RequestingPersonId,
                    schoolIdGuid);

                await _schoolsRepository.DeleteAsync(schoolIdGuid);

                if (!await _schoolsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
