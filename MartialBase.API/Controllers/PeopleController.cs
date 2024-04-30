// <copyright file="PeopleController.cs" company="Martialtech®">
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
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.Enums;
using MartialBase.API.Tools;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace MartialBase.API.Controllers
{
    /// <summary>
    /// Endpoints used to retrieve and manage <see cref="Person">People</see>.
    /// </summary>
    /// <remarks>
    /// All endpoints require the requesting user be either an
    /// <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> of the <see cref="Organisation"/> or a
    /// <see cref="UserRoles.SchoolSecretary">SchoolSecretary</see> of the <see cref="School"/> to which the
    /// <see cref="Person"/> belongs.
    /// </remarks>
    [Authorize]
    [Route("people")]
    [RequiredScope("query")]
    public class PeopleController : MartialBaseControllerBase
    {
        private readonly ICountriesRepository _countriesRepository;
        private readonly IMartialBaseUserHelper _martialBaseUserHelper;
        private readonly IMartialBaseUsersRepository _martialBaseUsersRepository;
        private readonly IOrganisationsRepository _organisationsRepository;
        private readonly IPeopleRepository _peopleRepository;
        private readonly ISchoolsRepository _schoolsRepository;
        private readonly IAzureUserHelper _azureUserHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeopleController"/> class.
        /// </summary>
        /// <param name="countriesRepository">The current <see cref="ICountriesRepository"/> instance.</param>
        /// <param name="martialBaseUserHelper">The current <see cref="IMartialBaseUserHelper"/> instance.</param>
        /// <param name="martialBaseUsersRepository">The current <see cref="IMartialBaseUsersRepository"/> instance.</param>
        /// <param name="organisationsRepository">The current <see cref="IOrganisationsRepository"/> instance.</param>
        /// <param name="peopleRepository">The current <see cref="IPeopleRepository"/> instance.</param>
        /// <param name="schoolsRepository">The current <see cref="ISchoolsRepository"/> instance.</param>
        /// <param name="azureUserHelper">The current <see cref="IAzureUserHelper"/> instance.</param>
        /// <param name="hostEnvironment">The current <see cref="IWebHostEnvironment"/> instance.</param>
        public PeopleController(
            ICountriesRepository countriesRepository,
            IMartialBaseUserHelper martialBaseUserHelper,
            IMartialBaseUsersRepository martialBaseUsersRepository,
            IOrganisationsRepository organisationsRepository,
            IPeopleRepository peopleRepository,
            ISchoolsRepository schoolsRepository,
            IAzureUserHelper azureUserHelper,
            IWebHostEnvironment hostEnvironment)
        {
            _countriesRepository = countriesRepository;
            _martialBaseUserHelper = martialBaseUserHelper;
            _martialBaseUsersRepository = martialBaseUsersRepository;
            _organisationsRepository = organisationsRepository;
            _peopleRepository = peopleRepository;
            _schoolsRepository = schoolsRepository;
            _azureUserHelper = azureUserHelper;
            HostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for the current requesting user's <see cref="Person"/> ID. This is obtained according to the
        /// Azure user ID contained in the auth token.
        /// </summary>
        /// <remarks>
        /// Requesting user must have an auth token but no <see cref="MartialBaseUserRole"/> is required.
        /// </remarks>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing the <see cref="Person"/> ID of the current requesting user in the form of a <c>Guid?</c>.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// </list>
        /// </returns>
        [HttpGet("getmyid")]
        [ProducesResponseType(typeof(Guid?), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPersonIdForCurrentMartialBaseUserAsync()
        {
            try
            {
                (Guid azureId, string invitationCode) = _azureUserHelper.GetAzureIdAndInvitationCodeFromHttpRequest(Request);

                return Ok(await _azureUserHelper.GetPersonIdForAzureUserAsync(azureId, invitationCode));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for a specified <see cref="MartialBaseUser"/>'s <see cref="Person"/> ID.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in the <see cref="UserRoles.SystemAdmin">SystemAdmin</see>
        /// role for this endpoint.
        /// </remarks>
        /// <param name="userId">The ID of the <see cref="MartialBaseUser"/> to return the <see cref="Person"/> ID for.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing the <see cref="Person"/> ID of the specified <see cref="MartialBaseUser"/> in the form of a <c>Guid?</c>.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.SystemAdmin">SystemAdmin</see> role.</item>
        /// <item>A <see cref="BadRequestResult"/> if no <paramref name="userId"/> parameter is provided.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="userId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpGet("getid")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(Guid?), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPersonIdForMartialBaseUserAsync(string userId)
        {
            try
            {
                if (!RequestingUserRoles.Any(role => role is UserRoles.SystemAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                if (userId == null)
                {
                    return BadRequest("No user ID parameter specified.");
                }

                var martialBaseUserIdGuid = new Guid(userId);

                if (!await _martialBaseUsersRepository.ExistsAsync(martialBaseUserIdGuid))
                {
                    throw new EntityIdNotFoundException("User", userId);
                }

                return Ok(await _martialBaseUsersRepository.GetPersonIdForUserAsync(martialBaseUserIdGuid));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("{personId}")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(PersonDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPersonAsync(string personId)
        {
            try
            {
                var personIdGuid = new Guid(personId);

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Person", personIdGuid);
                }

                await _martialBaseUserHelper.ValidateUserHasAccessToPersonAsync(RequestingPersonId, personIdGuid);

                return Ok(await _peopleRepository.GetAsync(personIdGuid));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(List<PersonDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindPeopleAsync(string email, string firstName, string middleName, string lastName, string returnAddresses)
        {
            try
            {
                if (!RequestingUserRoles.Any(role => role is UserRoles.SystemAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                if (email == null && firstName == null && middleName == null && lastName == null)
                {
                    return BadRequest(ErrorResponseCode.NoSearchParameters);
                }

                bool returnAddressesBool = false;

                if (returnAddresses != null)
                {
                    try
                    {
                        returnAddressesBool = Convert.ToBoolean(returnAddresses);
                    }
                    catch
                    {
                        return BadRequest("Invalid value provided for returnAddresses parameter.");
                    }
                }

                return Ok(await _peopleRepository.FindAsync(email, firstName, middleName, lastName, returnAddressesBool));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(CreatedPersonDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreatePersonAsync([FromBody] CreatePersonDTO createPersonDTO, string organisationId, string schoolId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                if (organisationId == null && schoolId == null)
                {
                    return BadRequest("No organisation or school ID provided for person.");
                }

                Guid? organisationIdGuid = organisationId != null ? new Guid(organisationId) : null;
                Guid? schoolIdGuid = schoolId != null ? new Guid(schoolId) : null;

                if (organisationId != null && schoolId == null)
                {
                    await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                        RequestingPersonId, (Guid)organisationIdGuid);
                }
                else
                {
                    await _martialBaseUserHelper.ValidateUserHasAdminAccessToSchoolAsync(
                        RequestingPersonId, (Guid)schoolIdGuid);
                }

                if (!_countriesRepository.Exists(createPersonDTO.Address.CountryCode))
                {
                    throw new EntityNotFoundException(
                        $"Country code '{createPersonDTO.Address.CountryCode}' not found.");
                }

                var createPersonInternalDTO = new CreatePersonInternalDTO
                {
                    Title = createPersonDTO.Title,
                    FirstName = createPersonDTO.FirstName,
                    MiddleName = createPersonDTO.MiddleName,
                    LastName = createPersonDTO.LastName,
                    DateOfBirth = Convert.ToDateTime(createPersonDTO.DateOfBirth),
                    Address = new CreateAddressDTO
                    {
                        Line1 = createPersonDTO.Address.Line1,
                        Line2 = createPersonDTO.Address.Line2,
                        Line3 = createPersonDTO.Address.Line3,
                        Town = createPersonDTO.Address.Town,
                        County = createPersonDTO.Address.County,
                        PostCode = createPersonDTO.Address.PostCode,
                        CountryCode = createPersonDTO.Address.CountryCode,
                        LandlinePhone = createPersonDTO.Address.LandlinePhone
                    },
                    MobileNo = createPersonDTO.MobileNo,
                    Email = createPersonDTO.Email
                };

                CreatedPersonDTO createdPerson =
                    await _peopleRepository.CreateAsync(createPersonInternalDTO, organisationIdGuid, schoolIdGuid);

                if (!await _peopleRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return StatusCode(StatusCodes.Status201Created, createdPerson);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPut("{personId}")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(PersonDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePersonAsync(string personId, [FromBody] UpdatePersonDTO updatePersonDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAccessToPersonAsync(RequestingPersonId, personIdGuid);

                if (!_countriesRepository.Exists(updatePersonDTO.Address.CountryCode))
                {
                    throw new EntityNotFoundException(
                        $"Country code '{updatePersonDTO.Address.CountryCode}' not found.");
                }

                PersonDTO updatedPerson = await _peopleRepository.UpdateAsync(personIdGuid, updatePersonDTO);

                if (!await _peopleRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return Ok(updatedPerson);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("{personId}/organisations")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(List<PersonOrganisationDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPersonOrganisationsAsync(string personId)
        {
            try
            {
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAccessToPersonAsync(
                    RequestingPersonId, personIdGuid);

                List<PersonOrganisationDTO> personOrganisations =
                    await _organisationsRepository.GetOrganisationsForPersonAsync(personIdGuid);

                var personOrganisationsAuthorized = new List<PersonOrganisationDTO>();

                if (RequestingUserRoles.Contains(UserRoles.Thanos))
                {
                    personOrganisationsAuthorized = personOrganisations;
                }
                else
                {
                    foreach (PersonOrganisationDTO personOrganisation in personOrganisations)
                    {
                        if (await _organisationsRepository.PersonCanAccessOrganisationAsync(
                                new Guid(personOrganisation.Organisation.Id), RequestingPersonId) ||
                             RequestingPersonId == personIdGuid)
                        {
                            personOrganisationsAuthorized.Add(personOrganisation);
                        }
                    }
                }

                return Ok(personOrganisationsAuthorized);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("{personId}/schools")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(List<StudentSchoolDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPersonSchoolsAsync(string personId)
        {
            try
            {
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAccessToPersonAsync(
                    RequestingPersonId, personIdGuid);

                List<StudentSchoolDTO> personSchools =
                    await _schoolsRepository.GetSchoolsForPersonAsync(personIdGuid);

                var personSchoolsAuthorized = new List<StudentSchoolDTO>();

                if (RequestingUserRoles.Contains(UserRoles.Thanos))
                {
                    personSchoolsAuthorized = personSchools;
                }
                else
                {
                    foreach (StudentSchoolDTO personSchool in personSchools)
                    {
                        if (await _schoolsRepository.SchoolHasStudentAsync(
                                 personSchool.School.Id, RequestingPersonId) ||
                             RequestingPersonId == personIdGuid)
                        {
                            personSchoolsAuthorized.Add(personSchool);
                        }
                    }
                }

                return Ok(personSchoolsAuthorized);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpDelete("{personId}")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeletePersonAsync(string personId)
        {
            try
            {
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAccessToPersonAsync(RequestingPersonId, personIdGuid);

                await _peopleRepository.DeleteAsync(personIdGuid);

                if (!await _peopleRepository.SaveChangesAsync())
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
