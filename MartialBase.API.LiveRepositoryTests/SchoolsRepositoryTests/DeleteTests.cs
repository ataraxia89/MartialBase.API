// <copyright file="DeleteTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.SchoolsRepositoryTests
{
    public class DeleteTests : BaseTestClass
    {
        [Test]
        public async Task CanDeleteSchool()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            List<Address> testAddresses = AddressResources.CreateTestAddresses(10, DbIdentifier);

            SchoolResources.EnsureSchoolHasAddresses(
                testSchool.Id,
                testAddresses.Select(p => p.Id).ToList(),
                DbIdentifier);

            await SchoolsRepository.DeleteAsync(testSchool.Id);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            Assert.IsFalse(SchoolResources.CheckExists(testSchool.Id, DbIdentifier));

            AddressResources.AssertDoNotExist(
                testAddresses.Select(a => a.Id).ToList(), DbIdentifier);
        }

        [Test]
        public async Task CanDeleteSchoolWithStudents()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            List<Address> testAddresses = AddressResources.CreateTestAddresses(10, DbIdentifier);
            List<Person> testStudents = PersonResources.CreateTestPeople(10, DbIdentifier, false);

            SchoolResources.EnsureSchoolHasAddresses(
                testSchool.Id,
                testAddresses.Select(p => p.Id).ToList(),
                DbIdentifier);
            SchoolStudentResources.EnsureSchoolHasStudents(
                testSchool.Id,
                testStudents.Select(p => p.Id).ToList(),
                DbIdentifier);
            OrganisationPersonResources.EnsureOrganisationHasPeople(
                testSchool.Organisation.Id,
                testStudents.Select(p => p.Id).ToList(),
                DbIdentifier);

            await SchoolsRepository.DeleteAsync(testSchool.Id);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            Assert.IsFalse(SchoolResources.CheckExists(testSchool.Id, DbIdentifier));

            Assert.IsFalse(AddressResources.CheckExists(
                testSchool.DefaultAddress.Id, DbIdentifier));
            AddressResources.AssertDoNotExist(
                testAddresses.Select(a => a.Id).ToList(), DbIdentifier);
            SchoolStudentResources.AssertSchoolDoesNotHaveStudents(
                testSchool.Id,
                testStudents.Select(s => s.Id).ToList(),
                DbIdentifier);

            foreach (Person testStudent in testStudents)
            {
                PersonResources.AssertExists(testStudent, DbIdentifier);

                (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                    testSchool.Organisation.Id, testStudent.Id, DbIdentifier);

                Assert.IsTrue(personExists);
            }
        }
    }
}
