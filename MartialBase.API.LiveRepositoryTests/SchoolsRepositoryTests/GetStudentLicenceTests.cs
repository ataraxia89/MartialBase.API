// <copyright file="GetStudentLicenceTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.SchoolsRepositoryTests
{
    public class GetStudentLicenceTests : BaseTestClass
    {
        [Test]
        public async Task CanGetStudentLicence()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);
            Document testLicenceDocument = DocumentResources.CreateTestDocument(DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testStudent.Id, DbIdentifier);
            SchoolStudentResources.EnsureStudentHasLicenceDocument(
                testSchool.Id,
                testStudent.Id,
                testLicenceDocument.Id,
                DbIdentifier);

            DocumentDTO retrievedLicenceDocument =
                await SchoolsRepository.GetStudentLicenceAsync(testSchool.Id, testStudent.Id);

            DocumentResources.AssertEqual(testLicenceDocument, retrievedLicenceDocument);
        }

        [Test]
        public async Task GetStudentLicenceReturnsNullWhenNoLicenceDocumentIsStored()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testStudent.Id, DbIdentifier);

            SchoolStudent schoolStudent = SchoolStudentResources.GetSchoolStudent(
                testSchool.Id,
                testStudent.Id,
                DbIdentifier);

            Assert.IsNull(schoolStudent.LicenceDocumentId);
            Assert.IsNull(schoolStudent.LicenceDocument);

            DocumentDTO retrievedLicenceDocument =
                await SchoolsRepository.GetStudentLicenceAsync(testSchool.Id, testStudent.Id);

            Assert.IsNull(retrievedLicenceDocument);
        }
    }
}
