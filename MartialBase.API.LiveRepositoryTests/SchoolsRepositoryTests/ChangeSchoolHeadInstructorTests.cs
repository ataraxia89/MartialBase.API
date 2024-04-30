// <copyright file="ChangeSchoolHeadInstructorTests.cs" company="Martialtech®">
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
    public class ChangeSchoolHeadInstructorTests : BaseTestClass
    {
        [TestCase(true)]
        [TestCase(false)]
        public async Task CanChangeSchoolHeadInstructorAndSetSecretaryStatus(bool retainSecretary)
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person oldHeadInstructor = PersonResources.CreateTestPerson(DbIdentifier, false);
            Person newHeadInstructor = PersonResources.CreateTestPerson(DbIdentifier, false);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id,
                oldHeadInstructor.Id,
                DbIdentifier,
                true,
                true);
            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id,
                newHeadInstructor.Id,
                DbIdentifier,
                false,
                false);
            SchoolResources.EnsurePersonIsHeadInstructor(
                testSchool, oldHeadInstructor, DbIdentifier);

            await SchoolsRepository.ChangeSchoolHeadInstructorAsync(
                testSchool.Id, newHeadInstructor.Id, retainSecretary);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            Assert.IsTrue(SchoolResources.CheckPersonIsHeadInstructor(
                testSchool.Id,
                newHeadInstructor.Id,
                DbIdentifier));

            (bool studentExists, bool studentIsInstructor, bool studentIsSecretary) = SchoolStudentResources.CheckSchoolHasStudent(
                testSchool.Id, oldHeadInstructor.Id, DbIdentifier);

            Assert.IsTrue(studentExists);
            Assert.IsTrue(studentIsInstructor);
            Assert.AreEqual(retainSecretary, studentIsSecretary);
        }

        [Test]
        public async Task CanSetExistingSchoolStudentAsSchoolHeadInstructor()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person headInstructor = PersonResources.CreateTestPerson(DbIdentifier, false);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id,
                headInstructor.Id,
                DbIdentifier,
                false,
                false);

            await SchoolsRepository.ChangeSchoolHeadInstructorAsync(
                testSchool.Id, headInstructor.Id, true);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            Assert.IsTrue(SchoolResources.CheckPersonIsHeadInstructor(
                testSchool.Id, headInstructor.Id, DbIdentifier));

            (bool studentExists, bool studentIsInstructor, bool studentIsSecretary) =
                SchoolStudentResources.CheckSchoolHasStudent(testSchool.Id, headInstructor.Id, DbIdentifier);

            Assert.IsTrue(studentExists);
            Assert.IsTrue(studentIsInstructor);
            Assert.IsTrue(studentIsSecretary);
        }
    }
}
