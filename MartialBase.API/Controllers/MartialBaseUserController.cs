// <copyright file="MartialBaseUserController.cs" company="Martialtech®">
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
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.MartialBaseUsers;
using MartialBase.API.Models.DTOs.UserRoles;
using MartialBase.API.Models.Enums;
using MartialBase.API.Tools;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace MartialBase.API.Controllers
{
    /// <summary>
    /// Endpoints used to manage <see cref="MartialBaseUser">MartialBaseUsers</see> and
    /// <see cref="MartialBaseUserRole">MartialBaseUserRoles</see>.
    /// </summary>
    /// <remarks>All endpoints require system admin or super user access.</remarks>
    [Route("admin")]
    [RequiredScope("query")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Suppression is used in this file due to differing behaviour in Testing configuration.")]
    public class MartialBaseUserController : MartialBaseControllerBase
    {
        private readonly IAzureUserHelper _azureUserHelper;
        private readonly IMartialBaseUsersRepository _martialBaseUsersRepository;
        private readonly IMartialBaseUserRolesRepository _martialBaseUserRolesRepository;
        private readonly IUserRolesRepository _userRolesRepository;
        private readonly IPeopleRepository _peopleRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartialBaseUserController"/> class.
        /// </summary>
        /// <param name="azureUserHelper">The current <see cref="IAzureUserHelper"/> instance.</param>
        /// <param name="martialBaseUsersRepository">The current <see cref="IMartialBaseUsersRepository"/> instance.</param>
        /// <param name="martialBaseUserRolesRepository">The current <see cref="IMartialBaseUserRolesRepository"/> instance.</param>
        /// <param name="userRolesRepository">The current <see cref="IUserRolesRepository"/> instance.</param>
        /// <param name="peopleRepository">The current <see cref="IPeopleRepository"/> instance.</param>
        /// <param name="hostEnvironment">The current <see cref="IWebHostEnvironment"/> instance.</param>
        public MartialBaseUserController(
            IAzureUserHelper azureUserHelper,
            IMartialBaseUsersRepository martialBaseUsersRepository,
            IMartialBaseUserRolesRepository martialBaseUserRolesRepository,
            IUserRolesRepository userRolesRepository,
            IPeopleRepository peopleRepository,
            IWebHostEnvironment hostEnvironment)
        {
            _azureUserHelper = azureUserHelper;
            _martialBaseUsersRepository = martialBaseUsersRepository;
            _martialBaseUserRolesRepository = martialBaseUserRolesRepository;
            _userRolesRepository = userRolesRepository;
            _peopleRepository = peopleRepository;
            HostEnvironment = hostEnvironment;
        }

        [Authorize]
        [PersonIdRequiredAsync]
        [Route("users/{userId}/invitationcode")]
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInvitationCodeAsync(string userId)
        {
            try
            {
                if (!RequestingUserRoles.Any(role => role is UserRoles.SystemAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                var userIdGuid = new Guid(userId);

                if (!await _martialBaseUsersRepository.ExistsAsync(userIdGuid))
                {
                    throw new EntityIdNotFoundException("User", userId);
                }

                string invitationCode = await _martialBaseUsersRepository.GenerateInvitationCodeAsync(userIdGuid);

                if (!await _martialBaseUsersRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return Ok(invitationCode);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [Authorize]
        [PersonIdRequiredAsync]
        [Route("users")]
        [HttpGet]
        [ProducesResponseType(typeof(List<MartialBaseUserDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsersAsync()
        {
            try
            {
                if (!RequestingUserRoles.Any(role => role is UserRoles.SystemAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                return Ok(await _martialBaseUsersRepository.GetAllAsync());
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [Authorize]
        [PersonIdRequiredAsync]
        [Route("users/{userId}")]
        [HttpGet]
        [ProducesResponseType(typeof(MartialBaseUserDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserAsync(string userId)
        {
            try
            {
                if (!RequestingUserRoles.Any(role => role is UserRoles.SystemAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                var userIdGuid = new Guid(userId);

                if (!await _martialBaseUsersRepository.ExistsAsync(userIdGuid))
                {
                    throw new EntityIdNotFoundException("User", userId);
                }

                return Ok(await _martialBaseUsersRepository.GetAsync(userIdGuid));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [Authorize]
        [PersonIdRequiredAsync]
        [Route("roles")]
        [HttpGet]
        [ProducesResponseType(typeof(List<UserRole>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRolesAsync()
        {
            try
            {
                if (!RequestingUserRoles.Any(role => role is UserRoles.SystemAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                return Ok(await _userRolesRepository.GetAllAsync());
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [Authorize]
        [PersonIdRequiredAsync]
        [Route("users/{userId}/roles")]
        [HttpGet]
        [ProducesResponseType(typeof(List<UserRoleDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRolesForUserAsync(string userId)
        {
            try
            {
                if (!RequestingUserRoles.Any(role => role is UserRoles.SystemAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                var martialBaseUserId = new Guid(userId);

                if (!await _martialBaseUsersRepository.ExistsAsync(martialBaseUserId))
                {
                    throw new EntityIdNotFoundException("User", userId);
                }

                return Ok(
                    await _martialBaseUserRolesRepository.GetRolesForUserAsync(martialBaseUserId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

#if !RELEASE
        [HttpPost("users")]
        [ProducesResponseType(typeof(MartialBaseUserDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateMartialBaseUserDTO createMartialBaseUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ModelStateTools.PrepareModelStateErrors(ModelState));
            }

            if (!string.IsNullOrWhiteSpace(createMartialBaseUserDTO.PersonId) &&
                !await _peopleRepository.ExistsAsync(new Guid(createMartialBaseUserDTO.PersonId)))
            {
                throw new EntityIdNotFoundException("Person", createMartialBaseUserDTO.PersonId);
            }

            var createdMartialBaseUser = await _martialBaseUsersRepository.CreateAsync(createMartialBaseUserDTO);

            if (!await _martialBaseUsersRepository.SaveChangesAsync())
            {
                throw new DbContextSaveChangesException();
            }

            return StatusCode(StatusCodes.Status201Created, createdMartialBaseUser);
        }
#endif

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.5">POST</see>
        /// request to add a <see cref="UserRole"/> to a <see cref="MartialBaseUser"/>.
        /// </summary>
        /// <remarks>Requesting user must be in the system admin or super user role.</remarks>
        /// <param name="userId">The ID of the <see cref="MartialBaseUser"/> to assign the <see cref="UserRole"/> to.</param>
        /// <param name="roleId">(Required query parameter) The ID of the <see cref="UserRole"/> to be assigned to the <see cref="MartialBaseUser"/>.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>A <see cref="CreatedResult"/> if the <see cref="UserRole"/> is successfully assigned to the <see cref="MartialBaseUser"/>.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is always returned.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="BadRequestResult"/> if no <paramref name="roleId"/> parameter is provided.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the system admin or super user role.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="userId"/> cannot be found.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="roleId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [Authorize]
        [PersonIdRequiredAsync]
        [HttpPost("users/{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddRoleToUserAsync(string userId, string roleId)
        {
            try
            {
                if (!RequestingUserRoles.Any(role => role is UserRoles.SystemAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                if (roleId == null)
                {
                    return BadRequest("No role ID parameter specified.");
                }

                var userIdGuid = new Guid(userId);
                var roleIdGuid = new Guid(roleId);

                if (!await _martialBaseUsersRepository.ExistsAsync(userIdGuid))
                {
                    throw new EntityIdNotFoundException("User", userId);
                }

                if (!await _userRolesRepository.ExistsAsync(roleIdGuid))
                {
                    throw new EntityIdNotFoundException("User role", roleId);
                }

                await _martialBaseUserRolesRepository.AddRoleToUserAsync(userIdGuid, new Guid(roleId));

                if (!await _martialBaseUserRolesRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

#if !TESTING
                await _azureUserHelper.UpdateAzureUserRoles(userIdGuid);
#endif

                return StatusCode(StatusCodes.Status201Created);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.7">DELETE</see>
        /// request to remove an assigned <see cref="UserRole"/> from a <see cref="MartialBaseUser"/>.
        /// </summary>
        /// <remarks>Requesting user must be in the system admin or super user role.</remarks>
        /// <param name="userId">The ID of the <see cref="MartialBaseUser"/> to remove the <see cref="UserRole"/> from.</param>
        /// <param name="roleId">The ID of the <see cref="UserRole"/> to remove from the <see cref="MartialBaseUser"/>.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>A <see cref="NoContentResult"/> if the <see cref="UserRole"/> is successfully removed from the <see cref="MartialBaseUser"/>.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is always returned.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the system admin or super user role.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="userId"/> cannot be found.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="roleId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [Authorize]
        [PersonIdRequiredAsync]
        [HttpDelete("users/{userId}/roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveRoleFromUserAsync(string userId, string roleId)
        {
            try
            {
                if (!RequestingUserRoles.Any(role => role is UserRoles.SystemAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                var userIdGuid = new Guid(userId);
                var roleIdGuid = new Guid(roleId);

                if (!await _martialBaseUsersRepository.ExistsAsync(userIdGuid))
                {
                    throw new EntityIdNotFoundException("User", userId);
                }

                if (!await _userRolesRepository.ExistsAsync(roleIdGuid))
                {
                    throw new EntityIdNotFoundException("User role", roleId);
                }

                await _martialBaseUserRolesRepository.RemoveRoleFromUserAsync(userIdGuid, new Guid(roleId));

                if (!await _martialBaseUserRolesRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

#if !TESTING
                await _azureUserHelper.UpdateAzureUserRoles(userIdGuid);
#endif

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [Authorize]
        [PersonIdRequiredAsync]
        [HttpPut("users/{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SetUserRolesAsync(string userId, [FromBody] List<Guid> userRoleIds)
        {
            try
            {
                if (!RequestingUserRoles.Any(role => role is UserRoles.SystemAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                var userIdGuid = new Guid(userId);

                if (!await _martialBaseUsersRepository.ExistsAsync(userIdGuid))
                {
                    throw new EntityIdNotFoundException("User", userId);
                }

                if (userRoleIds == null || !userRoleIds.Any())
                {
                    return BadRequest("No role IDs specified.");
                }

                foreach (Guid userRoleId in userRoleIds)
                {
                    if (!await _userRolesRepository.ExistsAsync(userRoleId))
                    {
                        throw new EntityIdNotFoundException("User role", userRoleId);
                    }
                }

                await _martialBaseUserRolesRepository.SetRolesForUserAsync(userIdGuid, userRoleIds);

                if (!await _martialBaseUserRolesRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

#if !TESTING
                await _azureUserHelper.UpdateAzureUserRoles(userIdGuid);
#endif

                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
