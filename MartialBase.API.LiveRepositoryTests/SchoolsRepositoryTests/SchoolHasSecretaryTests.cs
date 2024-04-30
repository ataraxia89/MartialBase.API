// <copyright file="SchoolHasSecretaryTests.cs" company="Martialtech®">
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
    public class SchoolHasSecretaryTests : BaseTestClass
    {
        [Test]
        public async Task CanCheckSchoolHasSecretary()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testSecretary = PersonResources.CreateTestPerson(DbIdentifier, false);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id,
                testSecretary.Id,
                DbIdentifier,
                isSecretary: true);

            Assert.IsTrue(await SchoolsRepository.SchoolHasSecretaryAsync(testSchool.Id, testSecretary.Id));
        }
    }
}
