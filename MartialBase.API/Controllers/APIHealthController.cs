// <copyright file="APIHealthController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;

namespace MartialBase.API.Controllers
{
    /// <summary>
    /// A controller specifically used to provide API health check responses.
    /// </summary>
    /// <remarks>
    /// No authentication/authorization is required to contact these endpoints.
    /// </remarks>
    [Route("health")]
    public class APIHealthController : MartialBaseControllerBase
    {
        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for the root endpoint (/health).
        /// </summary>
        /// <remarks>
        /// No authentication/authorization is required to contact this endpoint.
        /// </remarks>
        /// <returns>An <see cref="OkResult"/> indicating the API is running.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Index() => Ok();
    }
}
