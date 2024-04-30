// <copyright file="SchoolsRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Documents;
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Data.Models.InternalDTOs.Schools;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.DTOs.Schools;
using MartialBase.API.Tools;

using Microsoft.EntityFrameworkCore;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.Data.Repositories
{
    public class SchoolsRepository : ISchoolsRepository
    {
        private readonly MartialBaseDbContext _context;

        public SchoolsRepository(MartialBaseDbContext context) => _context = context;

        public async Task<bool> ExistsAsync(Guid id) => await _context.Schools.AnyAsync(s => s.Id == id);

        public async Task<bool> SchoolAddressExistsAsync(Guid schoolId, Guid addressId) => await _context.SchoolAddresses.AnyAsync(sa => sa.SchoolId == schoolId && sa.AddressId == addressId);

        public async Task<List<SchoolDTO>> GetAllAsync() => await GetAllAsync(null, null);

        public async Task<List<SchoolDTO>> GetAllAsync(Guid? artId, Guid? organisationId)
        {
            List<School> schools;

            if (artId != null && organisationId != null)
            {
                schools = await _context.Schools.Include(s => s.Art)
                    .Include(s => s.Organisation)
                    .Include(s => s.SchoolAddresses)
                    .ThenInclude(sa => sa.Address)
                    .Include(s => s.HeadInstructor)
                    .Include(s => s.DefaultAddress)
                    .Where(s => s.ArtId == artId && s.OrganisationId == organisationId)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            else if (artId != null)
            {
                schools = await _context.Schools.Include(s => s.Art)
                    .Include(s => s.Organisation)
                    .Include(s => s.SchoolAddresses)
                    .ThenInclude(sa => sa.Address)
                    .Include(s => s.HeadInstructor)
                    .Include(s => s.DefaultAddress)
                    .Where(s => s.ArtId == artId)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            else if (organisationId != null)
            {
                schools = await _context.Schools.Include(s => s.Art)
                    .Include(s => s.Organisation)
                    .Include(s => s.SchoolAddresses)
                    .ThenInclude(sa => sa.Address)
                    .Include(s => s.HeadInstructor)
                    .Include(s => s.DefaultAddress)
                    .Where(s => s.OrganisationId == organisationId)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            else
            {
                schools = await _context.Schools
                    .Include(s => s.Art)
                    .Include(s => s.Organisation)
                    .Include(s => s.SchoolAddresses)
                    .ThenInclude(sa => sa.Address)
                    .Include(s => s.HeadInstructor)
                    .Include(s => s.DefaultAddress)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }

            var schoolDTOs = new List<SchoolDTO>();

            foreach (School school in schools)
            {
                schoolDTOs.Add(ModelMapper.GetSchoolDTO(school));
            }

            return schoolDTOs;
        }

        public async Task<List<SchoolStudentDTO>> GetStudentsAsync(Guid schoolId)
        {
            var schoolStudents = new List<SchoolStudentDTO>();

            string schoolName = await _context.Schools
                .Where(s => s.Id == schoolId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();

            foreach (SchoolStudent schoolStudent in await _context.SchoolStudents
                         .Include(ss => ss.Student)
                         .ThenInclude(s => s.Address)
                         .Include(ss => ss.InsuranceDocument)
                         .ThenInclude(d => d.DocumentType)
                         .ThenInclude(dt => dt.Organisation)
                         .Include(ss => ss.LicenceDocument)
                         .ThenInclude(d => d.DocumentType)
                         .ThenInclude(dt => dt.Organisation)
                         .Where(ss => ss.SchoolId == schoolId)
                         .OrderBy(ss => ss.Student.FirstName)
                         .ThenBy(ss => ss.Student.LastName)
                         .ToListAsync())
            {
                schoolStudents.Add(
                    new SchoolStudentDTO(
                        new PersonDTO(
                            schoolStudent.Student.Id,
                            schoolStudent.Student.Title,
                            schoolStudent.Student.FirstName,
                            null,
                            schoolStudent.Student.LastName,
                            null,
                            schoolStudent.Student.Address == null
                                ? null
                                : ModelMapper.GetAddressDTO(schoolStudent.Student.Address),
                            null,
                            null),
                        schoolId,
                        schoolName,
                        schoolStudent.InsuranceDocument == null
                            ? null
                            : ModelMapper.GetDocumentDTO(schoolStudent.InsuranceDocument),
                        schoolStudent.LicenceDocument == null
                            ? null
                            : ModelMapper.GetDocumentDTO(schoolStudent.LicenceDocument),
                        schoolStudent.IsInstructor,
                        schoolStudent.IsSecretary));
            }

            return schoolStudents;
        }

        public async Task<AddressDTO> AddNewAddressToSchoolAsync(Guid schoolId, CreateAddressDTO createAddressDTO)
        {
            School school = await _context.Schools
                .Include(s => s.SchoolAddresses)
                .ThenInclude(sa => sa.Address)
                .FirstAsync(s => s.Id == schoolId);

            var schoolAddresses = school.SchoolAddresses.Select(a => a.Address).ToList();

            bool addressAlreadyExists = schoolAddresses.Any(GetMatchPredicate(createAddressDTO));

            if (addressAlreadyExists)
            {
                throw new EntityAlreadyExistsException($"The provided address already exists under school '{schoolId}'");
            }

            var schoolAddress = ModelMapper.GetAddress(createAddressDTO);

            await _context.Addresses.AddAsync(schoolAddress);

            await _context.SchoolAddresses.AddAsync(new SchoolAddress
            {
                Id = Guid.NewGuid(),
                SchoolId = school.Id,
                AddressId = schoolAddress.Id
            });

            return ModelMapper.GetAddressDTO(schoolAddress);
        }

        public async Task RemoveAddressFromSchoolAsync(Guid schoolId, Guid addressId)
        {
            _context.SchoolAddresses.Remove(
                await _context.SchoolAddresses.FirstAsync(sa => sa.SchoolId == schoolId && sa.AddressId == addressId));

            _context.Addresses.Remove(await _context.Addresses.FirstAsync(a => a.Id == addressId));
        }

        public async Task ChangeSchoolOrganisationAsync(Guid schoolId, Guid organisationId)
        {
            School school = await _context.Schools.FirstAsync(s => s.Id == schoolId);

            school.OrganisationId = organisationId;
        }

        public async Task ChangeSchoolArtAsync(Guid schoolId, Guid artId)
        {
            School school = await _context.Schools.FirstAsync(s => s.Id == schoolId);

            school.ArtId = artId;
        }

        public async Task ChangeSchoolHeadInstructorAsync(Guid schoolId, Guid studentId, bool retainSecretary)
        {
            Guid? previousHeadInstructorId =
                await _context.Schools.Where(s => s.Id == schoolId).Select(s => s.HeadInstructorId).FirstAsync();

            (await _context.Schools.FirstAsync(s => s.Id == schoolId)).HeadInstructorId = studentId;

            if (!await _context.SchoolStudents.AnyAsync(ss => ss.SchoolId == schoolId && ss.StudentId == studentId))
            {
                await _context.SchoolStudents.AddAsync(new SchoolStudent
                {
                    Id = Guid.NewGuid(),
                    SchoolId = schoolId,
                    StudentId = studentId,
                    IsInstructor = true,
                    IsSecretary = true
                });
            }
            else
            {
                SchoolStudent schoolStudent =
                    await _context.SchoolStudents.FirstAsync(ss => ss.SchoolId == schoolId && ss.StudentId == studentId);

                schoolStudent.IsInstructor = true;
                schoolStudent.IsSecretary = true;
            }

            if (!retainSecretary)
            {
                (await _context.SchoolStudents
                        .FirstAsync(ss => ss.SchoolId == schoolId && ss.StudentId == previousHeadInstructorId))
                        .IsSecretary = false;
            }
        }

        public async Task<PersonDTO> AddNewPersonToSchoolAsync(Guid schoolId, CreatePersonInternalDTO createPersonDTO, bool isInstructor = false, bool isSecretary = false)
        {
            var student = ModelMapper.GetPerson(createPersonDTO);

            await _context.Addresses.AddAsync(student.Address);
            await _context.People.AddAsync(student);

            Guid organisationId = _context.Schools
                .Where(s => s.Id == schoolId)
                .Select(s => s.OrganisationId).First();

            await _context.SchoolStudents.AddAsync(new SchoolStudent
            {
                Id = Guid.NewGuid(),
                SchoolId = schoolId,
                StudentId = student.Id,
                IsInstructor = isInstructor,
                IsSecretary = isSecretary
            });

            await _context.OrganisationPeople.AddAsync(new OrganisationPerson
            {
                Id = Guid.NewGuid(),
                OrganisationId = organisationId,
                PersonId = student.Id
            });

            return ModelMapper.GetPersonDTO(student);
        }

        public async Task AddExistingPersonToSchoolAsync(Guid schoolId, Guid studentId, bool isInstructor = false, bool isSecretary = false)
        {
            if (!await _context.SchoolStudents.AnyAsync(ss =>
                ss.SchoolId == schoolId && ss.StudentId == studentId))
            {
                await _context.SchoolStudents.AddAsync(new SchoolStudent
                {
                    Id = Guid.NewGuid(),
                    SchoolId = schoolId,
                    StudentId = studentId,
                    IsInstructor = isInstructor,
                    IsSecretary = isSecretary
                });
            }

            Guid organisationId = await _context.Schools
                .Where(s => s.Id == schoolId)
                .Select(s => s.OrganisationId).FirstAsync();

            if (!await _context.OrganisationPeople.AnyAsync(op =>
                op.OrganisationId == organisationId && op.PersonId == studentId))
            {
                await _context.OrganisationPeople.AddAsync(new OrganisationPerson
                {
                    Id = Guid.NewGuid(),
                    OrganisationId = organisationId,
                    PersonId = studentId,
                    IsOrganisationAdmin = false
                });
            }
        }

        public async Task<DocumentDTO> GetStudentInsuranceAsync(Guid schoolId, Guid studentId)
        {
            Document insuranceDocument = (await _context.SchoolStudents
                .Include(ss => ss.InsuranceDocument)
                .ThenInclude(d => d.DocumentType)
                .ThenInclude(dt => dt.Organisation)
                .FirstOrDefaultAsync(ss =>
                    ss.SchoolId == schoolId &&
                    ss.StudentId == studentId &&
                    ss.InsuranceDocumentId != null))?.InsuranceDocument;

            return insuranceDocument == null
                ? null
                : ModelMapper.GetDocumentDTO(insuranceDocument);
        }

        public async Task<DocumentDTO> AddStudentInsuranceAsync(Guid schoolId, Guid studentId, CreateDocumentInternalDTO createDocumentDTO, bool archiveExisting = true)
        {
            Guid? existingInsuranceId = await _context.SchoolStudents
                .Where(ss => ss.SchoolId == schoolId && ss.StudentId == studentId)
                .Select(ss => ss.InsuranceDocumentId)
                .FirstAsync();

            var newInsurance = (await _context.Documents.AddAsync(ModelMapper.GetDocument(createDocumentDTO))).Entity;

            newInsurance.DocumentType = await _context.DocumentTypes
                .Include(dt => dt.Organisation)
                .FirstAsync(dt => dt.Id == newInsurance.DocumentTypeId);

            await _context.PersonDocuments.AddAsync(new PersonDocument
            {
                Id = Guid.NewGuid(),
                PersonId = studentId,
                DocumentId = newInsurance.Id,
                IsActive = true
            });

            (await _context.SchoolStudents.FirstAsync(ss =>
                    ss.SchoolId == schoolId &&
                    ss.StudentId == studentId))
                .InsuranceDocumentId = newInsurance.Id;

            if (archiveExisting)
            {
                if (existingInsuranceId != null)
                {
                    (await _context.PersonDocuments
                        .FirstAsync(sd => sd.PersonId == studentId &&
                                     sd.DocumentId == existingInsuranceId))
                        .IsActive = false;
                }
            }
            else
            {
                if (existingInsuranceId != null)
                {
                    _context.PersonDocuments.Remove(
                        await _context.PersonDocuments.FirstAsync(pd =>
                            pd.PersonId == studentId &&
                            pd.DocumentId == existingInsuranceId));

                    _context.Documents.Remove(
                        await _context.Documents.FirstAsync(d => d.Id == existingInsuranceId));
                }
            }

            return ModelMapper.GetDocumentDTO(newInsurance);
        }

        public async Task<DocumentDTO> GetStudentLicenceAsync(Guid schoolId, Guid studentId)
        {
            Document licenceDocument = (await _context.SchoolStudents
                .Include(ss => ss.LicenceDocument)
                .ThenInclude(d => d.DocumentType)
                .ThenInclude(dt => dt.Organisation)
                .FirstOrDefaultAsync(ss =>
                    ss.SchoolId == schoolId &&
                    ss.StudentId == studentId &&
                    ss.LicenceDocumentId != null))?.LicenceDocument;

            return licenceDocument == null
                ? null
                : ModelMapper.GetDocumentDTO(licenceDocument);
        }

        public async Task<DocumentDTO> AddStudentLicenceAsync(Guid schoolId, Guid studentId, CreateDocumentInternalDTO createDocumentDTO, bool archiveExisting = true)
        {
            Guid? existingLicenceId = await _context.SchoolStudents
                .Where(ss => ss.SchoolId == schoolId && ss.StudentId == studentId)
                .Select(ss => ss.LicenceDocumentId)
                .FirstAsync();

            var newLicence = (await _context.Documents.AddAsync(ModelMapper.GetDocument(createDocumentDTO))).Entity;

            newLicence.DocumentType = await _context.DocumentTypes
                .Include(dt => dt.Organisation)
                .FirstAsync(dt => dt.Id == newLicence.DocumentTypeId);

            await _context.PersonDocuments.AddAsync(new PersonDocument
            {
                Id = Guid.NewGuid(),
                PersonId = studentId,
                DocumentId = newLicence.Id,
                IsActive = true
            });

            (await _context.SchoolStudents.FirstAsync(ss =>
                    ss.SchoolId == schoolId &&
                    ss.StudentId == studentId))
                .LicenceDocumentId = newLicence.Id;

            if (archiveExisting)
            {
                if (existingLicenceId != null)
                {
                    (await _context.PersonDocuments
                            .FirstAsync(sd => sd.PersonId == studentId &&
                                         sd.DocumentId == existingLicenceId))
                        .IsActive = false;
                }
            }
            else
            {
                if (existingLicenceId != null)
                {
                    _context.PersonDocuments.Remove(
                        await _context.PersonDocuments.FirstAsync(pd =>
                            pd.PersonId == studentId &&
                            pd.DocumentId == existingLicenceId));

                    _context.Documents.Remove(
                        await _context.Documents.FirstAsync(d => d.Id == existingLicenceId));
                }
            }

            return ModelMapper.GetDocumentDTO(newLicence);
        }

        public async Task RemoveStudentFromSchoolAsync(Guid schoolId, Guid studentId)
        {
            SchoolStudent schoolStudent =
                await _context.SchoolStudents.FirstOrDefaultAsync(ss => ss.SchoolId == schoolId && ss.StudentId == studentId);

            if (schoolStudent != null)
            {
                _context.SchoolStudents.Remove(schoolStudent);
            }
        }

        public async Task<bool> SchoolHasStudentAsync(Guid schoolId, Guid studentId) =>
            await _context.SchoolStudents.AnyAsync(ss => ss.SchoolId == schoolId && ss.StudentId == studentId);

        public async Task<bool> SchoolHasInstructorAsync(Guid schoolId, Guid studentId) =>
            await _context.SchoolStudents.AnyAsync(
                ss => ss.SchoolId == schoolId && ss.StudentId == studentId && ss.IsInstructor);

        public async Task<bool> SchoolHasSecretaryAsync(Guid schoolId, Guid studentId) =>
            await _context.SchoolStudents.AnyAsync(
                ss => ss.SchoolId == schoolId && ss.StudentId == studentId && ss.IsSecretary);

        public async Task<Guid?> GetHeadInstructorIdAsync(Guid schoolId) =>
            await _context.Schools.Where(s => s.Id == schoolId).Select(s => s.HeadInstructorId).FirstAsync();

        public async Task<List<StudentSchoolDTO>> GetSchoolsForPersonAsync(Guid personId)
        {
            var studentSchools =
                await _context.SchoolStudents
                    .Include(ss => ss.School)
                    .Include(ss => ss.School.Art)
                    .Include(ss => ss.School.Organisation)
                    .Include(ss => ss.School.HeadInstructor)
                    .Include(ss => ss.InsuranceDocument)
                    .ThenInclude(d => d.DocumentType)
                    .ThenInclude(dt => dt.Organisation)
                    .Include(ss => ss.LicenceDocument)
                    .ThenInclude(d => d.DocumentType)
                    .ThenInclude(dt => dt.Organisation)
                    .Where(ss => ss.StudentId == personId)
                    .ToListAsync();

            var studentSchoolDTOs = new List<StudentSchoolDTO>();

            foreach (SchoolStudent studentSchool in studentSchools)
            {
                studentSchoolDTOs.Add(new StudentSchoolDTO(
                    new SchoolDTO(
                        studentSchool.School.Id,
                        studentSchool.School.Art.Id.ToString(),
                        studentSchool.School.Art.Name,
                        studentSchool.School.Organisation.Id.ToString(),
                        studentSchool.School.Organisation.Initials,
                        studentSchool.School.Name,
                        studentSchool.School.HeadInstructor?.Id.ToString(),
                        studentSchool.School.HeadInstructor?.FullName,
                        null,
                        null,
                        null,
                        null,
                        null),
                    studentSchool.InsuranceDocument == null
                        ? null
                        : ModelMapper.GetDocumentDTO(studentSchool.InsuranceDocument),
                    studentSchool.LicenceDocument == null
                        ? null
                        : ModelMapper.GetDocumentDTO(studentSchool.LicenceDocument),
                    studentSchool.IsInstructor,
                    studentSchool.IsSecretary));
            }

            return studentSchoolDTOs;
        }

        public async Task<SchoolDTO> CreateAsync(CreateSchoolInternalDTO createDTO)
        {
            if (createDTO == null)
            {
                throw new ArgumentNullException(nameof(createDTO));
            }

            var school = ModelMapper.GetSchool(createDTO);

            school.Art = await _context.Arts.FirstAsync(a => a.Id == school.ArtId);
            school.Organisation = await _context.Organisations.FirstAsync(o => o.Id == school.OrganisationId);

            school.SchoolAddresses = new List<SchoolAddress>
            {
                new SchoolAddress
                {
                    Id = Guid.NewGuid(),
                    SchoolId = school.Id,
                    AddressId = school.DefaultAddress.Id
                }
            };

            if (createDTO.AdditionalTrainingVenues != null)
            {
                foreach (CreateAddressDTO schoolAddressDTO in createDTO.AdditionalTrainingVenues)
                {
                    var schoolAddress = ModelMapper.GetAddress(schoolAddressDTO);

                    await _context.Addresses.AddAsync(schoolAddress);

                    school.SchoolAddresses.Add(new SchoolAddress
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = school.Id,
                        AddressId = schoolAddress.Id
                    });
                }
            }

            await _context.Addresses.AddAsync(school.DefaultAddress);
            await _context.Schools.AddAsync(school);

            if (school.HeadInstructorId != null)
            {
                await _context.SchoolStudents.AddAsync(new SchoolStudent
                {
                    Id = Guid.NewGuid(),
                    SchoolId = school.Id,
                    StudentId = (Guid)school.HeadInstructorId,
                    IsInstructor = true,
                    IsSecretary = true
                });

                school.HeadInstructor = await _context.People.FirstAsync(p => p.Id == school.HeadInstructorId);
            }

            return ModelMapper.GetSchoolDTO(school);
        }

        public async Task<SchoolDTO> GetAsync(Guid id)
        {
            School school = await _context.Schools
                .Include(s => s.Art)
                .Include(s => s.Organisation)
                .Include(s => s.SchoolAddresses)
                .ThenInclude(sa => sa.Address)
                .Include(s => s.HeadInstructor)
                .Include(s => s.DefaultAddress)
                .FirstAsync(s => s.Id == id);

            return ModelMapper.GetSchoolDTO(school);
        }

        public async Task<SchoolDTO> UpdateAsync(Guid id, UpdateSchoolDTO updateDTO)
        {
            if (updateDTO == null)
            {
                throw new ArgumentNullException(nameof(updateDTO));
            }

            School school = await _context.Schools
                .Include(s => s.Art)
                .Include(s => s.Organisation)
                .Include(s => s.SchoolAddresses)
                .ThenInclude(sa => sa.Address)
                .Include(s => s.HeadInstructor)
                .Include(s => s.DefaultAddress)
                .FirstAsync(s => s.Id == id);

            school.Name = updateDTO.Name;
            school.DefaultAddress.Line1 = updateDTO.Address.Line1;
            school.DefaultAddress.Line2 = updateDTO.Address.Line2;
            school.DefaultAddress.Line3 = updateDTO.Address.Line3;
            school.DefaultAddress.Town = updateDTO.Address.Town;
            school.DefaultAddress.County = updateDTO.Address.County;
            school.DefaultAddress.PostCode = updateDTO.Address.PostCode;
            school.DefaultAddress.CountryCode = updateDTO.Address.CountryCode;
            school.DefaultAddress.LandlinePhone = updateDTO.Address.LandlinePhone;
            school.PhoneNo = updateDTO.PhoneNo;
            school.Email = updateDTO.EmailAddress;
            school.Website = updateDTO.WebsiteURL;

            var removedAddressIds = new List<Guid>();

            foreach (Address address in school.SchoolAddresses.Select(a => a.Address))
            {
                // The school default address details have already been updated above so don't need to be updated again
                if (address.Id != school.DefaultAddress.Id)
                {
                    string addressId = address.Id.ToString();

                    if (updateDTO.AdditionalTrainingVenues.ContainsKey(addressId))
                    {
                        address.Line1 = updateDTO.AdditionalTrainingVenues[addressId].Line1;
                        address.Line2 = updateDTO.AdditionalTrainingVenues[addressId].Line2;
                        address.Line3 = updateDTO.AdditionalTrainingVenues[addressId].Line3;
                        address.Town = updateDTO.AdditionalTrainingVenues[addressId].Town;
                        address.County = updateDTO.AdditionalTrainingVenues[addressId].County;
                        address.PostCode = updateDTO.AdditionalTrainingVenues[addressId].PostCode;
                        address.CountryCode = updateDTO.AdditionalTrainingVenues[addressId].CountryCode;
                        address.LandlinePhone = updateDTO.AdditionalTrainingVenues[addressId].LandlinePhone;
                    }
                    else
                    {
                        // The current address does not appear in the updated DTO, therefore doesn't belong to the school any more, therefore needs to be removed
                        _context.SchoolAddresses.Remove(await _context.SchoolAddresses.FirstAsync(sa =>
                            sa.SchoolId == school.Id && sa.AddressId == address.Id));
                        _context.Addresses.Remove(address);
                        removedAddressIds.Add(address.Id);
                    }
                }
            }

            foreach (Guid addressId in removedAddressIds)
            {
                school.SchoolAddresses.Remove(school.SchoolAddresses.First(sa => sa.AddressId == addressId));
            }

            return ModelMapper.GetSchoolDTO(school);
        }

        public async Task DeleteAsync(Guid id)
        {
            foreach (SchoolAddress schoolAddress in await _context.SchoolAddresses
                         .Include(sa => sa.Address)
                         .Where(sa => sa.SchoolId == id)
                         .ToListAsync())
            {
                _context.SchoolAddresses.Remove(schoolAddress);
                _context.Addresses.Remove(schoolAddress.Address);
            }

            foreach (SchoolStudent schoolStudent in await _context.SchoolStudents
                         .Where(ss => ss.SchoolId == id)
                         .ToListAsync())
            {
                _context.SchoolStudents.Remove(schoolStudent);
            }

            _context.Schools.Remove(await _context.Schools.FirstAsync(s => s.Id == id));
        }

        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() >= 0;

        private static Func<Address, bool> GetMatchPredicate(CreateAddressDTO createAddressDTO)
        {
            var predicate = PredicateBuilder.True<Address>();

            if (createAddressDTO.Line1 != null)
            {
                predicate = predicate.And(a => a.Line1 == createAddressDTO.Line1);
            }

            if (createAddressDTO.Line2 != null)
            {
                predicate = predicate.And(a => a.Line2 == createAddressDTO.Line2);
            }

            if (createAddressDTO.Line3 != null)
            {
                predicate = predicate.And(a => a.Line3 == createAddressDTO.Line3);
            }

            if (createAddressDTO.Town != null)
            {
                predicate = predicate.And(a => a.Town == createAddressDTO.Town);
            }

            if (createAddressDTO.County != null)
            {
                predicate = predicate.And(a => a.County == createAddressDTO.County);
            }

            if (createAddressDTO.PostCode != null)
            {
                predicate = predicate.And(a => a.PostCode == createAddressDTO.PostCode);
            }

            if (createAddressDTO.CountryCode != null)
            {
                predicate = predicate.And(a => a.CountryCode == createAddressDTO.CountryCode);
            }

            return predicate.Compile();
        }
    }
}
