// <copyright file="SchoolHasStudentTests.cs" company="Martialtech®">
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
    public class SchoolHasStudentTests : BaseTestClass
    {
        [Test]
        public async Task CanCheckSchoolHasStudent()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testStudent.Id, DbIdentifier);

            Assert.IsTrue(await SchoolsRepository.SchoolHasStudentAsync(testSchool.Id, testStudent.Id));
        }
    }
}
