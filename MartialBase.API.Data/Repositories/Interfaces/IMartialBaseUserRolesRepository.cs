// <copyright file="IMartialBaseUserRolesRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.UserRoles;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    /// <summary>
    /// Interface for the <see cref="MartialBaseUserRole"/> repository.
    /// </summary>
    public interface IMartialBaseUserRolesRepository
    {
        /// <summary>
        /// Checks if the <see cref="MartialBaseUser"/> relating to a provided Azure user ID has any of the
        /// allowed roles specified.
        /// </summary>
        /// <param name="azureUserId">The Azure ID relating to the relevant <see cref="MartialBaseUser"/>.</param>
        /// <param name="allowedRole">The <see cref="MartialBaseUserRole"/>s which is authorized.</param>
        /// <returns><c>true</c> if the user has the specified role, otherwise <c>false</c>.</returns>
        Task<bool> AzureUserHasRequiredRoleAsync(Guid azureUserId, string allowedRole);

        /// <summary>
        /// Checks if the <see cref="MartialBaseUser"/> relating to a provided Azure user ID has any of the
        /// allowed roles specified.
        /// </summary>
        /// <param name="azureUserId">The Azure ID relating to the relevant <see cref="MartialBaseUser"/>.</param>
        /// <param name="allowedRoles">The <see cref="MartialBaseUserRole"/>s which are authorized.</param>
        /// <returns><c>true</c> if the user has any of the specified roles, otherwise <c>false</c>.</returns>
        Task<bool> AzureUserHasRequiredRoleAsync(Guid azureUserId, List<string> allowedRoles);

        /// <summary>
        /// Retrieves all <see cref="MartialBaseUserRole"/>s assigned to the provided
        /// <see cref="MartialBaseUser"/> ID.
        /// </summary>
        /// <param name="userId">The ID relating to the relevant <see cref="MartialBaseUser"/>.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="MartialBaseUserRole"/>s assigned to the specified user.</returns>
        Task<List<UserRoleDTO>> GetRolesForUserAsync(Guid userId);

        /// <summary>
        /// Adds a <see cref="MartialBaseUserRole"/> to a specified <see cref="MartialBaseUser"/>.
        /// </summary>
        /// <param name="userId">The ID relating to the relevant <see cref="MartialBaseUser"/>.</param>
        /// <param name="roleId">The ID of the <see cref="MartialBaseUserRole"/> to be assigned to the <see cref="MartialBaseUser"/>.</param>
        Task AddRoleToUserAsync(Guid userId, Guid roleId);

        /// <summary>
        /// Adds a <see cref="MartialBaseUserRole"/> to a specified <see cref="MartialBaseUser"/>.
        /// </summary>
        /// <param name="userId">The ID relating to the relevant <see cref="MartialBaseUser"/>.</param>
        /// <param name="roleName">The name of the <see cref="MartialBaseUserRole"/> to be assigned to the <see cref="MartialBaseUser"/>.</param>
        Task AddRoleToUserAsync(Guid userId, string roleName);

        /// <summary>
        /// Removes a <see cref="MartialBaseUserRole"/> from a specified <see cref="MartialBaseUser"/>.
        /// </summary>
        /// <param name="userId">The ID relating to the relevant <see cref="MartialBaseUser"/>.</param>
        /// <param name="roleId">The ID of the <see cref="MartialBaseUserRole"/> to be removed from the <see cref="MartialBaseUser"/>.</param>
        Task RemoveRoleFromUserAsync(Guid userId, Guid roleId);

        /// <summary>
        /// Ensures that a collection of specified <see cref="MartialBaseUserRole"/>s are assigned to a specified <see cref="MartialBaseUser"/>.
        /// </summary>
        /// <param name="userId">The ID relating to the relevant <see cref="MartialBaseUser"/>.</param>
        /// <param name="roleIds">The IDs of the <see cref="MartialBaseUserRole"/>s to be assigned to the <see cref="MartialBaseUser"/>.</param>
        Task SetRolesForUserAsync(Guid userId, List<Guid> roleIds);

        /// <summary>
        /// Saves changes to the <see cref="MartialBaseDbContext"/>.
        /// </summary>
        /// <returns><c>true</c> if the changes are saved successfully, otherwise <c>false</c>.</returns>
        Task<bool> SaveChangesAsync();
    }
}
