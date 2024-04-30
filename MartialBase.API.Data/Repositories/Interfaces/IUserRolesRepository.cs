// <copyright file="IUserRolesRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MartialBase.API.Data.Models.EntityFramework;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    /// <summary>
    /// Interface for the <see cref="UserRole"/> repository.
    /// </summary>
    public interface IUserRolesRepository
    {
        /// <summary>
        /// Checks whether a <see cref="MartialBaseUser"/> exists according to a provided ID.
        /// </summary>
        /// <param name="userRoleId">The ID of the <see cref="UserRole"/> to be checked.</param>
        /// <returns><c>true</c> if a corresponding <see cref="UserRole"/> is found, otherwise <c>false</c>.</returns>
        Task<bool> ExistsAsync(Guid userRoleId);

        /// <summary>
        /// Retrieves all <see cref="UserRole"/>s.
        /// </summary>
        /// <returns>All <see cref="UserRole"/>s on system.</returns>
        Task<List<UserRole>> GetAllAsync();
    }
}
