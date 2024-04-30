// <copyright file="AddNewPersonToSchoolTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.SchoolsRepositoryTests
{
    public class AddNewPersonToSchoolTests : BaseTestClass
    {
        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public async Task CanAddNewPersonToSchool(bool isInstructor, bool isSecretary)
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            CreatePersonInternalDTO createPersonInternalDTO =
                DataGenerator.People.GenerateCreatePersonInternalDTO();

            PersonDTO createdPerson = await SchoolsRepository.AddNewPersonToSchoolAsync(
                testSchool.Id, createPersonInternalDTO, isInstructor, isSecretary);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            PersonResources.AssertEqual(createPersonInternalDTO, createdPerson);
            PersonResources.AssertExists(createdPerson, DbIdentifier);

            (bool studentExists, bool studentIsInstructor, bool studentIsSecretary) =
                SchoolStudentResources.CheckSchoolHasStudent(testSchool.Id, createdPerson.Id, DbIdentifier);

            Assert.IsTrue(studentExists);
            Assert.AreEqual(isInstructor, studentIsInstructor);
            Assert.AreEqual(isSecretary, studentIsSecretary);

            (bool personExists, bool personIsAdmin) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testSchool.Organisation.Id, createdPerson.Id, DbIdentifier);

            Assert.IsTrue(personExists);
            Assert.IsFalse(personIsAdmin);
        }
    }
}
