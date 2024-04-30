// <copyright file="OrganisationsController.cs" company="Martialtech®">
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
using MartialBase.API.Data.Models.InternalDTOs.Organisations;
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.DocumentTypes;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.Enums;
using MartialBase.API.Tools;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace MartialBase.API.Controllers
{
    /// <summary>
    /// Endpoints used to retrieve <see cref="Organisation">Organisations</see>.
    /// </summary>
    /// <remarks>
    /// All endpoints require the requesting user be either an
    /// <see cref="UserRoles.OrganisationMember">OrganisationMember</see> or
    /// <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> of the requested <see cref="Organisation"/>,
    /// and only admins and super users can manage <see cref="Organisation">Organisations</see>.
    /// </remarks>
    [Authorize]
    [Route("organisations")]
    [RequiredScope("query")]
    public class OrganisationsController : MartialBaseControllerBase
    {
        private readonly ICountriesRepository _countriesRepository;
        private readonly IDocumentTypesRepository _documentTypesRepository;
        private readonly IMartialBaseUserHelper _martialBaseUserHelper;
        private readonly IMartialBaseUserRolesRepository _martialBaseUserRolesRepository;
        private readonly IOrganisationsRepository _organisationsRepository;
        private readonly IPeopleRepository _peopleRepository;
        private readonly IAzureUserHelper _azureUserHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationsController"/> class.
        /// </summary>
        /// <param name="countriesRepository">The current <see cref="ICountriesRepository"/> instance.</param>
        /// <param name="documentTypesRepository">The current <see cref="IDocumentTypesRepository"/> instance.</param>
        /// <param name="martialBaseUserHelper">The current <see cref="IMartialBaseUserHelper"/> instance.</param>
        /// <param name="martialBaseUserRolesRepository">The current <see cref="IMartialBaseUserRolesRepository"/> instance.</param>
        /// <param name="organisationsRepository">The current <see cref="IOrganisationsRepository"/> instance.</param>
        /// <param name="peopleRepository">The current <see cref="IPeopleRepository"/> instance.</param>
        /// <param name="azureUserHelper">The current <see cref="IAzureUserHelper"/> instance.</param>
        /// <param name="hostEnvironment">The current <see cref="IWebHostEnvironment"/> instance.</param>
        public OrganisationsController(
            ICountriesRepository countriesRepository,
            IDocumentTypesRepository documentTypesRepository,
            IMartialBaseUserHelper martialBaseUserHelper,
            IMartialBaseUserRolesRepository martialBaseUserRolesRepository,
            IOrganisationsRepository organisationsRepository,
            IPeopleRepository peopleRepository,
            IAzureUserHelper azureUserHelper,
            IWebHostEnvironment hostEnvironment)
        {
            _countriesRepository = countriesRepository;
            _documentTypesRepository = documentTypesRepository;
            _martialBaseUserHelper = martialBaseUserHelper;
            _martialBaseUserRolesRepository = martialBaseUserRolesRepository;
            _organisationsRepository = organisationsRepository;
            _peopleRepository = peopleRepository;
            _azureUserHelper = azureUserHelper;
            HostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for a list of <see cref="Organisation">Organisations</see>.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Requesting user must be in either the <see cref="UserRoles.OrganisationMember">OrganisationMember</see> or <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for this endpoint.</item>
        /// <item>Only organisations of which the requesting user is a member will be returned. All organisations will be returned to a super user.</item>
        /// </list>
        /// </remarks>
        /// <param name="parentId">(Optional query parameter) The ID of the parent <see cref="Organisation"/> to retrieve child <see cref="Organisation">Organisations</see> for.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing a <see cref="List{T}"/> of <see cref="OrganisationDTO"/> objects.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationMember">OrganisationMember</see> or <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> roles.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="parentId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpGet]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(List<OrganisationDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrganisationsAsync(string parentId)
        {
            try
            {
                if (!RequestingUserRoles.Any(role =>
                        role is UserRoles.OrganisationMember or UserRoles.OrganisationAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                List<OrganisationDTO> allowedOrganisations =
                    await _martialBaseUserHelper.FilterOrganisationsByMemberAccess(
                        await GetOrganisationsInternalAsync(parentId),
                        RequestingPersonId);

                return Ok(allowedOrganisations.OrderBy(o => o.Initials).ToList());
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for a specified <see cref="Organisation"/>.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in either the <see cref="UserRoles.OrganisationMember">OrganisationMember</see>
        /// or <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for this endpoint.
        /// </remarks>
        /// <param name="organisationId">The ID of the <see cref="Organisation"/> to retrieve.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing an <see cref="OrganisationDTO"/> object.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in either the <see cref="UserRoles.OrganisationMember">OrganisationMember</see> or <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not a member of the requested <see cref="Organisation"/>.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="organisationId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpGet("{organisationId}")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(OrganisationDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrganisationAsync(string organisationId)
        {
            try
            {
                var organisationIdGuid = new Guid(organisationId);

                await _martialBaseUserHelper.ValidateUserHasMemberAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                return Ok(await _organisationsRepository.GetAsync(organisationIdGuid));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.5">POST</see>
        /// request to create a new <see cref="Organisation"/>.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Requesting user must be in the <see cref="UserRoles.User">User</see> role for this endpoint.</item>
        /// <item>If the new organisation has a parent, the requesting user must be an admin of the parent. The requesting user will be made an admin of the newly-created organisation and assigned the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// </list>
        /// </remarks>
        /// <param name="createOrganisationDTO">The <see cref="CreateOrganisationDTO"/> model to use to create the new <see cref="Organisation"/>.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>A <see cref="CreatedResult"/> containing an <see cref="OrganisationDTO"/> object if creation is successful.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if the <see cref="CreateOrganisationDTO"/> model state is not valid. Model state errors are returned in the response body.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.User">User</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not an admin of the parent <see cref="Organisation"/> to which the new child <see cref="Organisation"/> will belong (if applicable).</item>
        /// <item>A <see cref="NotFoundResult"/> if a parent <see cref="Organisation"/> ID is provided but cannot be found.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <see cref="CreateOrganisationDTO"/> contains an address with an invalid <see cref="Country"/> code.</item>
        /// </list>
        /// </returns>
        [HttpPost]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(OrganisationDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateOrganisationAsync([FromBody] CreateOrganisationDTO createOrganisationDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                Guid? parentIdGuid = null;

                if (!string.IsNullOrWhiteSpace(createOrganisationDTO.ParentId))
                {
                    parentIdGuid = new Guid(createOrganisationDTO.ParentId);

                    await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                        RequestingPersonId,
                        (Guid)parentIdGuid);
                }

                if (createOrganisationDTO.Address != null &&
                    !_countriesRepository.Exists(createOrganisationDTO.Address.CountryCode))
                {
                    throw new EntityNotFoundException(
                        $"Country code '{createOrganisationDTO.Address.CountryCode}' not found.");
                }

                var createOrganisationInternalDTO = new CreateOrganisationInternalDTO
                {
                    Initials = createOrganisationDTO.Initials,
                    Name = createOrganisationDTO.Name,
                    ParentId = parentIdGuid,
                    Address = createOrganisationDTO.Address
                };

                OrganisationDTO createdOrganisation =
                    await _organisationsRepository.CreateAsync(createOrganisationInternalDTO);

                await _organisationsRepository.AddOrganisationPersonAsync(
                    new Guid(createdOrganisation.Id), RequestingPersonId, true);

                if (!await _organisationsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                await _martialBaseUserRolesRepository.AddRoleToUserAsync(
                    await _martialBaseUserHelper.GetUserIdForPersonAsync(RequestingPersonId), UserRoles.OrganisationAdmin);

                return StatusCode(StatusCodes.Status201Created, createdOrganisation);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("newperson")]
        [ProducesResponseType(typeof(PersonOrganisationDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateOrganisationWithNewPersonAsync([FromBody] CreatePersonOrganisationDTO createPersonOrganisationDTO)
        {
            // This endpoint is ONLY to be used for initial setup where the requesting user has no Person or
            // MartialBaseUser record. The requesting user will be set as an OrganisationAdmin of the
            // newly-created Organisation.

            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                if (createPersonOrganisationDTO.Organisation.Address != null &&
                    !_countriesRepository.Exists(createPersonOrganisationDTO.Organisation.Address.CountryCode))
                {
                    throw new EntityNotFoundException(
                        $"Country code '{createPersonOrganisationDTO.Organisation.Address.CountryCode}' not found.");
                }

                if (createPersonOrganisationDTO.Person.Address != null &&
                    !_countriesRepository.Exists(createPersonOrganisationDTO.Person.Address.CountryCode))
                {
                    throw new EntityNotFoundException(
                        $"Country code '{createPersonOrganisationDTO.Person.Address.CountryCode}' not found.");
                }

                var createOrganisationInternalDTO = new CreateOrganisationInternalDTO
                {
                    Initials = createPersonOrganisationDTO.Organisation.Initials,
                    Name = createPersonOrganisationDTO.Organisation.Name,
                    Address = createPersonOrganisationDTO.Organisation.Address
                };

                PersonOrganisationDTO createdPersonOrganisation;

                var createPersonInternalDTO = new CreatePersonInternalDTO
                {
                    Title = createPersonOrganisationDTO.Person.Title,
                    FirstName = createPersonOrganisationDTO.Person.FirstName,
                    MiddleName = createPersonOrganisationDTO.Person.MiddleName,
                    LastName = createPersonOrganisationDTO.Person.LastName,
                    DateOfBirth =
                        createPersonOrganisationDTO.Person.DateOfBirth != null ?
                            Convert.ToDateTime(createPersonOrganisationDTO.Person.DateOfBirth) :
                            null,
                    Address = createPersonOrganisationDTO.Person.Address,
                    MobileNo = createPersonOrganisationDTO.Person.MobileNo,
                    Email = createPersonOrganisationDTO.Person.Email
                };

                createdPersonOrganisation =
                    await _organisationsRepository.CreateOrganisationWithNewPersonAsAdminAsync(
                        createOrganisationInternalDTO,
                        createPersonInternalDTO,
                        _azureUserHelper.GetAzureIdAndInvitationCodeFromHttpRequest(Request).AzureId);

                if (!await _organisationsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return StatusCode(StatusCodes.Status201Created, createdPersonOrganisation);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.6">PUT</see>
        /// request to update an existing <see cref="Organisation"/>.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for
        /// this endpoint.
        /// </remarks>
        /// <param name="organisationId">The ID of the <see cref="Organisation"/> to update.</param>
        /// <param name="updateOrganisationDTO">The <see cref="UpdateOrganisationDTO"/> model to use to update the <see cref="Organisation"/>.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing an <see cref="OrganisationDTO"/> object if update is successful.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if the <see cref="UpdateOrganisationDTO"/> model state is not valid. Model state errors are returned in the response body.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not an admin of the <see cref="Organisation"/> to be updated.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="organisationId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpPut("{organisationId}")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(OrganisationDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateOrganisationAsync(string organisationId, [FromBody] UpdateOrganisationDTO updateOrganisationDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                var organisationIdGuid = new Guid(organisationId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                OrganisationDTO updatedOrganisation =
                    await _organisationsRepository.UpdateAsync(organisationIdGuid, updateOrganisationDTO);

                if (!await _organisationsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return Ok(updatedOrganisation);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.6">PUT</see>
        /// request to change an existing <see cref="Organisation"/>'s parent.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for
        /// this endpoint.
        /// </remarks>
        /// <param name="organisationId">The ID of the <see cref="Organisation"/> to change the parent of.</param>
        /// <param name="parentId">(Required query parameter) The ID of the new parent <see cref="Organisation"/>.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> if update is successful.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not an admin of any of the old parent, new parent or child <see cref="Organisation">Organisations</see>.</item>
        /// <item>A <see cref="BadRequestResult"/> if no <paramref name="parentId"/> parameter is provided.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="organisationId"/> cannot be found.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="parentId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpPut("{organisationId}/parent")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeOrganisationParentAsync(string organisationId, string parentId)
        {
            try
            {
                if (parentId == null)
                {
                    return BadRequest("No parent ID parameter specified.");
                }

                var organisationIdGuid = new Guid(organisationId);
                var newParentIdGuid = new Guid(parentId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                Guid? existingParentId = await _organisationsRepository.GetOrganisationParentIdAsync(organisationIdGuid);

                if (existingParentId != null)
                {
                    await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                        RequestingPersonId, (Guid)existingParentId);
                }

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, newParentIdGuid);

                await _organisationsRepository.ChangeOrganisationParentAsync(organisationIdGuid, newParentIdGuid);

                if (!await _organisationsRepository.SaveChangesAsync())
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

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.7">DELETE</see>
        /// request to remove a parent from an existing <see cref="Organisation"/>.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for
        /// this endpoint.
        /// </remarks>
        /// <param name="organisationId">The ID of the <see cref="Organisation"/> to remove the parent from.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>A <see cref="NoContentResult"/> if removal is successful or if the provided <see cref="Organisation"/> doesn't have a parent already.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not an admin of either the current parent or child <see cref="Organisation"/>.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="organisationId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpDelete("{organisationId}/parent")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveOrganisationParentAsync(string organisationId)
        {
            try
            {
                var organisationIdGuid = new Guid(organisationId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                Guid? existingParentId = await _organisationsRepository.GetOrganisationParentIdAsync(organisationIdGuid);

                if (existingParentId == null)
                {
                    return NoContent();
                }

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, (Guid)existingParentId);

                await _organisationsRepository.ChangeOrganisationParentAsync(organisationIdGuid, null);

                if (!await _organisationsRepository.SaveChangesAsync())
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

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.5">POST</see>
        /// request to create and set a new <see cref="Address"/> for an existing <see cref="Organisation"/>.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Requesting user must be in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for this endpoint.</item>
        /// <item>The existing <see cref="Address"/> assigned to the <see cref="Organisation"/> will be deleted.</item>
        /// </list>
        /// </remarks>
        /// <param name="organisationId">The ID of the <see cref="Organisation"/> to create a new <see cref="Address"/> for.</param>
        /// <param name="createAddressDTO">The <see cref="CreateAddressDTO"/> model to use to create the new <see cref="Address"/>.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>A <see cref="CreatedResult"/> containing an <see cref="AddressDTO"/> object if creation is successful.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if the <see cref="CreateAddressDTO"/> model state is not valid. Model state errors are returned in the response body.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not an admin of the <see cref="Organisation"/> to which the new <see cref="DocumentType"/> will belong.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="organisationId"/> cannot be found.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="createAddressDTO"/> contains an invalid <see cref="Country"/> code.</item>
        /// </list>
        /// </returns>
        [HttpPost("{organisationId}/address")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(AddressDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> ChangeOrganisationAddressAsync(string organisationId, [FromBody] CreateAddressDTO createAddressDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                var organisationIdGuid = new Guid(organisationId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                if (!_countriesRepository.Exists(createAddressDTO.CountryCode))
                {
                    throw new EntityNotFoundException($"Country code '{createAddressDTO.CountryCode}' not found.");
                }

                AddressDTO newAddress =
                    await _organisationsRepository.ChangeOrganisationAddressAsync(organisationIdGuid, createAddressDTO);

                if (!await _organisationsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return StatusCode(StatusCodes.Status201Created, newAddress);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.5">POST</see>
        /// request to add an existing <see cref="Person"/> to an existing <see cref="Organisation"/>.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Requesting user must be in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for this endpoint.</item>
        /// <item>If the specified <see cref="Person"/> is already a member of the <see cref="Organisation"/>, their admin status will be set according to the provided <paramref name="isAdmin"/> parameter.</item>
        /// </list>
        /// </remarks>
        /// <param name="organisationId">The ID of the <see cref="Organisation"/> to assign the <see cref="Person"/> to.</param>
        /// <param name="personId">(Required query parameter) The ID of the <see cref="Person"/> to be assigned.</param>
        /// <param name="isAdmin">(Optional query parameter) Whether the <see cref="Person"/> should be set as an admin of the <see cref="Organisation"/>. Default is false if not provided.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An empty <see cref="CreatedResult"/> if successful.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not an admin of the <see cref="Organisation"/> to which the <see cref="Person"/> will be assigned.</item>
        /// <item>A <see cref="BadRequestResult"/> if no <paramref name="personId"/> parameter is provided.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="organisationId"/> cannot be found.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="personId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpPost("{organisationId}/people")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddExistingPersonToOrganisationAsync(string organisationId, string personId, string isAdmin)
        {
            try
            {
                bool isAdminBool = false;

                if (personId == null)
                {
                    return BadRequest("No person ID parameter specified.");
                }

                if (isAdmin != null)
                {
                    try
                    {
                        isAdminBool = Convert.ToBoolean(isAdmin);
                    }
                    catch
                    {
                        return BadRequest("Invalid value provided for isAdmin parameter.");
                    }
                }

                var organisationIdGuid = new Guid(organisationId);
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Person", personIdGuid);
                }

                // If the person is already an admin and the isAdmin parameter is set to false, then demote the
                // person
                if (isAdmin != null && !isAdminBool &&
                    await _organisationsRepository.PersonIsOrganisationAdminAsync(organisationIdGuid, personIdGuid))
                {
                    await _organisationsRepository.DemoteOrganisationAdminAsync(organisationIdGuid, personIdGuid);
                }
                else
                {
                    await _organisationsRepository.AddOrganisationPersonAsync(organisationIdGuid, personIdGuid, isAdminBool);
                }

                if (!await _organisationsRepository.SaveChangesAsync())
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

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.7">DELETE</see>
        /// request to remove a <see cref="Person"/> from an <see cref="Organisation"/>.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Requesting user must be in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for this endpoint.</item>
        /// <item>This only removes the <see cref="Person"/> from the <see cref="Organisation"/>, it doesn't delete them from system.</item>
        /// </list>
        /// </remarks>
        /// <param name="organisationId">The ID of the <see cref="Organisation"/> to remove the <see cref="Person"/> from.</param>
        /// <param name="personId">The ID of the <see cref="Person"/> to be removed.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>A <see cref="NoContentResult"/> if removal is successful.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not an admin of the specified <see cref="Organisation"/>.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="organisationId"/> cannot be found.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="personId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpDelete("{organisationId}/people/{personId}")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveOrganisationPersonAsync(string organisationId, string personId)
        {
            try
            {
                var organisationIdGuid = new Guid(organisationId);
                var personIdGuid = new Guid(personId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                if (!await _peopleRepository.ExistsAsync(personIdGuid))
                {
                    throw new EntityIdNotFoundException("Person", personIdGuid);
                }

                await _organisationsRepository.RemoveOrganisationPersonAsync(organisationIdGuid, personIdGuid);

                if (!await _organisationsRepository.SaveChangesAsync())
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

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for a list of <see cref="Person">People</see> belonging to a specified <see cref="Organisation"/>.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for
        /// this endpoint.
        /// </remarks>
        /// <param name="organisationId">The ID of the <see cref="Organisation"/> to retrieve <see cref="Person">People</see> for.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing an <see cref="IList{T}"/> of <see cref="OrganisationPersonDTO"/> objects.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="organisationId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpGet("{organisationId}/people")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(List<OrganisationPersonDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrganisationPeopleAsync(string organisationId)
        {
            try
            {
                var organisationIdGuid = new Guid(organisationId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                return Ok(await _organisationsRepository.GetPeopleAsync(organisationIdGuid));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for a list of <see cref="DocumentType">DocumentTypes</see> belonging to a specified <see cref="Organisation"/>.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in either the <see cref="UserRoles.OrganisationMember">OrganisationMember</see>
        /// or <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for this endpoint.
        /// </remarks>
        /// <param name="organisationId">The ID of the <see cref="Organisation"/> to retrieve <see cref="DocumentType">DocumentTypes</see> for.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing an <see cref="IList{T}"/> of <see cref="DocumentTypeDTO"/> objects.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationMember">OrganisationMember</see> or <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> roles.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="organisationId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpGet("{organisationId}/documenttypes")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(typeof(List<DocumentTypeDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrganisationDocumentTypesAsync(string organisationId)
        {
            try
            {
                var organisationIdGuid = new Guid(organisationId);

                await _martialBaseUserHelper.ValidateUserHasMemberAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                return Ok(await _documentTypesRepository.GetAllAsync(organisationIdGuid));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.7">DELETE</see>
        /// request to delete an existing <see cref="Organisation"/>.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Requesting user must be in the super user role for this endpoint.</item>
        /// <item>This will not delete any <see cref="Person">People</see> in the <see cref="Organisation"/> from system, it only removes the membership from the <see cref="Organisation"/> being deleted.</item>
        /// </list>
        /// </remarks>
        /// <param name="organisationId">The ID of the <see cref="Organisation"/> to delete.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>A <see cref="NoContentResult"/> if deletion is successful.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not a super user.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="organisationId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpDelete("{organisationId}")]
        [PersonIdRequiredAsync]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteOrganisationAsync(string organisationId)
        {
            try
            {
                var organisationIdGuid = new Guid(organisationId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                await _organisationsRepository.DeleteAsync(organisationIdGuid);

                if (!await _organisationsRepository.SaveChangesAsync())
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

        /// <summary>
        /// Internal method to retrieve all organisations, where a parent ID may be specified.
        /// </summary>
        /// <param name="parentOrganisationId">The ID of an organisation to retrieve child organisations for.</param>
        /// <returns>A list of Organisations.</returns>
        /// <exception cref="EntityIdNotFoundException">Thrown when the provided Organisation ID is not found.</exception>
        private async Task<IList<OrganisationDTO>> GetOrganisationsInternalAsync(string parentOrganisationId)
        {
            if (parentOrganisationId != null)
            {
                var parentIdGuid = new Guid(parentOrganisationId);

                if (!await _organisationsRepository.ExistsAsync(parentIdGuid))
                {
                    throw new EntityIdNotFoundException("Organisation", parentIdGuid);
                }

                return await _organisationsRepository.GetChildOrganisationsAsync(parentIdGuid);
            }

            return await _organisationsRepository.GetAllAsync();
        }
    }
}
