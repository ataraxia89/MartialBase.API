// <copyright file="SchoolResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data;
using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Schools;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.DTOs.Schools;
using MartialBase.API.Tools;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class SchoolResources
    {
        internal static bool CheckExists(Guid schoolId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.Schools.Any(s => s.Id == schoolId);
            }
        }

        internal static void AssertExist(List<StudentSchoolDTO> studentSchools, string dbIdentifier)
        {
            foreach (StudentSchoolDTO studentSchool in studentSchools)
            {
                AssertExistsRestrictedDTO(studentSchool.School, dbIdentifier);
            }
        }

        internal static void AssertExists(SchoolDTO school, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                School checkSchool = dbContext.Schools
                    .Include(s => s.Art)
                    .Include(s => s.Organisation)
                    .ThenInclude(o => o.Address)
                    .Include(s => s.HeadInstructor)
                    .ThenInclude(hi => hi.Address)
                    .Include(s => s.DefaultAddress)
                    .Include(s => s.SchoolAddresses)
                    .ThenInclude(sa => sa.Address)
                    .FirstOrDefault(s => s.Id == school.Id);
                AssertEqual(checkSchool, school);
            }
        }

        internal static void AssertExistsRestrictedDTO(SchoolDTO school, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                School checkSchool = dbContext.Schools
                    .Include(s => s.Art)
                    .Include(s => s.Organisation)
                    .ThenInclude(o => o.Address)
                    .Include(s => s.HeadInstructor)
                    .ThenInclude(hi => hi.Address)
                    .Include(s => s.DefaultAddress)
                    .Include(s => s.SchoolAddresses)
                    .ThenInclude(sa => sa.Address)
                    .FirstOrDefault(s => s.Id == school.Id);
                AssertEqualRestrictedDTO(checkSchool, school);
            }
        }

        internal static School CreateSchool(School school, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.False(dbContext.Schools.Any(s => s.Id == school.Id));

                dbContext.Schools.Add(school);

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.Schools.Any(s => s.Id == school.Id));
            }

            return school;
        }

        internal static School CreateTestSchool(string dbIdentifier, bool realisticData = false) => CreateTestSchools(1, dbIdentifier, realisticData).First();

        internal static List<School> CreateTestSchools(int numberToCreate, string dbIdentifier, bool realisticData = false)
        {
            if (numberToCreate <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberToCreate),
                    numberToCreate,
                    $"Cannot create {numberToCreate} test schools.");
            }

            var createdSchools = new List<School>();

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                for (int i = 0; i < numberToCreate; i++)
                {
                    School school = DataGenerator.Schools.GenerateSchoolObject(realisticData);

                    Assert.False(dbContext.Schools.Any(s => s.Id == school.Id));

                    dbContext.Schools.Add(school);
                    createdSchools.Add(school);
                }

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                foreach (School createdSchool in createdSchools)
                {
                    School checkSchool = dbContext.Schools
                        .Include(s => s.Art)
                        .Include(s => s.Organisation)
                        .ThenInclude(o => o.Address)
                        .Include(s => s.Organisation)
                        .ThenInclude(op => op.Address)
                        .Include(s => s.HeadInstructor)
                        .ThenInclude(hi => hi.Address)
                        .Include(s => s.DefaultAddress)
                        .Include(s => s.SchoolAddresses)
                        .FirstOrDefault(s => s.Id == createdSchool.Id);
                    AssertEqual(createdSchool, checkSchool);
                }
            }

            return createdSchools;
        }

        internal static bool CheckSchoolBelongsToArt(Guid schoolId, Guid artId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.Schools.Any(s => s.Id == schoolId && s.ArtId == artId);
            }
        }

        internal static void EnsureSchoolsBelongToArt(List<School> schools, Art art, string dbIdentifier, bool updateInMemoryObjects = true)
        {
            foreach (School school in schools)
            {
                EnsureSchoolBelongsToArt(school, art, dbIdentifier, updateInMemoryObjects);
            }
        }

        internal static void EnsureSchoolBelongsToArt(School school, Art art, string dbIdentifier, bool updateInMemoryObject = true)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                School dbSchool = dbContext.Schools.First(s => s.Id == school.Id);

                if (dbSchool.ArtId != art.Id)
                {
                    dbSchool.ArtId = art.Id;

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.Schools.Any(s =>
                    s.Id == school.Id &&
                    s.ArtId == art.Id));
            }

            if (updateInMemoryObject)
            {
                school.ArtId = art.Id;
                school.Art = art;
            }
        }

        internal static bool CheckSchoolBelongsToOrganisation(Guid schoolId, Guid organisationId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.Schools.Any(s => s.Id == schoolId && s.OrganisationId == organisationId);
            }
        }

        internal static void EnsureSchoolsBelongToOrganisation(List<School> schools, Organisation organisation, string dbIdentifier, bool updateInMemoryObjects = true)
        {
            foreach (School school in schools)
            {
                EnsureSchoolBelongsToOrganisation(school, organisation, dbIdentifier, updateInMemoryObjects);
            }
        }

        internal static void EnsureSchoolBelongsToOrganisation(School school, Organisation organisation, string dbIdentifier, bool updateInMemoryObject = true)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                School dbSchool = dbContext.Schools.First(s => s.Id == school.Id);

                if (dbSchool.OrganisationId != organisation.Id)
                {
                    dbSchool.OrganisationId = organisation.Id;

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.Schools.Any(s =>
                    s.Id == school.Id &&
                    s.OrganisationId == organisation.Id));
            }

            if (updateInMemoryObject)
            {
                school.OrganisationId = organisation.Id;
                school.Organisation = organisation;
            }
        }

        internal static bool CheckPersonIsHeadInstructor(Guid schoolId, Guid personId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.Schools.Any(s => s.Id == schoolId && s.HeadInstructorId == personId);
            }
        }

        internal static void EnsurePersonIsHeadInstructor(School school, Person person, string dbIdentifier, bool updateInMemoryObject = true)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                School dbSchool = dbContext.Schools.First(s => s.Id == school.Id);

                if (dbSchool.HeadInstructorId != person.Id)
                {
                    dbSchool.HeadInstructorId = person.Id;

                    Assert.True(dbContext.SaveChanges() > 0);
                }

                if (!dbContext.SchoolStudents.Any(ss => ss.SchoolId == school.Id && ss.StudentId == person.Id))
                {
                    dbContext.SchoolStudents.Add(new SchoolStudent
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = school.Id,
                        StudentId = person.Id,
                        IsInstructor = true,
                        IsSecretary = true
                    });

                    Assert.True(dbContext.SaveChanges() > 0);
                }
                else
                {
                    SchoolStudent schoolStudent =
                        dbContext.SchoolStudents.First(ss => ss.SchoolId == school.Id && ss.StudentId == person.Id);

                    if (!schoolStudent.IsInstructor || !schoolStudent.IsSecretary)
                    {
                        schoolStudent.IsInstructor = true;
                        schoolStudent.IsSecretary = true;

                        Assert.True(dbContext.SaveChanges() > 0);
                    }
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.Schools.Any(s =>
                    s.Id == school.Id && s.HeadInstructorId == person.Id));

                Assert.True(dbContext.SchoolStudents.Any(ss =>
                    ss.SchoolId == school.Id && ss.StudentId == person.Id && ss.IsInstructor && ss.IsSecretary));
            }

            if (updateInMemoryObject)
            {
                school.HeadInstructorId = person.Id;
                school.HeadInstructor = person;
            }
        }

        internal static void EnsurePersonIsSecretary(School school, Person person, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                SchoolStudent dbSchoolStudent = dbContext.SchoolStudents.FirstOrDefault(
                    ss => ss.SchoolId == school.Id && ss.StudentId == person.Id);

                if (dbSchoolStudent != null)
                {
                    if (!dbSchoolStudent.IsSecretary)
                    {
                        dbSchoolStudent.IsSecretary = true;

                        Assert.True(dbContext.SaveChanges() > 0);
                    }
                }
                else
                {
                    dbContext.SchoolStudents.Add(new SchoolStudent
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = school.Id,
                        StudentId = person.Id,
                        IsSecretary = true
                    });

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.SchoolStudents.Any(
                    ss => ss.SchoolId == school.Id && ss.StudentId == person.Id && ss.IsSecretary));
            }
        }

        internal static bool CheckSchoolHasAddress(Guid schoolId, Guid addressId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.SchoolAddresses.Any(sa =>
                    sa.SchoolId == schoolId &&
                    sa.AddressId == addressId);
            }
        }

        internal static void EnsureSchoolHasAddresses(Guid schoolId, List<Guid> addressIds, string dbIdentifier)
        {
            foreach (Guid addressId in addressIds)
            {
                EnsureSchoolHasAddress(schoolId, addressId, dbIdentifier);
            }
        }

        internal static void EnsureSchoolHasAddress(Guid schoolId, Guid addressId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                if (!dbContext.SchoolAddresses.Any(sa =>
                    sa.SchoolId == schoolId &&
                    sa.AddressId == addressId))
                {
                    dbContext.SchoolAddresses.Add(new SchoolAddress
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = schoolId,
                        AddressId = addressId
                    });

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.SchoolAddresses.Any(sa =>
                    sa.SchoolId == schoolId &&
                    sa.AddressId == addressId));
            }
        }

        internal static void AssertEqual(List<School> expected, List<SchoolDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (School expectedSchool in expected)
            {
                SchoolDTO actualSchool = actual
                    .FirstOrDefault(s => s.Id == expectedSchool.Id);

                AssertEqual(expectedSchool, actualSchool);
            }
        }

        internal static void AssertEqual(List<School> expected, List<StudentSchoolDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (School expectedSchool in expected)
            {
                SchoolDTO actualSchool =
                    actual.FirstOrDefault(ss => ss.School.Id == expectedSchool.Id)?
                        .School;

                AssertEqualRestrictedDTO(expectedSchool, actualSchool);
            }
        }

        internal static void AssertEqual(School expected, School actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.ArtId, actual.ArtId);
            ArtResources.AssertEqual(expected.Art, actual.Art);
            Assert.Equal(expected.OrganisationId, actual.OrganisationId);
            OrganisationResources.AssertEqual(expected.Organisation, actual.Organisation);
            Assert.Equal(expected.HeadInstructorId, actual.HeadInstructorId);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.DefaultAddressId, actual.DefaultAddressId);
            AddressResources.AssertEqual(expected.DefaultAddress, actual.DefaultAddress);
            Assert.Equal(expected.PhoneNo, actual.PhoneNo);
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Website, actual.Website);

            if (expected.HeadInstructor != null)
            {
                PersonResources.AssertEqual(expected.HeadInstructor, actual.HeadInstructor);
            }
            else
            {
                Assert.Null(actual.HeadInstructor);
            }

            var checkedAddressIds = new List<Guid> { actual.DefaultAddress.Id };

            foreach (SchoolAddress expectedSchoolAddress in expected.SchoolAddresses)
            {
                Assert.Contains(actual.SchoolAddresses, sa =>
                        sa.AddressId == expectedSchoolAddress.AddressId);

                checkedAddressIds.Add(expectedSchoolAddress.AddressId);
            }

            Assert.Empty(actual.SchoolAddresses.Where(sa =>
                !checkedAddressIds.Contains(sa.Address.Id)));
        }

        internal static void AssertEqual(School expected, SchoolDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Art.Id.ToString(), actual.ArtId);
            Assert.Equal(expected.Art.Name, actual.ArtName);
            Assert.Equal(expected.Organisation.Id.ToString(), actual.OrganisationId);
            Assert.Equal(expected.Organisation.Initials, actual.OrganisationName);
            Assert.Equal(expected.HeadInstructorId?.ToString(), actual.HeadInstructorId);
            Assert.Equal(expected.Name, actual.Name);
            AddressResources.AssertEqual(expected.DefaultAddress, actual.Address);
            Assert.Equal(expected.PhoneNo, actual.PhoneNo);
            Assert.Equal(expected.Email, actual.EmailAddress);
            Assert.Equal(expected.Website, actual.WebsiteURL);

            if (expected.HeadInstructor != null)
            {
                Assert.Equal(expected.HeadInstructor.FullName, actual.HeadInstructorName);
            }
            else
            {
                Assert.Null(actual.HeadInstructorName);
            }

            var checkedAddressIds = new List<string> { actual.Address.Id };

            foreach (SchoolAddress expectedSchoolAddress in expected.SchoolAddresses)
            {
                Assert.Contains(actual.TrainingVenues, tv =>
                    tv.Id == expectedSchoolAddress.AddressId.ToString());

                checkedAddressIds.Add(expectedSchoolAddress.AddressId.ToString());
            }

            Assert.Empty(actual.TrainingVenues.Where(tv =>
                !checkedAddressIds.Contains(tv.Id)));
        }

        internal static void AssertEqual(CreateSchoolInternalDTO expected, SchoolDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.ArtId.ToString(), actual.ArtId);
            Assert.Equal(expected.OrganisationId.ToString(), actual.OrganisationId);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.HeadInstructorId?.ToString(), actual.HeadInstructorId);
            AddressResources.AssertEqual(expected.Address, actual.Address);
            Assert.Equal(expected.PhoneNo, actual.PhoneNo);
            Assert.Equal(expected.EmailAddress, actual.EmailAddress);
            Assert.Equal(expected.WebsiteURL, actual.WebsiteURL);

            var checkedAddressIds = new List<string> { actual.Address.Id };

            if (expected.AdditionalTrainingVenues != null)
            {
                foreach (CreateAddressDTO expectedAdditionalTrainingVenue in expected.AdditionalTrainingVenues)
                {
                    string foundAddressId = actual.TrainingVenues.FirstOrDefault(tv =>
                        tv.Line1 == expectedAdditionalTrainingVenue.Line1 &&
                        tv.Line2 == expectedAdditionalTrainingVenue.Line2 &&
                        tv.Line3 == expectedAdditionalTrainingVenue.Line3 &&
                        tv.Town == expectedAdditionalTrainingVenue.Town &&
                        tv.County == expectedAdditionalTrainingVenue.County &&
                        tv.PostCode == expectedAdditionalTrainingVenue.PostCode &&
                        tv.CountryCode == expectedAdditionalTrainingVenue.CountryCode &&
                        tv.LandlinePhone == expectedAdditionalTrainingVenue.LandlinePhone)?.Id;

                    Assert.NotNull(foundAddressId);

                    checkedAddressIds.Add(foundAddressId);
                }
            }

            Assert.Empty(actual.TrainingVenues.Where(tv => !checkedAddressIds.Contains(tv.Id)));
        }

        internal static void AssertEqual(UpdateSchoolDTO expected, SchoolDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Name, actual.Name);
            AddressResources.AssertEqual(expected.Address, actual.Address);
            Assert.Equal(expected.PhoneNo, actual.PhoneNo);
            Assert.Equal(expected.EmailAddress, actual.EmailAddress);
            Assert.Equal(expected.WebsiteURL, actual.WebsiteURL);

            foreach (KeyValuePair<string, UpdateAddressDTO> expectedAddress in expected.AdditionalTrainingVenues)
            {
                AddressDTO actualAddress = actual.TrainingVenues.FirstOrDefault(tv =>
                    tv.Id == expectedAddress.Key);

                AddressResources.AssertEqual(expectedAddress.Value, actualAddress);
            }
        }

        internal static void AssertNotEqual(School expected, UpdateSchoolDTO actual)
        {
            Assert.NotNull(actual);
            Assert.NotEqual(expected.Name, actual.Name);
            AddressResources.AssertNotEqual(expected.DefaultAddress, actual.Address);
            Assert.NotEqual(expected.PhoneNo, actual.PhoneNo);
            Assert.NotEqual(expected.Email, actual.EmailAddress);
            Assert.NotEqual(expected.Website, actual.WebsiteURL);

            foreach (SchoolAddress schoolAddress in expected.SchoolAddresses)
            {
                if (schoolAddress.AddressId != expected.DefaultAddressId)
                {
                    UpdateAddressDTO actualAddress =
                        actual.AdditionalTrainingVenues[schoolAddress.Address.Id.ToString()];

                    // It doesn't matter if the address doesn't appear in the UpdateDTO, this would cause the
                    // repository to remove the address from the school and is expected behaviour. However, if there
                    // is an address found, it must be different for the purposes of testing
                    if (actualAddress != null)
                    {
                        AddressResources.AssertNotEqual(schoolAddress.Address, actualAddress);
                    }
                }
            }
        }

        private static void AssertEqualRestrictedDTO(School expected, SchoolDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Art.Id.ToString(), actual.ArtId);
            Assert.Equal(expected.Art.Name, actual.ArtName);
            Assert.Equal(expected.Organisation.Id.ToString(), actual.OrganisationId);
            Assert.Equal(expected.Organisation.Initials, actual.OrganisationName);
            Assert.Equal(expected.HeadInstructorId?.ToString(), actual.HeadInstructorId);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Null(actual.Address);
            Assert.Null(actual.PhoneNo);
            Assert.Null(actual.EmailAddress);
            Assert.Null(actual.WebsiteURL);

            if (expected.HeadInstructor != null)
            {
                Assert.Equal(expected.HeadInstructor.FullName, actual.HeadInstructorName);
            }
            else
            {
                Assert.Null(actual.HeadInstructorName);
            }
        }
    }
}
