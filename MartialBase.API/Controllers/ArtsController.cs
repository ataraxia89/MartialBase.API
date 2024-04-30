// <copyright file="ArtsController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Arts;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace MartialBase.API.Controllers
{
    /// <summary>
    /// Endpoints used to retrieve <see cref="Art">Arts</see>.
    /// </summary>
    /// <remarks>
    /// Retrieving arts requires basic authorization, managing arts is not available via the API.
    /// </remarks>
    [Authorize]
    [Route("arts")]
    [RequiredScope("query")]
    public class ArtsController : MartialBaseControllerBase
    {
        private readonly IArtsRepository _artsRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtsController"/> class.
        /// </summary>
        /// <param name="artsRepository">The current <see cref="IArtsRepository"/> instance.</param>
        /// <param name="hostEnvironment">The current <see cref="IWebHostEnvironment"/> instance.</param>
        public ArtsController(
            IArtsRepository artsRepository,
            IWebHostEnvironment hostEnvironment)
        {
            _artsRepository = artsRepository;
            HostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for a list of <see cref="Art">Arts</see>.
        /// </summary>
        /// <remarks>
        /// Requesting user must have an auth token but no <see cref="MartialBaseUserRole"/> is required.
        /// </remarks>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing an <see cref="IList{T}"/> of <see cref="ArtDTO"/> objects.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked.</item>
        /// </list>
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ArtDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetArtsAsync()
        {
            return Ok(await _artsRepository.GetAllAsync());
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for a specified <see cref="Art"/>.
        /// </summary>
        /// <remarks>
        /// Requesting user must have an auth token but no <see cref="MartialBaseUserRole"/> is required.
        /// </remarks>
        /// <param name="artId">The ID of the <see cref="Art"/> to be retrieved.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing an <see cref="ArtDTO"/> object.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="artId"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpGet("{artId}")]
        [ProducesResponseType(typeof(ArtDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetArtAsync(string artId)
        {
            var artIdGuid = new Guid(artId);

            if (!await _artsRepository.ExistsAsync(artIdGuid))
            {
                throw new EntityIdNotFoundException("Art", artIdGuid);
            }

            return Ok(await _artsRepository.GetAsync(artIdGuid));
        }
    }
}
