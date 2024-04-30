// <copyright file="GetSchoolsForStudentTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.SchoolsRepositoryTests
{
    public class GetSchoolsForStudentTests : BaseTestClass
    {
        [Test]
        public async Task CanGetStudentSchoolsWithoutInsuranceOrLicence()
        {
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);
            List<School> testSchools = SchoolResources.CreateTestSchools(10, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsHaveStudent(
                testSchools.Select(p => p.Id).ToList(),
                testStudent.Id,
                DbIdentifier);

            List<StudentSchoolDTO> retrievedStudentSchools =
                await SchoolsRepository.GetSchoolsForPersonAsync(testStudent.Id);

            Assert.NotNull(retrievedStudentSchools);

            SchoolResources.AssertEqual(testSchools, retrievedStudentSchools);
            SchoolResources.AssertExist(retrievedStudentSchools, DbIdentifier);

            foreach (StudentSchoolDTO retrievedStudentSchool in retrievedStudentSchools)
            {
                Assert.IsNull(retrievedStudentSchool.InsuranceDocument);
                Assert.IsNull(retrievedStudentSchool.LicenceDocument);

                SchoolStudent schoolStudent = SchoolStudentResources.GetStudentSchool(
                    testStudent.Id,
                    retrievedStudentSchool.School.Id,
                    DbIdentifier);

                Assert.IsNull(schoolStudent.InsuranceDocument);
                Assert.IsNull(schoolStudent.LicenceDocument);
            }
        }

        [Test]
        public async Task CanGetStudentSchoolsWithLicenceAndNoInsurance()
        {
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);
            List<School> testSchools = SchoolResources.CreateTestSchools(10, DbIdentifier);

            foreach (School testSchool in testSchools)
            {
                SchoolStudentResources.EnsureSchoolHasStudent(
                    testSchool.Id, testStudent.Id, DbIdentifier);

                Document testLicenceDocument = DocumentResources.CreateTestDocument(DbIdentifier);

                SchoolStudentResources.EnsureStudentHasLicenceDocument(
                    testSchool.Id,
                    testStudent.Id,
                    testLicenceDocument.Id,
                    DbIdentifier);
            }

            List<StudentSchoolDTO> retrievedStudentSchools =
                await SchoolsRepository.GetSchoolsForPersonAsync(testStudent.Id);

            Assert.NotNull(retrievedStudentSchools);

            SchoolResources.AssertEqual(testSchools, retrievedStudentSchools);
            SchoolResources.AssertExist(retrievedStudentSchools, DbIdentifier);

            foreach (StudentSchoolDTO retrievedStudentSchool in retrievedStudentSchools)
            {
                Assert.Null(retrievedStudentSchool.InsuranceDocument);
                Assert.NotNull(retrievedStudentSchool.LicenceDocument);

                SchoolStudent schoolStudent = SchoolStudentResources.GetStudentSchool(
                    testStudent.Id,
                    retrievedStudentSchool.School.Id,
                    DbIdentifier);

                Assert.IsNull(schoolStudent.InsuranceDocument);
                DocumentResources.AssertEqual(
                    schoolStudent.LicenceDocument,
                    retrievedStudentSchool.LicenceDocument);
            }
        }

        [Test]
        public async Task CanGetStudentSchoolsWithInsuranceAndNoLicence()
        {
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);
            List<School> testSchools = SchoolResources.CreateTestSchools(10, DbIdentifier);

            foreach (School testSchool in testSchools)
            {
                SchoolStudentResources.EnsureSchoolHasStudent(
                    testSchool.Id, testStudent.Id, DbIdentifier);

                Document testInsuranceDocument = DocumentResources.CreateTestDocument(DbIdentifier);

                SchoolStudentResources.EnsureStudentHasInsuranceDocument(
                    testSchool.Id,
                    testStudent.Id,
                    testInsuranceDocument.Id,
                    DbIdentifier);
            }

            List<StudentSchoolDTO> retrievedStudentSchools =
                await SchoolsRepository.GetSchoolsForPersonAsync(testStudent.Id);

            Assert.NotNull(retrievedStudentSchools);

            SchoolResources.AssertEqual(testSchools, retrievedStudentSchools);
            SchoolResources.AssertExist(retrievedStudentSchools, DbIdentifier);

            foreach (StudentSchoolDTO retrievedStudentSchool in retrievedStudentSchools)
            {
                Assert.NotNull(retrievedStudentSchool.InsuranceDocument);
                Assert.Null(retrievedStudentSchool.LicenceDocument);

                SchoolStudent schoolStudent = SchoolStudentResources.GetStudentSchool(
                    testStudent.Id,
                    retrievedStudentSchool.School.Id,
                    DbIdentifier);

                DocumentResources.AssertEqual(
                    schoolStudent.InsuranceDocument,
                    retrievedStudentSchool.InsuranceDocument);
                Assert.IsNull(schoolStudent.LicenceDocument);
            }
        }

        [Test]
        public async Task CanGetStudentSchoolsWithInsuranceAndLicence()
        {
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);
            List<School> testSchools = SchoolResources.CreateTestSchools(10, DbIdentifier);

            foreach (School testSchool in testSchools)
            {
                SchoolStudentResources.EnsureSchoolHasStudent(
                    testSchool.Id, testStudent.Id, DbIdentifier);

                Document testInsuranceDocument = DocumentResources.CreateTestDocument(DbIdentifier);
                Document testLicenceDocument = DocumentResources.CreateTestDocument(
                    DbIdentifier,
                    testInsuranceDocument.FiledDate,
                    testInsuranceDocument.DocumentDate,
                    testInsuranceDocument.ExpiryDate);

                DocumentResources.AssertNotEqual(testInsuranceDocument, testLicenceDocument);

                SchoolStudentResources.EnsureStudentHasInsuranceDocument(
                    testSchool.Id,
                    testStudent.Id,
                    testInsuranceDocument.Id,
                    DbIdentifier);
                SchoolStudentResources.EnsureStudentHasLicenceDocument(
                    testSchool.Id,
                    testStudent.Id,
                    testLicenceDocument.Id,
                    DbIdentifier);
            }

            List<StudentSchoolDTO> retrievedStudentSchools =
                await SchoolsRepository.GetSchoolsForPersonAsync(testStudent.Id);

            Assert.NotNull(retrievedStudentSchools);

            SchoolResources.AssertEqual(testSchools, retrievedStudentSchools);
            SchoolResources.AssertExist(retrievedStudentSchools, DbIdentifier);

            foreach (StudentSchoolDTO retrievedStudentSchool in retrievedStudentSchools)
            {
                Assert.NotNull(retrievedStudentSchool.InsuranceDocument);
                Assert.NotNull(retrievedStudentSchool.LicenceDocument);

                SchoolStudent schoolStudent = SchoolStudentResources.GetStudentSchool(
                    testStudent.Id,
                    retrievedStudentSchool.School.Id,
                    DbIdentifier);

                DocumentResources.AssertEqual(
                    schoolStudent.InsuranceDocument,
                    retrievedStudentSchool.InsuranceDocument);
                DocumentResources.AssertEqual(
                    schoolStudent.LicenceDocument,
                    retrievedStudentSchool.LicenceDocument);
            }
        }
    }
}
