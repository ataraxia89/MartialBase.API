// <copyright file="ArtGradesController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.AuthTools.Attributes;
using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.ArtGrades;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.ArtGrades;
using MartialBase.API.Tools;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace MartialBase.API.Controllers
{
    [Authorize]
    [PersonIdRequiredAsync]
    [Route("artgrades")]
    [RequiredScope("query")]
    public class ArtGradesController : MartialBaseControllerBase
    {
        private readonly IArtsRepository _artsRepository;
        private readonly IArtGradesRepository _artGradesRepository;
        private readonly IMartialBaseUserHelper _martialBaseUserHelper;
        private readonly IOrganisationsRepository _organisationsRepository;

        public ArtGradesController(
            IArtsRepository artsRepository,
            IArtGradesRepository artGradesRepository,
            IMartialBaseUserHelper martialBaseUserHelper,
            IOrganisationsRepository organisationsRepository,
            IWebHostEnvironment hostEnvironment)
        {
            _artsRepository = artsRepository;
            _artGradesRepository = artGradesRepository;
            _martialBaseUserHelper = martialBaseUserHelper;
            _organisationsRepository = organisationsRepository;
            HostEnvironment = hostEnvironment;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ArtGradeDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetArtGradesAsync(string artId, string organisationId)
        {
            try
            {
                if (artId == null)
                {
                    return BadRequest("No art ID parameter specified.");
                }

                if (organisationId == null)
                {
                    return BadRequest("No organisation ID parameter specified.");
                }

                var artIdGuid = new Guid(artId);
                var organisationIdGuid = new Guid(organisationId);

                if (!await _artsRepository.ExistsAsync(artIdGuid))
                {
                    throw new EntityIdNotFoundException("Art", artIdGuid);
                }

                await _martialBaseUserHelper.ValidateUserHasMemberAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                return Ok(await _artGradesRepository.GetAllAsync(artIdGuid, organisationIdGuid));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("{artGradeId}")]
        [ProducesResponseType(typeof(ArtGradeDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetArtGradeAsync(string artGradeId)
        {
            try
            {
                var artGradeIdGuid = new Guid(artGradeId);

                if (!await _artGradesRepository.ExistsAsync(artGradeIdGuid))
                {
                    throw new EntityIdNotFoundException("Art grade", artGradeIdGuid);
                }

                ArtGradeDTO artGradeDTO = await _artGradesRepository.GetAsync(artGradeIdGuid);

                var organisationIdGuid = new Guid(artGradeDTO.OrganisationId);

                await _martialBaseUserHelper.ValidateUserHasMemberAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                return Ok(artGradeDTO);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ArtGradeDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateArtGradeAsync([FromBody] CreateArtGradeDTO createArtGradeDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                var artIdGuid = new Guid(createArtGradeDTO.ArtId);
                var organisationIdGuid = new Guid(createArtGradeDTO.OrganisationId);

                if (!await _artsRepository.ExistsAsync(artIdGuid))
                {
                    throw new EntityIdNotFoundException("Art", artIdGuid);
                }

                if (!await _organisationsRepository.ExistsAsync(organisationIdGuid))
                {
                    throw new EntityIdNotFoundException("Organisation", organisationIdGuid);
                }

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                var createArtGradeInternalDTO = new CreateArtGradeInternalDTO
                {
                    ArtId = artIdGuid,
                    OrganisationId = organisationIdGuid,
                    GradeLevel = createArtGradeDTO.GradeLevel,
                    Description = createArtGradeDTO.Description
                };

                ArtGradeDTO createdArtGrade = await _artGradesRepository.CreateAsync(createArtGradeInternalDTO);

                if (!await _artGradesRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return StatusCode(StatusCodes.Status201Created, createdArtGrade);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPut("{artGradeId}")]
        [ProducesResponseType(typeof(ArtGradeDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateArtGradeAsync(string artGradeId, [FromBody] UpdateArtGradeDTO updateArtGradeDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ModelStateTools.PrepareModelStateErrors(ModelState));
                }

                var artGradeIdGuid = new Guid(artGradeId);

                if (!await _artGradesRepository.ExistsAsync(artGradeIdGuid))
                {
                    throw new EntityIdNotFoundException("Art grade", artGradeIdGuid);
                }

                Guid organisationIdGuid = await _artGradesRepository.GetArtGradeOrganisationIdAsync(artGradeIdGuid);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                ArtGradeDTO updatedArtGrade =
                    await _artGradesRepository.UpdateAsync(artGradeIdGuid, updateArtGradeDTO);

                if (!await _artGradesRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                return Ok(updatedArtGrade);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpDelete("{artGradeId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteArtGradeAsync(string artGradeId)
        {
            try
            {
                var artGradeIdGuid = new Guid(artGradeId);

                if (!await _artGradesRepository.ExistsAsync(artGradeIdGuid))
                {
                    throw new EntityIdNotFoundException("Art grade", artGradeIdGuid);
                }

                Guid organisationIdGuid =
                    await _artGradesRepository.GetArtGradeOrganisationIdAsync(artGradeIdGuid);

                await _martialBaseUserHelper.ValidateUserHasAdminAccessToOrganisationAsync(
                    RequestingPersonId, organisationIdGuid);

                await _artGradesRepository.DeleteAsync(artGradeIdGuid);

                if (!await _artGradesRepository.SaveChangesAsync())
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
