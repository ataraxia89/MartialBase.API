// <copyright file="SchoolHasInstructorTests.cs" company="Martialtech®">
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
    public class SchoolHasInstructorTests : BaseTestClass
    {
        [Test]
        public async Task CanCheckSchoolHasInstructor()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testInstructor = PersonResources.CreateTestPerson(DbIdentifier, false);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id,
                testInstructor.Id,
                DbIdentifier,
                isInstructor: true);

            Assert.IsTrue(await SchoolsRepository.SchoolHasInstructorAsync(testSchool.Id, testInstructor.Id));
        }
    }
}
