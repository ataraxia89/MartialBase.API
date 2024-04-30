// <copyright file="OrganisationsRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Organisations;
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.DTOs.People;

using Microsoft.EntityFrameworkCore;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.Data.Repositories
{
    public class OrganisationsRepository : IOrganisationsRepository
    {
        private readonly MartialBaseDbContext _context;

        public OrganisationsRepository(MartialBaseDbContext context) => _context = context;

        public async Task<bool> ExistsAsync(Guid id) => await _context.Organisations.AnyAsync(o => o.Id == id);

        public async Task<OrganisationDTO> CreateAsync(CreateOrganisationInternalDTO createDTO)
        {
            var newOrganisation = (await _context.Organisations.AddAsync(ModelMapper.GetOrganisation(createDTO))).Entity;

            if (newOrganisation.ParentId != null)
            {
                newOrganisation.Parent = await _context.Organisations.FirstAsync(o => o.Id == newOrganisation.ParentId);
            }

            return ModelMapper.GetOrganisationDTO(newOrganisation);
        }

        public async Task<PersonOrganisationDTO> CreateOrganisationWithNewPersonAsAdminAsync(CreateOrganisationInternalDTO createOrganisationDTO, CreatePersonInternalDTO createPersonDTO, Guid azureId)
        {
            if (createOrganisationDTO == null)
            {
                throw new ArgumentNullException(nameof(createOrganisationDTO));
            }

            if (createOrganisationDTO.Address != null)
            {
                Address organisationAddress = ModelMapper.GetAddress(createOrganisationDTO.Address);

                await _context.Addresses.AddAsync(organisationAddress);
            }

            var organisation = (await _context.Organisations.AddAsync(ModelMapper.GetOrganisation(createOrganisationDTO))).Entity;

            var newOrganisationDTO = ModelMapper.GetOrganisationDTO(organisation);

            var person = ModelMapper.GetPerson(createPersonDTO);

            await _context.Addresses.AddAsync(person.Address);
            await _context.People.AddAsync(person);

            await _context.OrganisationPeople.AddAsync(new OrganisationPerson
            {
                Id = Guid.NewGuid(),
                OrganisationId = organisation.Id,
                PersonId = person.Id,
                IsOrganisationAdmin = true
            });

            MartialBaseUser newUser = await CreateMartialBaseUserAsync(person.Id, azureId);

            Guid userRoleId = (await _context.UserRoles.FirstAsync(ur =>
                ur.Name == UserRoles.OrganisationAdmin)).Id;

            await _context.MartialBaseUserRoles.AddAsync(new MartialBaseUserRole
            {
                Id = Guid.NewGuid(),
                MartialBaseUserId = newUser.Id,
                UserRoleId = userRoleId
            });

            var newPersonDTO = ModelMapper.GetPersonDTO(person);

            return new PersonOrganisationDTO(newPersonDTO, newOrganisationDTO, true);
        }

        public async Task<List<OrganisationDTO>> GetAllAsync()
        {
            var organisations = new List<OrganisationDTO>();

            foreach (Organisation organisation in await _context.Organisations
                         .Include(o => o.Parent)
                         .Include(o => o.Address)
                         .OrderBy(o => o.Initials)
                         .ToListAsync())
            {
                organisations.Add(ModelMapper.GetOrganisationDTO(organisation));
            }

            return organisations;
        }

        public async Task<List<OrganisationDTO>> GetChildOrganisationsAsync(Guid parentOrganisationId)
        {
            var organisations = new List<OrganisationDTO>();

            foreach (Organisation organisation in await _context.Organisations
                         .Include(o => o.Parent)
                         .Include(o => o.Address)
                         .Where(o => o.ParentId == parentOrganisationId)
                         .OrderBy(o => o.Initials)
                         .ToListAsync())
            {
                organisations.Add(ModelMapper.GetOrganisationDTO(organisation));
            }

            return organisations;
        }

        public async Task<Guid?> GetOrganisationParentIdAsync(Guid organisationId) =>
            await _context.Organisations.Where(o => o.Id == organisationId).Select(o => o.ParentId)
                .FirstOrDefaultAsync();

        public async Task<List<OrganisationPersonDTO>> GetPeopleAsync(Guid organisationId)
        {
            var organisationPeople = new List<OrganisationPersonDTO>();

            foreach (OrganisationPerson organisationPerson in await _context.OrganisationPeople
                         .Include(op => op.Person)
                         .Where(op => op.OrganisationId == organisationId)
                         .OrderBy(op => op.Person.FirstName)
                         .ThenBy(op => op.Person.LastName)
                         .ToListAsync())
            {
                organisationPeople.Add(
                    new OrganisationPersonDTO(
                        new PersonDTO(
                            organisationPerson.Person.Id,
                            organisationPerson.Person.Title,
                            organisationPerson.Person.FirstName,
                            null,
                            organisationPerson.Person.LastName,
                            null,
                            null,
                            null,
                            null),
                        organisationPerson.IsOrganisationAdmin));
            }

            return organisationPeople;
        }

        public async Task<bool> PersonCanAccessOrganisationAsync(Guid organisationId, Guid personId)
        {
            if (await _context.Organisations
                    .Where(o => o.Id == organisationId)
                    .Select(o => o.IsPublic)
                    .FirstAsync())
            {
                return true;
            }

            return await _context.OrganisationPeople.AnyAsync(op =>
                op.OrganisationId == organisationId && op.PersonId == personId);
        }

        public async Task AddOrganisationPersonAsync(Guid organisationId, Guid personId, bool isAdmin)
        {
            if (await _context.OrganisationPeople.AnyAsync(
                os => os.OrganisationId == organisationId && os.PersonId == personId))
            {
                OrganisationPerson organisationPerson = await _context.OrganisationPeople.FirstAsync(op =>
                    op.OrganisationId == organisationId && op.PersonId == personId);

                organisationPerson.IsOrganisationAdmin = isAdmin;
            }
            else
            {
                await _context.OrganisationPeople.AddAsync(new OrganisationPerson
                {
                    Id = Guid.NewGuid(),
                    OrganisationId = organisationId,
                    PersonId = personId,
                    IsOrganisationAdmin = isAdmin
                });
            }
        }

        public async Task RemoveOrganisationPersonAsync(Guid organisationId, Guid personId)
        {
            if (await _context.OrganisationPeople.CountAsync(op => op.PersonId == personId) == 1)
            {
                throw new OrphanPersonEntityException();
            }

            _context.OrganisationPeople.Remove(await _context.OrganisationPeople.FirstAsync(os =>
                os.OrganisationId == organisationId && os.PersonId == personId));
        }

        public async Task DemoteOrganisationAdminAsync(Guid organisationId, Guid personId)
        {
            OrganisationPerson organisationPerson =
                await _context.OrganisationPeople.FirstAsync(op =>
                    op.OrganisationId == organisationId && op.PersonId == personId);

            organisationPerson.IsOrganisationAdmin = false;
        }

        public async Task<bool> PersonIsOrganisationAdminAsync(Guid organisationId, Guid personId) => await _context.OrganisationPeople.AnyAsync(op =>
                                                                                                   op.OrganisationId == organisationId && op.PersonId == personId && op.IsOrganisationAdmin);

        public async Task<List<PersonOrganisationDTO>> GetOrganisationsForPersonAsync(Guid personId)
        {
            var personOrganisations =
                await _context.OrganisationPeople
                    .Include(op => op.Organisation)
                    .Where(op => op.PersonId == personId).ToListAsync();

            var personOrganisationDTOs = new List<PersonOrganisationDTO>();

            foreach (OrganisationPerson personOrganisation in personOrganisations)
            {
                personOrganisationDTOs.Add(new PersonOrganisationDTO(
                    null,
                    new OrganisationDTO(
                        personOrganisation.Organisation.Id.ToString(),
                        personOrganisation.Organisation.Initials,
                        personOrganisation.Organisation.Name,
                        null,
                        null,
                        null),
                    personOrganisation.IsOrganisationAdmin));
            }

            return personOrganisationDTOs;
        }

        public async Task<OrganisationDTO> GetAsync(Guid id)
        {
            Organisation organisation = await _context.Organisations
                .Include(o => o.Parent)
                .Include(o => o.Address)
                .FirstAsync(o => o.Id == id);

            return ModelMapper.GetOrganisationDTO(organisation);
        }

        public async Task<OrganisationDTO> UpdateAsync(Guid id, UpdateOrganisationDTO updateDTO)
        {
            if (updateDTO == null)
            {
                throw new ArgumentNullException(nameof(updateDTO));
            }

            Organisation organisation = await _context.Organisations
                .Include(o => o.Address)
                .Include(o => o.Parent)
                .FirstAsync(o => o.Id == id);

            organisation.Initials = updateDTO.Initials;
            organisation.Name = updateDTO.Name;

            return ModelMapper.GetOrganisationDTO(organisation);
        }

        public async Task ChangeOrganisationParentAsync(Guid organisationId, Guid? parentOrganisationId)
        {
            Organisation organisation = await _context.Organisations.FirstAsync(o => o.Id == organisationId);

            organisation.ParentId = parentOrganisationId;
        }

        public async Task<AddressDTO> ChangeOrganisationAddressAsync(Guid organisationId, CreateAddressDTO createAddressDTO)
        {
            Organisation organisation = await _context.Organisations.FirstAsync(o => o.Id == organisationId);

            if (organisation.AddressId != null)
            {
                _context.Addresses.Remove(await _context.Addresses.FirstAsync(a => a.Id == organisation.AddressId));
            }

            var address = ModelMapper.GetAddress(createAddressDTO);

            await _context.Addresses.AddAsync(address);

            organisation.AddressId = address.Id;

            return ModelMapper.GetAddressDTO(address);
        }

        public async Task DeleteAsync(Guid id)
        {
            foreach (OrganisationPerson organisationPerson in await _context.OrganisationPeople
                .Where(op => op.OrganisationId == id).ToListAsync())
            {
                if (await _context.OrganisationPeople.CountAsync(op =>
                        op.PersonId == organisationPerson.PersonId) == 1)
                {
                    throw new OrphanPersonEntityException();
                }

                _context.OrganisationPeople.Remove(organisationPerson);
            }

            if (await _context.Schools.AnyAsync(s => s.OrganisationId == id))
            {
                throw new OrphanSchoolEntityException();
            }

            Organisation organisation = await _context.Organisations.FirstAsync(o => o.Id == id);

            if (organisation.AddressId != null)
            {
                _context.Addresses.Remove(await _context.Addresses.FirstAsync(a => a.Id == organisation.AddressId));
            }

            _context.Organisations.Remove(organisation);
        }

        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() >= 0;

        /// <summary>
        /// Creates a <see cref="MartialBaseUser"/> object and assigns the provided Azure ID.
        /// </summary>
        /// <remarks>
        /// This method should ONLY be used by controllers which create <see cref="Person">People</see>. It should
        /// NOT be used by any controller that is not responsible for managing <see cref="Person">People</see>
        /// objects.
        /// </remarks>
        /// <param name="personId">The ID of the <see cref="Person"/> to create an associated record for.</param>
        /// <param name="azureId">The Azure ID to be assigned to the new user.</param>
        /// <returns>The newly-created <see cref="MartialBaseUser"/>.</returns>
        private async Task<MartialBaseUser> CreateMartialBaseUserAsync(Guid personId, Guid azureId)
        {
            var newUser = new MartialBaseUser
            {
                Id = Guid.NewGuid(),
                PersonId = personId,
                AzureId = azureId
            };

            await _context.MartialBaseUsers.AddAsync(newUser);

            return newUser;
        }
    }
}
