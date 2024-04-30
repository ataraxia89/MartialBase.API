// <copyright file="RemoveStudentFromSchoolTests.cs" company="Martialtech®">
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
    public class RemoveStudentFromSchoolTests : BaseTestClass
    {
        [Test]
        public async Task CanRemoveStudentFromSchool()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testStudent.Id, DbIdentifier);
            OrganisationPersonResources.EnsureOrganisationHasPerson(
                testSchool.Organisation.Id,
                testStudent.Id,
                DbIdentifier);

            await SchoolsRepository.RemoveStudentFromSchoolAsync(testSchool.Id, testStudent.Id);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            (bool studentExists, _, _) = SchoolStudentResources.CheckSchoolHasStudent(
                testSchool.Id, testStudent.Id, DbIdentifier);

            Assert.IsFalse(studentExists);
            Assert.IsTrue(PersonResources.CheckExists(testStudent.Id, DbIdentifier));

            (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testSchool.Organisation.Id,
                testStudent.Id,
                DbIdentifier);

            Assert.IsTrue(personExists);
        }
    }
}
