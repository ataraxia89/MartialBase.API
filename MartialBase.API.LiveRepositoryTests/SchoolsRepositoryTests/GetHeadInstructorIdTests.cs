// <copyright file="GetHeadInstructorIdTests.cs" company="Martialtech®">
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
    public class GetHeadInstructorIdTests : BaseTestClass
    {
        [Test]
        public async Task CanGetSchoolHeadInstructorId()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testHeadInstructor = PersonResources.CreateTestPerson(DbIdentifier, false);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id,
                testHeadInstructor.Id,
                DbIdentifier,
                true,
                true);
            SchoolResources.EnsurePersonIsHeadInstructor(testSchool, testHeadInstructor, DbIdentifier);

            Assert.AreEqual(
                testHeadInstructor.Id, await SchoolsRepository.GetHeadInstructorIdAsync(testSchool.Id));
        }
    }
}
