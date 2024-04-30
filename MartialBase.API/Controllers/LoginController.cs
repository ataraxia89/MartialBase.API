// <copyright file="LoginController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.MartialBaseUsers;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Tools;

using Microsoft.AspNetCore.Mvc;

namespace MartialBase.API.Controllers
{
    [Route("login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IMartialBaseUsersRepository _martialBaseUsersRepository;
        private readonly IMartialBaseUserRolesRepository _martialBaseUserRolesRepository;
        private readonly IConfiguration _configuration;

        public LoginController(
            IMartialBaseUsersRepository martialBaseBaseUsersRepository,
            IMartialBaseUserRolesRepository martialBaseUserRolesRepository,
            IConfiguration configuration)
        {
            _martialBaseUsersRepository = martialBaseBaseUsersRepository;
            _martialBaseUserRolesRepository = martialBaseUserRolesRepository;
            _configuration = configuration;
        }

#if DEBUG
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(string martialBaseUserId = null)
        {
            Guid azureId;

            if (martialBaseUserId != null)
            {
                Guid? foundAzureId =
                    await _martialBaseUsersRepository.GetAzureIdForUserAsync(new Guid(martialBaseUserId));

                if (foundAzureId != null)
                {
                    azureId = foundAzureId.Value;
                }
                else
                {
                    throw new EntityIdNotFoundException("MartialBase user", martialBaseUserId);
                }
            }
            else
            {
                azureId = Guid.NewGuid();

                var createMartialBaseUserDTO = new CreateMartialBaseUserDTO
                {
                    AzureId = azureId.ToString(),
                    Person = new CreatePersonDTO
                    {
                        Title = "Mr",
                        FirstName = "John",
                        LastName = "Smith",
                        DateOfBirth = DateTime.Now.AddYears(-25).ToString("yyyy-MM-dd"),
                        Email = "john.smith@martialbase.net",
                        MobileNo = "07777777777"
                    }
                };

                var userId = (await _martialBaseUsersRepository.CreateAsync(createMartialBaseUserDTO)).Id;

                if (!await _martialBaseUsersRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }

                foreach (UserRole userRole in UserRoles.GetRoles)
                {
                    await _martialBaseUserRolesRepository.AddRoleToUserAsync(new Guid(userId), userRole.Name);
                }

                if (!await _martialBaseUserRolesRepository.SaveChangesAsync())
                {
                    throw new DbContextSaveChangesException();
                }
            }

            return Ok(AuthTokens.GenerateAuthorizationToken(_configuration, azureId));
        }
#endif
    }
}
