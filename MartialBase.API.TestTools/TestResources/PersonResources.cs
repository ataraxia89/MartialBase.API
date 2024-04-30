// <copyright file="PersonResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MartialBase.API.Data;
using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Tools;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class PersonResources
    {
        internal static bool CheckExists(Guid personId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.People.Any(p => p.Id == personId);
            }
        }

        internal static void AssertExist(List<SchoolStudentDTO> schoolStudents, string dbIdentifier)
        {
            foreach (SchoolStudentDTO schoolStudent in schoolStudents)
            {
                AssertExistsRestrictedDTO(schoolStudent.Student, dbIdentifier);
            }
        }

        internal static void AssertExist(List<PersonDTO> people, string dbIdentifier)
        {
            foreach (PersonDTO person in people)
            {
                AssertExists(person, dbIdentifier);
            }
        }

        internal static void AssertExists(PersonDTO person, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Person checkPerson = dbContext.People
                    .Include(p => p.Address)
                    .FirstOrDefault(p => p.Id == person.Id);
                AssertEqual(checkPerson, person);
            }
        }

        internal static void AssertExists(Person person, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Person checkPerson = dbContext.People
                    .Include(p => p.Address)
                    .FirstOrDefault(p => p.Id == person.Id);
                AssertEqual(checkPerson, person);
            }
        }

        internal static void AssertDoesNotExist(Guid personId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.False(dbContext.People.Any(p => p.Id == personId));
            }
        }

        internal static void AssertExistsRestrictedDTO(PersonDTO person, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Person checkPerson = dbContext.People
                    .Include(p => p.Address)
                    .FirstOrDefault(p => p.Id == person.Id);
                AssertEqualRestrictedDTO(checkPerson, person, person.Address != null);
            }
        }

        internal static async Task<Person> CreatePersonAsync(Person person, string dbIdentifier)
        {
            await using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.False(await dbContext.People.AnyAsync(p => p.Id == person.Id));

                await dbContext.People.AddAsync(person);

                Assert.True(await dbContext.SaveChangesAsync() > 0);
            }

            await using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(await dbContext.People.AnyAsync(p => p.Id == person.Id));
            }

            return person;
        }

        internal static Person CreateTestPerson(
            string dbIdentifier,
            bool includeAddress = true,
            bool realisticData = false) => CreateTestPeople(1, dbIdentifier, includeAddress, realisticData).First();

        internal static List<Person> CreateTestPeople(int numberToCreate, string dbIdentifier, bool includeAddresses = true, bool realisticData = false)
        {
            if (numberToCreate <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberToCreate),
                    numberToCreate,
                    $"Cannot create {numberToCreate} test people.");
            }

            var createdPeople = new List<Person>();

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                for (int i = 0; i < numberToCreate; i++)
                {
                    Person person = DataGenerator.People.GeneratePersonObject(includeAddresses, realisticData);

                    Assert.False(dbContext.People.Any(p => p.Id == person.Id));

                    dbContext.People.Add(person);
                    createdPeople.Add(person);
                }

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                foreach (Person createdPerson in createdPeople)
                {
                    Person checkPerson = dbContext.People
                        .Include(p => p.Address)
                        .FirstOrDefault(p => p.Id == createdPerson.Id);
                    AssertEqual(createdPerson, checkPerson);
                }
            }

            return createdPeople;
        }

        internal static List<Person> CreateTestPeople(Person templatePerson, int numberToCreate, string dbIdentifier, bool includeAddresses = false)
        {
            if (numberToCreate <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberToCreate),
                    numberToCreate,
                    $"Cannot create {numberToCreate} test people.");
            }

            var createdPeople = new List<Person>();

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                for (int i = 0; i < numberToCreate; i++)
                {
                    Person person = DataGenerator.People.GeneratePersonObject(includeAddresses);
                    person.Email = templatePerson.Email;
                    person.FirstName = templatePerson.FirstName;
                    person.MiddleName = templatePerson.MiddleName;
                    person.LastName = templatePerson.LastName;

                    Assert.False(dbContext.People.Any(p => p.Id == person.Id));

                    dbContext.People.Add(person);
                    createdPeople.Add(person);
                }

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                foreach (Person createdPerson in createdPeople)
                {
                    Person checkPerson = dbContext.People
                        .Include(p => p.Address)
                        .FirstOrDefault(p => p.Id == createdPerson.Id);
                    AssertEqual(createdPerson, checkPerson);
                }
            }

            return createdPeople;
        }

        internal static Person GetPerson(Guid personId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.People
                    .Include(p => p.Address)
                    .FirstOrDefault(p => p.Id == personId);
            }
        }

        internal static Person GetPersonFromAzureId(string azureId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.MartialBaseUsers
                    .Include(u => u.Person)
                    .ThenInclude(p => p.Address)
                    .FirstOrDefault(u => u.AzureId == new Guid(azureId))?.Person;
            }
        }

        internal static List<Person> FindPersonByName(string firstName, string lastName, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.People
                    .Include(p => p.Address)
                    .Where(p => p.FirstName == firstName && p.LastName == lastName).ToList();
            }
        }

        internal static Person DuplicatePersonObject(Person person) => new ()
        {
            Id = person.Id,
            Title = person.Title,
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DoB = person.DoB,
            AddressId = person.AddressId,
            Address = person.Address == null ? null : AddressResources.DuplicateAddressObject(person.Address),
            MobileNo = person.MobileNo,
            Email = person.Email
        };

        internal static void AssertEqual(Person expected, Person actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.MiddleName, actual.MiddleName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.DoB, actual.DoB);
            Assert.Equal(expected.MobileNo, actual.MobileNo);
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.AddressId, actual.AddressId);

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqual(Person expected, PersonDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.MiddleName, actual.MiddleName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.DoB, actual.DateOfBirth);
            Assert.Equal(expected.MobileNo, actual.MobileNo);
            Assert.Equal(expected.Email, actual.Email);

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqual(PersonDTO expected, Person actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.MiddleName, actual.MiddleName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.DateOfBirth, actual.DoB);
            Assert.Equal(expected.MobileNo, actual.MobileNo);
            Assert.Equal(expected.Email, actual.Email);

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqual(PersonDTO expected, PersonDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.MiddleName, actual.MiddleName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.DateOfBirth, actual.DateOfBirth);
            Assert.Equal(expected.MobileNo, actual.MobileNo);
            Assert.Equal(expected.Email, actual.Email);

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqual(CreatePersonDTO expected, PersonDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.MiddleName, actual.MiddleName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.DateOfBirth, actual.DateOfBirth?.ToString("yyyy-MM-dd"));
            Assert.Equal(expected.MobileNo, actual.MobileNo);
            Assert.Equal(expected.Email, actual.Email);

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqual(CreatePersonInternalDTO expected, PersonDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.MiddleName, actual.MiddleName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.DateOfBirth, actual.DateOfBirth);
            Assert.Equal(expected.MobileNo, actual.MobileNo);
            Assert.Equal(expected.Email, actual.Email);

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqual(CreatePersonDTO expected, Person actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.MiddleName, actual.MiddleName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.DateOfBirth, actual.DoB?.ToString("yyyy-MM-dd"));
            Assert.Equal(expected.MobileNo, actual.MobileNo);
            Assert.Equal(expected.Email, actual.Email);

            if (expected.Address != null)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqual(UpdatePersonDTO expected, PersonDTO actual, bool checkAddress = true)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.MiddleName, actual.MiddleName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.MobileNo, actual.MobileNo);
            Assert.Equal(expected.Email, actual.Email);

            if (checkAddress)
            {
                if (expected.Address != null)
                {
                    AddressResources.AssertEqual(expected.Address, actual.Address);
                }
                else
                {
                    Assert.Null(actual.Address);
                }
            }
        }

        internal static void AssertEqual(List<Person> expected, List<PersonDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (Person expectedPerson in expected)
            {
                PersonDTO actualPerson = actual.FirstOrDefault(p =>
                    p.Id == expectedPerson.Id);

                AssertEqual(expectedPerson, actualPerson);
            }
        }

        internal static void AssertEqual(List<Person> expected, List<SchoolStudentDTO> actual, bool hasAddresses = false)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (Person expectedStudent in expected)
            {
                PersonDTO actualStudent =
                    actual.FirstOrDefault(ss => ss.Student.Id == expectedStudent.Id)?
                        .Student;

                AssertEqualRestrictedDTO(expectedStudent, actualStudent, hasAddresses);
            }
        }

        internal static void AssertEqual(List<Person> expected, List<OrganisationPersonDTO> actual, bool hasAddresses = false)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (Person expectedPerson in expected)
            {
                PersonDTO actualPerson = actual.FirstOrDefault(op =>
                    op.Person.Id == expectedPerson.Id)?.Person;

                AssertEqualRestrictedDTO(expectedPerson, actualPerson, hasAddresses);
            }
        }

        internal static void AssertEqual(List<OrganisationPersonDTO> expected, List<OrganisationPersonDTO> actual, bool hasAddresses = false)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (OrganisationPersonDTO expectedPerson in expected)
            {
                OrganisationPersonDTO actualPerson = actual.FirstOrDefault(op =>
                    op.Person.Id == expectedPerson.Person.Id);

                AssertEqualRestrictedDTO(expectedPerson.Person, actualPerson?.Person, hasAddresses);
                Assert.Equal(expectedPerson.IsAdmin, actualPerson?.IsAdmin);
            }
        }

        internal static void AssertNotEqual(Person expected, UpdatePersonDTO actual)
        {
            Assert.NotNull(actual);
            Assert.NotEqual(expected.Title, actual.Title);
            Assert.NotEqual(expected.FirstName, actual.FirstName);
            Assert.NotEqual(expected.MiddleName, actual.MiddleName);
            Assert.NotEqual(expected.LastName, actual.LastName);
            Assert.NotEqual(expected.MobileNo, actual.MobileNo);
            Assert.NotEqual(expected.Email, actual.Email);

            if (expected.Address != null && actual.Address != null)
            {
                AddressResources.AssertNotEqual(expected.Address, actual.Address);
            }
        }

        internal static void AssertEqualRestrictedDTO(CreatePersonDTO expected, PersonDTO actual, bool hasAddress)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.MiddleName, actual.MiddleName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Null(actual.DateOfBirth);
            Assert.Null(actual.MobileNo);
            Assert.Null(actual.Email);

            if (hasAddress)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        internal static void AssertEqualRestrictedDTO(UpdatePersonDTO expected, PersonDTO actual, bool hasAddress)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.MiddleName, actual.MiddleName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Null(actual.DateOfBirth);
            Assert.Null(actual.MobileNo);
            Assert.Null(actual.Email);

            if (hasAddress)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        private static void AssertEqualRestrictedDTO(Person expected, PersonDTO actual, bool hasAddress)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Null(actual.MiddleName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Null(actual.DateOfBirth);
            Assert.Null(actual.MobileNo);
            Assert.Null(actual.Email);

            if (hasAddress)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }

        private static void AssertEqualRestrictedDTO(PersonDTO expected, PersonDTO actual, bool hasAddress)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Null(actual.MiddleName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Null(actual.DateOfBirth);
            Assert.Null(actual.MobileNo);
            Assert.Null(actual.Email);

            if (hasAddress)
            {
                AddressResources.AssertEqual(expected.Address, actual.Address);
            }
            else
            {
                Assert.Null(actual.Address);
            }
        }
    }
}
