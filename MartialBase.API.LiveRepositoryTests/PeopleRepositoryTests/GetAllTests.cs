// <copyright file="GetAllTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.PeopleRepositoryTests
{
    public class GetAllTests : BaseTestClass
    {
        [Test]
        public async Task GetAllPeopleWithNoParametersThrowsArgumentException() => Assert.That(
            async () => await PeopleRepository.GetAllAsync(),
            Throws.Exception.TypeOf<ArgumentException>());

        [Test]
        public async Task GetAllPeopleWithNullParametersThrowsArgumentException() => Assert.That(
            async () =>
                await PeopleRepository.GetAllAsync(null, null),
            Throws.Exception.TypeOf<ArgumentException>());

        [Test]
        public async Task GetAllPeopleWithOrganisationAndSchoolThrowsNotImplementedException() => Assert.That(
            async () =>
                await PeopleRepository.GetAllAsync(Guid.NewGuid(), Guid.NewGuid()),
            Throws.Exception.TypeOf<NotImplementedException>());

        [Test]
        public async Task CanGetAllOrganisationPeopleWithAddresses()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier, true);

            OrganisationPersonResources.EnsureOrganisationHasPeople(
                testOrganisation.Id,
                testPeople.Select(p => p.Id).ToList(),
                DbIdentifier);

            List<PersonDTO> retrievedPeople = await PeopleRepository.GetAllAsync(testOrganisation.Id, null);

            PersonResources.AssertEqual(testPeople, retrievedPeople);
            PersonResources.AssertExist(retrievedPeople, DbIdentifier);
        }

        [Test]
        public async Task CanGetAllOrganisationPeopleWithoutAddresses()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier, false);

            OrganisationPersonResources.EnsureOrganisationHasPeople(
                testOrganisation.Id,
                testPeople.Select(p => p.Id).ToList(),
                DbIdentifier);

            List<PersonDTO> retrievedPeople = await PeopleRepository.GetAllAsync(testOrganisation.Id, null);

            Assert.AreEqual(testPeople.Count, retrievedPeople.Count);

            PersonResources.AssertEqual(testPeople, retrievedPeople);
            PersonResources.AssertExist(retrievedPeople, DbIdentifier);
        }

        [Test]
        public async Task CanGetAllOrganisationPeopleWithAndWithoutAddresses()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier, true);
            testPeople.AddRange(PersonResources.CreateTestPeople(
                10, DbIdentifier, false));

            OrganisationPersonResources.EnsureOrganisationHasPeople(
                testOrganisation.Id,
                testPeople.Select(p => p.Id).ToList(),
                DbIdentifier);

            List<PersonDTO> retrievedPeople = await PeopleRepository.GetAllAsync(testOrganisation.Id, null);

            PersonResources.AssertEqual(testPeople, retrievedPeople);
            PersonResources.AssertExist(retrievedPeople, DbIdentifier);
        }

        [Test]
        public async Task CanGetAllSchoolStudentsWithAddresses()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier, true);

            SchoolStudentResources.EnsureSchoolHasStudents(
                testSchool.Id,
                testPeople.Select(p => p.Id).ToList(),
                DbIdentifier);

            List<PersonDTO> retrievedPeople = await PeopleRepository.GetAllAsync(null, testSchool.Id);

            PersonResources.AssertEqual(testPeople, retrievedPeople);
            PersonResources.AssertExist(retrievedPeople, DbIdentifier);
        }

        [Test]
        public async Task CanGetAllSchoolStudentsWithoutAddresses()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            List<Person> testPeople = PersonResources.CreateTestPeople(10, DbIdentifier, false);

            SchoolStudentResources.EnsureSchoolHasStudents(
                testSchool.Id,
                testPeople.Select(p => p.Id).ToList(),
                DbIdentifier);

            List<PersonDTO> retrievedPeople = await PeopleRepository.GetAllAsync(null, testSchool.Id);

            PersonResources.AssertEqual(testPeople, retrievedPeople);
            PersonResources.AssertExist(retrievedPeople, DbIdentifier);
        }

        [Test]
        public async Task CanGetAllSchoolStudentsWithAndWithoutAddresses()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            List<Person> testPeople = PersonResources.CreateTestPeople(
                10, DbIdentifier, true);
            testPeople.AddRange(PersonResources.CreateTestPeople(
                10, DbIdentifier, false));

            SchoolStudentResources.EnsureSchoolHasStudents(
                testSchool.Id,
                testPeople.Select(p => p.Id).ToList(),
                DbIdentifier);

            List<PersonDTO> retrievedPeople = await PeopleRepository.GetAllAsync(null, testSchool.Id);

            PersonResources.AssertEqual(testPeople, retrievedPeople);
            PersonResources.AssertExist(retrievedPeople, DbIdentifier);
        }
    }
}
