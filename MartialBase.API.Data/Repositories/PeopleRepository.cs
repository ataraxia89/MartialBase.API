// <copyright file="PeopleRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Documents;
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Tools;

using Microsoft.EntityFrameworkCore;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.Data.Repositories
{
    public class PeopleRepository : IPeopleRepository
    {
        private readonly MartialBaseDbContext _context;

        public PeopleRepository(MartialBaseDbContext context) => _context = context;

        public async Task<bool> ExistsAsync(Guid id) => await _context.People.AnyAsync(p => p.Id == id);

        /// <inheritdoc />
        Task<CreatedPersonDTO> IPeopleRepository.CreateAsync(CreatePersonInternalDTO createDTO) =>
            throw new NotImplementedException();

        /// <inheritdoc />
        Task<PersonDTO> IRepository<PersonDTO, CreatePersonInternalDTO, UpdatePersonDTO>.CreateAsync(CreatePersonInternalDTO createDTO) =>
            throw new NotImplementedException();

        public async Task<CreatedPersonDTO> CreateAsync(CreatePersonInternalDTO createDTO, Guid? organisationId, Guid? schoolId)
        {
            if (organisationId == null && schoolId == null)
            {
                throw new ArgumentNullException(nameof(organisationId), "Person must be created within an organisation.");
            }

            var person = ModelMapper.GetPerson(createDTO);

            if (person.Address != null)
            {
                await _context.Addresses.AddAsync(person.Address);
            }

            await _context.People.AddAsync(person);

            if (schoolId != null)
            {
                organisationId = (await _context.Schools.FirstAsync(s => s.Id == schoolId)).OrganisationId;

                await _context.SchoolStudents.AddAsync(new SchoolStudent
                {
                    Id = Guid.NewGuid(),
                    SchoolId = schoolId,
                    StudentId = person.Id
                });
            }

            if (organisationId == null)
            {
                throw new ArgumentNullException(nameof(organisationId));
            }

            await _context.OrganisationPeople.AddAsync(new OrganisationPerson
            {
                Id = Guid.NewGuid(),
                OrganisationId = (Guid)organisationId,
                PersonId = person.Id
            });

            return new CreatedPersonDTO(
                person.Id,
                person.Title,
                person.FirstName,
                person.MiddleName,
                person.LastName,
                person.DoB,
                ModelMapper.GetAddressDTO(person.Address),
                person.MobileNo,
                person.Email,
                (await CreateMartialBaseUserAsync(person.Id)).InvitationCode);
        }

        public Task<List<PersonDTO>> GetAllAsync() =>
            throw new ArgumentException("Organisation or school Id must be specified.");

        public async Task<List<PersonDTO>> GetAllAsync(Guid? organisationId, Guid? schoolId)
        {
            List<Person> people;

            if (organisationId == null && schoolId == null)
            {
                throw new ArgumentException("Organisation or school Id must be specified.");
            }

            if (organisationId != null && schoolId != null)
            {
                throw new NotImplementedException();
            }

            if (schoolId != null)
            {
                people = await _context.SchoolStudents
                    .Where(ss => ss.SchoolId == schoolId)
                    .Include(ss => ss.Student)
                    .ThenInclude(s => s.Address)
                    .Select(ss => ss.Student)
                    .ToListAsync();
            }
            else
            {
                people = await _context.OrganisationPeople
                    .Where(op => op.OrganisationId == organisationId)
                    .Include(op => op.Person)
                    .ThenInclude(p => p.Address)
                    .Select(op => op.Person)
                    .ToListAsync();
            }

            var personDTOs = new List<PersonDTO>();

            foreach (Person person in people)
            {
                personDTOs.Add(ModelMapper.GetPersonDTO(person));
            }

            return personDTOs;
        }

        public async Task<PersonDTO> GetAsync(Guid id)
        {
            Person person = await _context.People
                .Include(p => p.Address)
                .FirstAsync(p => p.Id == id);

            return ModelMapper.GetPersonDTO(person);
        }

        /// <inheritdoc />
        public async Task<DocumentDTO> CreatePersonDocumentAsync(Guid personId, CreateDocumentInternalDTO createDTO)
        {
            if (!await _context.People.AnyAsync(p => p.Id == personId))
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            var document = (await _context.Documents.AddAsync(ModelMapper.GetDocument(createDTO))).Entity;

            document.DocumentType = await _context.DocumentTypes
                .Include(dt => dt.Organisation)
                .FirstAsync(dt => dt.Id == document.DocumentTypeId);

            await _context.PersonDocuments.AddAsync(new PersonDocument
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                PersonId = personId
            });

            return ModelMapper.GetDocumentDTO(document);
        }

        /// <inheritdoc />
        public async Task<DocumentDTO> GetPersonDocumentAsync(Guid personId, Guid documentId)
        {
            Document document = await _context.PersonDocuments
                .Include(pd => pd.Document)
                .ThenInclude(d => d.DocumentType)
                .ThenInclude(dt => dt.Organisation)
                .Where(pd => pd.PersonId == personId && pd.DocumentId == documentId)
                .Select(pd => pd.Document)
                .FirstAsync();

            return ModelMapper.GetDocumentDTO(document);
        }

        /// <inheritdoc />
        public async Task<List<DocumentDTO>> GetPersonDocumentsAsync(Guid personId, bool includeInactive)
        {
            if (!await _context.People.AnyAsync(p => p.Id == personId))
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            List<Document> documents;

            if (!includeInactive)
            {
                documents = await _context.PersonDocuments
                    .Include(pd => pd.Document)
                    .ThenInclude(d => d.DocumentType)
                    .ThenInclude(dt => dt.Organisation)
                    .Where(pd => pd.PersonId == personId && pd.IsActive)
                    .Select(pd => pd.Document)
                    .ToListAsync();
            }
            else
            {
                documents = await _context.PersonDocuments
                    .Include(pd => pd.Document)
                    .ThenInclude(d => d.DocumentType)
                    .ThenInclude(dt => dt.Organisation)
                    .Where(pd => pd.PersonId == personId)
                    .Select(pd => pd.Document)
                    .ToListAsync();
            }

            var documentDTOs = new List<DocumentDTO>();

            foreach (Document document in documents)
            {
                documentDTOs.Add(ModelMapper.GetDocumentDTO(document));
            }

            return documentDTOs;
        }

        public async Task<Guid?> FindPersonIdByEmailAsync(string personEmail)
        {
            Person person = await _context.People.FirstOrDefaultAsync(p => p.Email == personEmail);

            return person?.Id;
        }

        public async Task<List<PersonDTO>> FindAsync(string email = null, string firstName = null, string middleName = null, string lastName = null, bool returnAddresses = false)
        {
            var searchPredicate = GetSearchPredicate(email, firstName, middleName, lastName);

            if (!returnAddresses)
            {
                return await _context.People
                    .Where(searchPredicate)
                    .Select(person => ModelMapper.GetPersonDTO(person))
                    .ToListAsync();
            }

            return await _context.People
                .Where(searchPredicate)
                .Include(p => p.Address)
                .Select(person => ModelMapper.GetPersonDTO(person))
                .ToListAsync();
        }

        public async Task<string> GetPersonNameFromIdAsync(Guid personId) =>
            await _context.People.Where(p => p.Id == personId).Select(p => p.FullName).FirstAsync();

        public async Task<PersonDTO> UpdateAsync(Guid id, UpdatePersonDTO updateDTO)
        {
            if (updateDTO == null)
            {
                throw new ArgumentNullException(nameof(updateDTO));
            }

            Person person = await _context.People
                .Include(p => p.Address)
                .FirstAsync(p => p.Id == id);

            person.Title = updateDTO.Title;
            person.FirstName = updateDTO.FirstName;
            person.MiddleName = updateDTO.MiddleName;
            person.LastName = updateDTO.LastName;

            if (updateDTO.Address != null)
            {
                if (person.Address != null)
                {
                    person.Address.Line1 = updateDTO.Address.Line1;
                    person.Address.Line2 = updateDTO.Address.Line2;
                    person.Address.Line3 = updateDTO.Address.Line3;
                    person.Address.Town = updateDTO.Address.Town;
                    person.Address.County = updateDTO.Address.County;
                    person.Address.PostCode = updateDTO.Address.PostCode;
                    person.Address.CountryCode = updateDTO.Address.CountryCode;
                    person.Address.LandlinePhone = updateDTO.Address.LandlinePhone;
                }
                else
                {
                    var personAddress = new Address
                    {
                        Line1 = updateDTO.Address.Line1,
                        Line2 = updateDTO.Address.Line2,
                        Line3 = updateDTO.Address.Line3,
                        Town = updateDTO.Address.Town,
                        County = updateDTO.Address.County,
                        PostCode = updateDTO.Address.PostCode,
                        CountryCode = updateDTO.Address.CountryCode,
                        LandlinePhone = updateDTO.Address.LandlinePhone
                    };

                    await _context.Addresses.AddAsync(personAddress);

                    person.Address = personAddress;
                }
            }
            else
            {
                if (person.Address != null)
                {
                    _context.Addresses.Remove(person.Address);

                    person.AddressId = null;
                    person.Address = null;
                }
            }

            person.MobileNo = updateDTO.MobileNo;
            person.Email = updateDTO.Email;

            return ModelMapper.GetPersonDTO(person);
        }

        public async Task DeleteAsync(Guid id)
        {
            Person person = await _context.People.FirstAsync(p => p.Id == id);

            if (person.AddressId != null)
            {
                _context.Addresses.Remove(_context.Addresses.First(a => a.Id == person.AddressId));
            }

            _context.People.Remove(person);
        }

        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() >= 0;

        private static Expression<Func<Person, bool>> GetSearchPredicate(
            string email = null,
            string firstName = null,
            string middleName = null,
            string lastName = null)
        {
            if (string.IsNullOrEmpty(email)
                && string.IsNullOrEmpty(firstName)
                && string.IsNullOrEmpty(middleName)
                && string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentException("No search parameters provided.");
            }

            var predicate = PredicateBuilder.True<Person>();

            if (email != null)
            {
                predicate = predicate.And(p => p.Email == email);
            }

            if (firstName != null)
            {
                predicate = predicate.And(p => p.FirstName == firstName);
            }

            if (middleName != null)
            {
                predicate = predicate.And(p => p.MiddleName == middleName);
            }

            if (lastName != null)
            {
                predicate = predicate.And(p => p.LastName == lastName);
            }

            return predicate;
        }

        /// <summary>
        /// Creates a <see cref="MartialBaseUser"/> object with an invitation code for a newly-registered Azure
        /// user to assign to their own account later on.
        /// </summary>
        /// <remarks>
        /// This method should ONLY be used by controllers which create <see cref="Person">People</see>. It should
        /// NOT be used by any controller that is not responsible for managing <see cref="Person">People</see>
        /// objects.
        /// </remarks>
        /// <param name="personId">The ID of the <see cref="Person"/> to create an associated record for.</param>
        /// <returns>The newly-created <see cref="MartialBaseUser"/>.</returns>
        private async Task<MartialBaseUser> CreateMartialBaseUserAsync(Guid personId)
        {
            string invitationCode;

            do
            {
                invitationCode = RandomData.GetRandomString(
                    MartialBaseUsersRepository.INVITATION_CODE_LENGTH,
                    true,
                    false);
            }
            while (await _context.MartialBaseUsers.AnyAsync(mbu =>
                mbu.InvitationCode == invitationCode));

            var newUser = new MartialBaseUser
            {
                Id = Guid.NewGuid(),
                PersonId = personId,
                InvitationCode = invitationCode
            };

            await _context.MartialBaseUsers.AddAsync(newUser);

            return newUser;
        }
    }
}
