// <copyright file="CountriesController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Countries;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace MartialBase.API.Controllers
{
    /// <summary>
    /// Endpoints used to retrieve <see cref="Country">Countries</see>.
    /// </summary>
    /// <remarks>All endpoints require basic authorization but no <see cref="MartialBaseUserRole"/> is required.</remarks>
    [Authorize]
    [Route("countries")]
    [RequiredScope("query")]
    public class CountriesController : MartialBaseControllerBase
    {
        private readonly ICountriesRepository _countriesRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountriesController"/> class.
        /// </summary>
        /// <param name="countriesRepository">The current <see cref="ICountriesRepository"/> instance.</param>
        /// <param name="hostEnvironment">The current <see cref="IWebHostEnvironment"/> instance.</param>
        public CountriesController(
            ICountriesRepository countriesRepository,
            IWebHostEnvironment hostEnvironment)
        {
            _countriesRepository = countriesRepository;
            HostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for a list of <see cref="CountryDTO">Countries</see>.
        /// </summary>
        /// <remarks>
        /// Requesting user must have an auth token but no <see cref="MartialBaseUserRole"/> is required.
        /// </remarks>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing a <see cref="List{T}"/> of <see cref="CountryDTO"/> objects.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked.</item>
        /// </list>
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<CountryDTO>), StatusCodes.Status200OK)]
        public IActionResult GetCountries()
        {
            return Ok(_countriesRepository.GetAll(populationLimit: 1000000));
        }

        /// <summary>
        /// Accepts a <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</see>
        /// request for a specified <see cref="Country"/>.
        /// </summary>
        /// <remarks>
        /// Requesting user must have an auth token but no <see cref="MartialBaseUserRole"/> is required.
        /// </remarks>
        /// <param name="countryCode">The <see href="https://www.iso.org/iso-3166-country-codes.html">ISO-3166</see> 3-character code of the <see cref="Country"/> to be retrieved.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>An <see cref="OkResult"/> containing a <see cref="CountryDTO"/> object.</item>
        /// <item>An <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> if any unhandled exception occurs. Exception message is only returned in a development or testing environment.</item>
        /// <item>An <see cref="UnauthorizedResult"/> if the request token has expired or been revoked.</item>
        /// <item>A <see cref="NotFoundResult"/> if the provided <paramref name="countryCode"/> cannot be found.</item>
        /// </list>
        /// </returns>
        [HttpGet("{countryCode}")]
        [ProducesResponseType(typeof(CountryDTO), StatusCodes.Status200OK)]
        public IActionResult GetCountry(string countryCode)
        {
            if (!_countriesRepository.Exists(countryCode))
            {
                throw new EntityNotFoundException($"Country code '{countryCode}' not found.");
            }

            return Ok(_countriesRepository.Get(countryCode));
        }
    }
}
