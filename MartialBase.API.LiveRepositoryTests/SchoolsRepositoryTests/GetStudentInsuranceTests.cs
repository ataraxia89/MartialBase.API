// <copyright file="GetStudentInsuranceTests.cs" company="Martialtech®">
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
    public class GetStudentInsuranceTests : BaseTestClass
    {
        [Test]
        public async Task CanGetStudentInsurance()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);
            Document testInsuranceDocument = DocumentResources.CreateTestDocument(DbIdentifier);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testStudent.Id, DbIdentifier);
            SchoolStudentResources.EnsureStudentHasInsuranceDocument(
                testSchool.Id,
                testStudent.Id,
                testInsuranceDocument.Id,
                DbIdentifier);

            DocumentDTO retrievedInsuranceDocument =
                await SchoolsRepository.GetStudentInsuranceAsync(testSchool.Id, testStudent.Id);

            DocumentResources.AssertEqual(testInsuranceDocument, retrievedInsuranceDocument);
        }

        [Test]
        public async Task GetStudentInsuranceReturnsNullWhenNoInsuranceDocumentIsStored()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testStudent.Id, DbIdentifier);

            SchoolStudent schoolStudent = SchoolStudentResources.GetSchoolStudent(
                testSchool.Id,
                testStudent.Id,
                DbIdentifier);

            Assert.IsNull(schoolStudent.InsuranceDocumentId);
            Assert.IsNull(schoolStudent.InsuranceDocument);

            DocumentDTO retrievedInsuranceDocument =
                await SchoolsRepository.GetStudentInsuranceAsync(testSchool.Id, testStudent.Id);

            Assert.IsNull(retrievedInsuranceDocument);
        }
    }
}
