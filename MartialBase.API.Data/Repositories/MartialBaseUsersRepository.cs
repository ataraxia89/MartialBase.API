// <copyright file="MartialBaseUsersRepository.cs" company="Martialtech®">
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
using MartialBase.API.Models.DTOs.MartialBaseUsers;
using MartialBase.API.Tools;

using Microsoft.EntityFrameworkCore;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.Data.Repositories
{
    /// <inheritdoc />
    public class MartialBaseUsersRepository : IMartialBaseUsersRepository
    {
        public const int INVITATION_CODE_LENGTH = 7;

        private readonly MartialBaseDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartialBaseUsersRepository"/> class.
        /// </summary>
        /// <param name="context">The <see cref="MartialBaseDbContext"/> to be used.</param>
        public MartialBaseUsersRepository(MartialBaseDbContext context) => _context = context;

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(Guid id) => await _context.MartialBaseUsers.AnyAsync(mbu =>
                                                                    mbu.Id == id);

        /// <inheritdoc />
        public async Task<MartialBaseUserDTO> CreateAsync(CreateMartialBaseUserDTO createDTO)
        {
            var person = createDTO.PersonId != null
                ? await _context.People.FirstAsync(p => p.Id == new Guid(createDTO.PersonId))
                : (await _context.People.AddAsync(ModelMapper.GetPerson(createDTO.Person))).Entity;

            var martialBaseUser = (await _context.MartialBaseUsers.AddAsync(
                    new MartialBaseUser
                    {
                        PersonId = person.Id,
                        AzureId = new Guid(createDTO.AzureId),
                        InvitationCode = createDTO.InvitationCode
                    }))
                .Entity;

            martialBaseUser.Person = person;

            return ModelMapper.GetMartialBaseUserDTO(martialBaseUser);
        }

        /// <inheritdoc />
        public async Task<List<MartialBaseUserDTO>> GetAllAsync()
        {
            var martialBaseUsers = new List<MartialBaseUserDTO>();

            foreach (MartialBaseUser martialBaseUser in await _context.MartialBaseUsers
                .Include(mbu => mbu.Person)
                .ThenInclude(p => p.Address)
                .ToListAsync())
            {
                martialBaseUsers.Add(ModelMapper.GetMartialBaseUserDTO(martialBaseUser));
            }

            return martialBaseUsers;
        }

        /// <inheritdoc />
        public async Task<MartialBaseUserDTO> GetAsync(Guid id)
        {
            MartialBaseUser martialBaseUser = await _context.MartialBaseUsers
                .Include(mbu => mbu.Person)
                .ThenInclude(p => p.Address)
                .FirstAsync(mbu => mbu.Id == id);

            return ModelMapper.GetMartialBaseUserDTO(martialBaseUser);
        }

        /// <inheritdoc />
        public Task<MartialBaseUserDTO> UpdateAsync(Guid id, UpdateMartialBaseUserDTO updateDTO) =>
            throw new NotSupportedException();

        /// <inheritdoc />
        public Task DeleteAsync(Guid id) =>
            throw new NotSupportedException();

        /// <inheritdoc />
        public async Task<Guid> GetUserIdForPersonAsync(Guid personId) => (await _context.MartialBaseUsers.FirstAsync(mbu =>
                                                                       mbu.PersonId == personId)).Id;

        /// <inheritdoc />
        public async Task<Guid?> GetPersonIdForUserAsync(Guid userId) => (await _context.MartialBaseUsers.FirstOrDefaultAsync(mbu =>
                                                                      mbu.Id == userId))?.PersonId;

        /// <inheritdoc />
        public async Task<string> GenerateInvitationCodeAsync(Guid userId)
        {
            string invitationCode;

            do
            {
                invitationCode = RandomData.GetRandomString(INVITATION_CODE_LENGTH, true, false);
            }
            while (await _context.MartialBaseUsers.AnyAsync(mbu => mbu.InvitationCode == invitationCode));

            (await _context.MartialBaseUsers.FirstAsync(mbu => mbu.Id == userId)).InvitationCode = invitationCode;

            return invitationCode;
        }

        /// <inheritdoc />
        public async Task<Guid?> GetPersonIdForAzureUserAsync(Guid azureUserId, string invitationCode)
        {
            Guid? personId = (await _context.MartialBaseUsers.FirstOrDefaultAsync(mbu =>
                mbu.AzureId == azureUserId))?.PersonId;

            if (personId != null)
            {
                return personId;
            }

            MartialBaseUser user = await _context.MartialBaseUsers.FirstOrDefaultAsync(
                mbu =>
                    mbu.InvitationCode != null &&
                    mbu.InvitationCode == invitationCode);

            if (user == null)
            {
                return null;
            }

            personId = user.PersonId;

            user.AzureId = azureUserId;
            user.InvitationCode = null;

            await SetUserRolesAsync(user.Id, user.PersonId);

            await SaveChangesAsync();

            return personId;
        }

        /// <inheritdoc />
        public async Task<Guid?> GetAzureIdForUserAsync(Guid martialBaseUserId) =>
            (await _context.MartialBaseUsers.FirstOrDefaultAsync(
                mbu =>
                    mbu.Id == martialBaseUserId))?.AzureId;

        /// <inheritdoc />
        public async Task DisassociateAzureUserAsync(Guid userId)
        {
            MartialBaseUser martialBaseUser = await _context.MartialBaseUsers.FirstAsync(mbu => mbu.Id == userId);

            martialBaseUser.AzureId = null;
            martialBaseUser.InvitationCode = null;
        }

        /// <inheritdoc />
        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() >= 0;

        private async Task SetUserRolesAsync(Guid userId, Guid personId)
        {
            var schoolStudentRecords =
                await _context.SchoolStudents.Where(ss => ss.StudentId == personId).ToListAsync();

            if (schoolStudentRecords.Any())
            {
                Guid schoolMemberRoleId = (await _context.UserRoles.FirstAsync(ur => ur.Name == UserRoles.SchoolMember)).Id;

                await _context.MartialBaseUserRoles.AddAsync(
                    new MartialBaseUserRole { MartialBaseUserId = userId, UserRoleId = schoolMemberRoleId });
            }

            if (schoolStudentRecords.Any(ss => ss.IsInstructor))
            {
                Guid schoolInstructorRoleId = (await _context.UserRoles.FirstAsync(ur => ur.Name == UserRoles.SchoolInstructor)).Id;

                await _context.MartialBaseUserRoles.AddAsync(
                    new MartialBaseUserRole { MartialBaseUserId = userId, UserRoleId = schoolInstructorRoleId });
            }

            if (schoolStudentRecords.Any(ss => ss.IsSecretary))
            {
                Guid schoolSecretaryRoleId = (await _context.UserRoles.FirstAsync(ur => ur.Name == UserRoles.SchoolSecretary)).Id;

                await _context.MartialBaseUserRoles.AddAsync(
                    new MartialBaseUserRole { MartialBaseUserId = userId, UserRoleId = schoolSecretaryRoleId });
            }

            if (await _context.Schools.AnyAsync(s => s.HeadInstructorId == personId))
            {
                Guid headInstructorRoleId = (await _context.UserRoles.FirstAsync(ur => ur.Name == UserRoles.SchoolHeadInstructor)).Id;

                await _context.MartialBaseUserRoles.AddAsync(
                    new MartialBaseUserRole { MartialBaseUserId = userId, UserRoleId = headInstructorRoleId });
            }

            var organisationPeopleRecords =
                await _context.OrganisationPeople.Where(op => op.PersonId == personId).ToListAsync();

            if (organisationPeopleRecords.Any())
            {
                Guid organisationMemberRoleId = (await _context.UserRoles.FirstAsync(ur => ur.Name == UserRoles.OrganisationMember)).Id;

                await _context.MartialBaseUserRoles.AddAsync(
                    new MartialBaseUserRole { MartialBaseUserId = userId, UserRoleId = organisationMemberRoleId });
            }

            if (organisationPeopleRecords.Any(op => op.IsOrganisationAdmin))
            {
                Guid organisationAdminRoleId = (await _context.UserRoles.FirstAsync(ur => ur.Name == UserRoles.OrganisationAdmin)).Id;

                await _context.MartialBaseUserRoles.AddAsync(
                    new MartialBaseUserRole { MartialBaseUserId = userId, UserRoleId = organisationAdminRoleId });
            }
        }
    }
}
