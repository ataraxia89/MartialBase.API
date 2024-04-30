// <copyright file="AuthController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Net;

using MartialBase.API.AuthTools.Attributes;
using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Exceptions;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.Enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace MartialBase.API.Controllers
{
    /// <summary>
    /// Endpoints used to manage MartialBase <see cref="MartialBaseUser">users</see>.
    /// </summary>
    [Authorize]
    [RequiredScope("query")]
    public class AuthController : MartialBaseControllerBase
    {
        private readonly IMartialBaseUsersRepository _martialBaseUsersRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="martialBaseUsersRepository">The current <see cref="IMartialBaseUsersRepository"/> instance.</param>
        /// <param name="hostEnvironment">The current <see cref="IWebHostEnvironment"/> instance.</param>
        public AuthController(
            IMartialBaseUsersRepository martialBaseUsersRepository,
            IWebHostEnvironment hostEnvironment)
        {
            _martialBaseUsersRepository = martialBaseUsersRepository;
            HostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.7">DELETE</see>
        /// request to remove any Azure-registered user record from the specified <see cref="MartialBaseUser"/>.
        /// </summary>
        /// <param name="userId">The ID of the <see cref="MartialBaseUser"/> to remove Azure associations from.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>A <see cref="NoContentResult"/> if deletion is successful.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked, or it does not contain a valid Azure user ID.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user is not in the <see cref="UserRoles.SystemAdmin">SystemAdmin</see> role.</item>
        /// <item>A <see cref="StatusCodes.Status403Forbidden">Forbidden</see> result if the requesting user does not have a registered <see cref="Person"/> record.</item>
        /// <item>A <see cref="BadRequestResult"/> if no <paramref name="userId"/> parameter is provided.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="userId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [Route("login")]
        [HttpDelete]
        [PersonIdRequiredAsync]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> BlockUserAccessAsync(string userId)
        {
            try
            {
                if (!RequestingUserRoles.Any(role => role is UserRoles.SystemAdmin or UserRoles.Thanos))
                {
                    throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("No user ID parameter specified.");
                }

                var martialBaseUserId = new Guid(userId);

                if (!await _martialBaseUsersRepository.ExistsAsync(martialBaseUserId))
                {
                    throw new EntityIdNotFoundException("User", userId);
                }

                await _martialBaseUsersRepository.DisassociateAzureUserAsync(martialBaseUserId);

                if (!await _martialBaseUsersRepository.SaveChangesAsync())
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
