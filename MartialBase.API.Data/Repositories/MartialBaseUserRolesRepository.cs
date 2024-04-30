// <copyright file="MartialBaseUserRolesRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.UserRoles;

using Microsoft.EntityFrameworkCore;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.Data.Repositories
{
    /// <inheritdoc />
    public class MartialBaseUserRolesRepository : IMartialBaseUserRolesRepository
    {
        private readonly MartialBaseDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartialBaseUserRolesRepository"/> class.
        /// </summary>
        /// <param name="context">The <see cref="MartialBaseDbContext"/> to be used.</param>
        public MartialBaseUserRolesRepository(MartialBaseDbContext context) => _context = context;

        /// <inheritdoc />
        public async Task<bool> AzureUserHasRequiredRoleAsync(Guid azureUserId, string allowedRole) => await AzureUserHasRequiredRoleAsync(azureUserId, new List<string> { allowedRole });

        /// <inheritdoc />
        public async Task<bool> AzureUserHasRequiredRoleAsync(Guid azureUserId, List<string> allowedRoles)
        {
            allowedRoles.Add(UserRoles.Thanos);

            var userRoleIds = await _context.UserRoles
                .Where(ur =>
                    allowedRoles.Contains(ur.Name))
                .Select(ur => ur.Id)
                .ToListAsync();

            return await _context.MartialBaseUserRoles.AnyAsync(mur =>
                mur.MartialBaseUser.AzureId == azureUserId &&
                userRoleIds.Contains(mur.UserRoleId));
        }

        /// <inheritdoc />
        public async Task<List<UserRoleDTO>> GetRolesForUserAsync(Guid userId)
        {
            var userRoleDTOs = new List<UserRoleDTO>();

            foreach (UserRole userRole in await _context.MartialBaseUserRoles
                .Where(mur => mur.MartialBaseUserId == userId)
                .Select(mur => mur.UserRole)
                         .ToListAsync())
            {
                userRoleDTOs.Add(ModelMapper.GetUserRoleDTO(userRole));
            }

            return userRoleDTOs;
        }

        /// <inheritdoc />
        public async Task AddRoleToUserAsync(Guid userId, string roleName)
        {
            Guid userRoleId = (await _context.UserRoles.FirstAsync(ur =>
                ur.Name == roleName)).Id;

            await AddRoleToUserAsync(userId, userRoleId);
        }

        /// <inheritdoc />
        public async Task AddRoleToUserAsync(Guid userId, Guid roleId)
        {
            if (await _context.MartialBaseUserRoles.AnyAsync(mur =>
                mur.MartialBaseUserId == userId &&
                mur.UserRoleId == roleId))
            {
                return;
            }

            await ValidateUserIdAsync(userId);
            await ValidateRoleIdAsync(roleId);

            await _context.MartialBaseUserRoles.AddAsync(new MartialBaseUserRole
            {
                Id = Guid.NewGuid(),
                MartialBaseUserId = userId,
                UserRoleId = roleId
            });
        }

        /// <inheritdoc />
        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            await ValidateUserIdAsync(userId);
            await ValidateRoleIdAsync(roleId);

            if (!await _context.MartialBaseUserRoles.AnyAsync(mur =>
                mur.MartialBaseUserId == userId &&
                mur.UserRoleId == roleId))
            {
                return;
            }

            _context.MartialBaseUserRoles.Remove(_context.MartialBaseUserRoles.First(mur =>
                mur.MartialBaseUserId == userId &&
                mur.UserRoleId == roleId));
        }

        /// <inheritdoc />
        public async Task SetRolesForUserAsync(Guid userId, List<Guid> roleIds)
        {
            var addRoles = new List<MartialBaseUserRole>();
            var removeRoles = new List<MartialBaseUserRole>();

            await ValidateUserIdAsync(userId);
            await ValidateRoleIdsAsync(roleIds);

            foreach (UserRole userRole in await _context.UserRoles.ToListAsync())
            {
                if (!roleIds.Contains(userRole.Id))
                {
                    MartialBaseUserRole removeRole =
                        await _context.MartialBaseUserRoles.FirstOrDefaultAsync(
                            mur =>
                                mur.MartialBaseUserId == userId &&
                                mur.UserRoleId == userRole.Id);

                    if (removeRole != null)
                    {
                        removeRoles.Add(removeRole);
                    }
                }
                else
                {
                    if (!await _context.MartialBaseUserRoles.AnyAsync(mur =>
                        mur.MartialBaseUserId == userId &&
                        mur.UserRoleId == userRole.Id))
                    {
                        addRoles.Add(new MartialBaseUserRole
                        {
                            Id = Guid.NewGuid(),
                            MartialBaseUserId = userId,
                            UserRoleId = userRole.Id
                        });
                    }
                }
            }

            if (addRoles.Count > 0)
            {
                await _context.MartialBaseUserRoles.AddRangeAsync(addRoles);
            }

            if (removeRoles.Count > 0)
            {
                _context.MartialBaseUserRoles.RemoveRange(removeRoles);
            }
        }

        /// <inheritdoc />
        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() >= 0;

        private async Task ValidateUserIdAsync(Guid userId)
        {
            if (!await _context.MartialBaseUsers.AnyAsync(mbu => mbu.Id == userId))
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
        }

        private async Task ValidateRoleIdAsync(Guid roleId)
        {
            if (!await _context.UserRoles.AnyAsync(ur => ur.Id == roleId))
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
        }

        private async Task ValidateRoleIdsAsync(List<Guid> roleIds)
        {
            foreach (Guid roleId in roleIds)
            {
                if (!await _context.UserRoles.AnyAsync(ur => ur.Id == roleId))
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
            }
        }
    }
}
