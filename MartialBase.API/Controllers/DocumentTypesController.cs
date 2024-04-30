// <copyright file="DocumentTypesController.cs" company="Martialtech®">
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
using MartialBase.API.Data.Models.InternalDTOs.DocumentTypes;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.DocumentTypes;
using MartialBase.API.Models.Enums;
using MartialBase.API.Tools;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace MartialBase.API.Controllers
{
    /// <summary>
    /// Endpoints used to retrieve <see cref="DocumentType">DocumentTypes</see>.
    /// </summary>
    /// <remarks>
    /// All endpoints require the requesting user be either an
    /// <see cref="UserRoles.OrganisationMember">OrganisationMember</see> or
    /// <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> of the <see cref="Organisation"/> owning
    /// the <see cref="DocumentType"/> (except public <see cref="Organisation">Organisations</see>), and only
    /// organisation admins and super users can manage <see cref="DocumentType">DocumentTypes</see>.
    /// </remarks>
    [Authorize]
    [PersonIdRequiredAsync]
    [Route("documenttypes")]
    [RequiredScope("query")]
    public class DocumentTypesController : MartialBaseControllerBase
    {
        private readonly IDocumentTypesRepository _documentTypesRepository;
        private readonly IMartialBaseUserHelper _martialBaseUserHelper;
        private readonly IOrganisationsRepository _organisationsRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTypesController"/> class.
        /// </summary>
        /// <param name="documentTypesRepository">The current <see cref="IDocumentTypesRepository"/> instance.</param>
        /// <param name="martialBaseUserHelper">The current <see cref="IMartialBaseUserHelper"/> instance.</param>
        /// <param name="organisationsRepository">The current <see cref="IOrganisationsRepository"/>instance.</param>
        /// <param name="hostEnvironment">The current <see cref="IWebHostEnvironment"/> instance.</param>
        public DocumentTypesController(
            IDocumentTypesRepository documentTypesRepository,
            IMartialBaseUserHelper martialBaseUserHelper,
            IOrganisationsRepository organisationsRepository,
            IWebHostEnvironment hostEnvironment)
        {
            _documentTypesRepository = documentTypesRepository;
            _martialBaseUserHelper = martialBaseUserHelper;
            _organisationsRepository = organisationsRepository;
            HostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for a list of <see cref="DocumentType">DocumentTypes</see>.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in either the <see cref="UserRoles.OrganisationMember">OrganisationMember</see>
        /// or <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for this endpoint.
        /// </remarks>
        /// <param name="organisationId">(Optional query parameter) The ID of the <see cref="Organisation"/> to retrieve <see cref="DocumentType">DocumentTypes</see> for.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing a <see cref="List{T}"/> of <see cref="DocumentTypeDTO"/> objects.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationMember">OrganisationMember</see> or <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> roles.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="organisationId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<DocumentTypeDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDocumentTypesAsync(string organisationId)
        {
            try
            {
                if (!RequestingUserRoles.Any(role =>
                        role is UserRoles.OrganisationMember or UserRoles.OrganisationAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(
                        ErrorResponseCode.InsufficientUserRole,
                        HttpStatusCode.Forbidden);
                }

                List<DocumentTypeDTO> documentTypes;

                if (organisationId != null)
                {
                    var organisationIdGuid = new Guid(organisationId);

                    if (!await _organisationsRepository.ExistsAsync(organisationIdGuid))
                    {
                        throw new EntityIdNotFoundException("Organisation", organisationIdGuid);
                    }

                    documentTypes = await _documentTypesRepository.GetAllAsync(organisationIdGuid);
                }
                else
                {
                    documentTypes = await _documentTypesRepository.GetAllAsync();
                }

                var allowedDocumentTypes = new List<DocumentTypeDTO>();

                if (RequestingUserRoles.Contains(UserRoles.Thanos))
                {
                    allowedDocumentTypes = documentTypes;
                }
                else
                {
                    foreach (DocumentTypeDTO documentType in documentTypes)
                    {
                        if (await _organisationsRepository.PersonCanAccessOrganisationAsync(
                            new Guid(documentType.OrganisationId), RequestingPersonId))
                        {
                            allowedDocumentTypes.Add(documentType);
                        }
                    }
                }

                return Ok(allowedDocumentTypes.OrderBy(dt => dt.Name));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for a specified <see cref="DocumentType"/>.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in either the <see cref="UserRoles.OrganisationMember">OrganisationMember</see>
        /// or <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for this endpoint.
        /// </remarks>
        /// <param name="documentTypeId">The ID of the <see cref="DocumentType"/> to retrieve.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing a <see cref="DocumentTypeDTO"/> object.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationMember">OrganisationMember</see> or <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> roles.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not a member or admin of the <see cref="Organisation"/> to which the requested <see cref="DocumentType"/> belongs.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="documentTypeId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpGet("{documentTypeId}")]
        [ProducesResponseType(typeof(DocumentTypeDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDocumentTypeAsync(string documentTypeId)
        {
            try
            {
                var documentTypeIdGuid = new Guid(documentTypeId);

                if (!await _documentTypesRepository.ExistsAsync(documentTypeIdGuid))
                {
                    throw new EntityIdNotFoundException("Document type", documentTypeIdGuid);
                }

                DocumentTypeDTO documentTypeDTO = await _documentTypesRepository.GetAsync(documentTypeIdGuid);

                var organisationIdGuid = new Guid(documentTypeDTO.OrganisationId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                return Ok(documentTypeDTO);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.5">POST</see>
        /// request to create a new <see cref="DocumentType"/>.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for
        /// this endpoint.
        /// </remarks>
        /// <param name="createDocumentTypeDTO">The <see cref="CreateDocumentTypeDTO"/> model to use to create the new <see cref="DocumentType"/>.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>A <see cref="CreatedResult"/> containing a <see cref="DocumentTypeDTO"/> object if creation is successful.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if the <see cref="CreateDocumentTypeDTO"/> model state is not valid. Model state errors are returned in the response body.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not an admin of the <see cref="Organisation"/> to which the new <see cref="DocumentType"/> will belong.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="createDocumentTypeDTO"/> contains an invalid <see cref="Organisation"/> ID.</item>
        /// </list>
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(DocumentTypeDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateDocumentTypeAsync([FromBody] CreateDocumentTypeDTO createDocumentTypeDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                var organisationIdGuid = new Guid(createDocumentTypeDTO.OrganisationId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                var createDocumentTypeInternalDTO = new CreateDocumentTypeInternalDTO
                {
                    OrganisationId = organisationIdGuid,
                    ReferenceNo = createDocumentTypeDTO.ReferenceNo,
                    Name = createDocumentTypeDTO.Name,
                    DefaultExpiryDays = createDocumentTypeDTO.DefaultExpiryDays,
                    URL = createDocumentTypeDTO.URL
                };

                DocumentTypeDTO createdDocumentType =
                    await _documentTypesRepository.CreateAsync(createDocumentTypeInternalDTO);

                if (!await _documentTypesRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return StatusCode(StatusCodes.Status201Created, createdDocumentType);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.6">PUT</see>
        /// request to update an existing <see cref="DocumentType"/>.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for
        /// this endpoint.
        /// </remarks>
        /// <param name="documentTypeId">(Required query parameter) The ID of the <see cref="DocumentType"/> to update.</param>
        /// <param name="updateDocumentTypeDTO">The <see cref="UpdateDocumentTypeDTO"/> model to use to update the <see cref="DocumentType"/>.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>A <see cref="OkResult"/> containing a <see cref="DocumentTypeDTO"/> object if update is successful.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if the <see cref="UpdateDocumentTypeDTO"/> model state is not valid. Model state errors are returned in the response body.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not an admin of the <see cref="Organisation"/> to which the updated <see cref="DocumentType"/> belongs.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="documentTypeId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(DocumentTypeDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDocumentTypeAsync(string documentTypeId, [FromBody] UpdateDocumentTypeDTO updateDocumentTypeDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                var documentTypeIdGuid = new Guid(documentTypeId);

                if (!await _documentTypesRepository.ExistsAsync(documentTypeIdGuid))
                {
                    throw new EntityIdNotFoundException("Document type", documentTypeIdGuid);
                }

                DocumentTypeDTO documentTypeDTO = await _documentTypesRepository.GetAsync(documentTypeIdGuid);

                var organisationIdGuid = new Guid(documentTypeDTO.OrganisationId);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                DocumentTypeDTO updatedDocumentType =
                    await _documentTypesRepository.UpdateAsync(documentTypeIdGuid, updateDocumentTypeDTO);

                if (!await _organisationsRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return Ok(updatedDocumentType);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.6">PUT</see>
        /// request to change the <see cref="Organisation"/> to which an existing <see cref="DocumentType"/> belongs.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for
        /// this endpoint.
        /// </remarks>
        /// <param name="documentTypeId">The ID of the <see cref="DocumentType"/> to update.</param>
        /// <param name="organisationId">(Required query parameter) The ID of the <see cref="Organisation"/> to which the <see cref="DocumentType"/> will be moved to.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> if update is successful.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="BadRequestResult"/> if no <paramref name="organisationId"/> parameter is provided.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not an admin of either the old or the new <see cref="Organisation"/> to which the <see cref="DocumentType"/> belongs.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="documentTypeId"/> cannot be found.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="organisationId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpPut("{documentTypeId}/organisation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeDocumentTypeOrganisationAsync(string documentTypeId, string organisationId)
        {
            try
            {
                if (organisationId == null)
                {
                    return BadRequest("No organisation ID parameter specified.");
                }

                var documentTypeIdGuid = new Guid(documentTypeId);
                var organisationIdGuid = new Guid(organisationId);

                if (!await _documentTypesRepository.ExistsAsync(documentTypeIdGuid))
                {
                    throw new EntityIdNotFoundException("Document type", documentTypeIdGuid);
                }

                Guid existingOrganisationId =
                    await _documentTypesRepository.GetDocumentTypeOrganisationIdAsync(documentTypeIdGuid);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, existingOrganisationId);

                await _documentTypesRepository.ChangeDocumentTypeOrganisationAsync(documentTypeIdGuid, organisationIdGuid);

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
        /// request to delete an existing <see cref="DocumentType"/>.
        /// </summary>
        /// <remarks>
        /// Requesting user must be in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role for
        /// this endpoint.
        /// </remarks>
        /// <param name="documentTypeId">The ID of the <see cref="DocumentType"/> to delete.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>A <see cref="NoContentResult"/> if deletion is successful.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.OrganisationAdmin">OrganisationAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not an admin of the <see cref="Organisation"/> to which the <see cref="DocumentType"/> belongs.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="documentTypeId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpDelete("{documentTypeId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteDocumentTypeAsync(string documentTypeId)
        {
            try
            {
                var documentTypeIdGuid = new Guid(documentTypeId);

                if (!await _documentTypesRepository.ExistsAsync(documentTypeIdGuid))
                {
                    throw new EntityIdNotFoundException("Document type", documentTypeIdGuid);
                }

                Guid organisationIdGuid =
                    await _documentTypesRepository.GetDocumentTypeOrganisationIdAsync(documentTypeIdGuid);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                await _documentTypesRepository.DeleteAsync(documentTypeIdGuid);

                if (!await _documentTypesRepository.SaveChangesAsync())
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
