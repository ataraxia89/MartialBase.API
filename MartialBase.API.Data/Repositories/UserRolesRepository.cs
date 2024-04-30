// <copyright file="UserRolesRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace MartialBase.API.Data.Repositories
{
    /// <inheritdoc />
    public class UserRolesRepository : IUserRolesRepository
    {
        private readonly MartialBaseDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRolesRepository"/> class.
        /// </summary>
        /// <param name="context">The <see cref="MartialBaseDbContext"/> to be used.</param>
        public UserRolesRepository(MartialBaseDbContext context) => _context = context;

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(Guid userRoleId) => await _context.UserRoles.AnyAsync(ur => ur.Id == userRoleId);

        /// <inheritdoc />
        public async Task<List<UserRole>> GetAllAsync() => await _context.UserRoles.ToListAsync();
    }
}
