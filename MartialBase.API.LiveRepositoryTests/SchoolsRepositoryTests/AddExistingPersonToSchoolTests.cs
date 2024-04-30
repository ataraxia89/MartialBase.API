// <copyright file="AddExistingPersonToSchoolTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.SchoolsRepositoryTests
{
    public class AddExistingPersonToSchoolTests : BaseTestClass
    {
        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public async Task CanAddExistingPersonToSchool(bool isInstructor, bool isSecretary)
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            SchoolStudentResources.EnsureSchoolDoesNotHaveStudent(
                testSchool.Id, testPerson.Id, DbIdentifier);

            await SchoolsRepository.AddExistingPersonToSchoolAsync(
                testSchool.Id, testPerson.Id, isInstructor, isSecretary);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            (bool studentExists, bool studentIsInstructor, bool studentIsSecretary) =
                SchoolStudentResources.CheckSchoolHasStudent(testSchool.Id, testPerson.Id, DbIdentifier);

            Assert.IsTrue(studentExists);
            Assert.AreEqual(isInstructor, studentIsInstructor);
            Assert.AreEqual(isSecretary, studentIsSecretary);

            (bool personExists, bool personIsAdmin) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testSchool.Organisation.Id, testPerson.Id, DbIdentifier);

            Assert.IsTrue(personExists);
            Assert.IsFalse(personIsAdmin);
        }
    }
}
