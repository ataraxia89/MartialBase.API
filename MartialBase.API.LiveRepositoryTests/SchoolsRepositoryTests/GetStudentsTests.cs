// <copyright file="GetStudentsTests.cs" company="Martialtech®">
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
    public class GetStudentsTests : BaseTestClass
    {
        [Test]
        public async Task CanGetSchoolStudentsWithoutInsuranceOrLicence()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            List<Person> testStudents =
                PersonResources.CreateTestPeople(10, DbIdentifier, true);

            SchoolStudentResources.EnsureSchoolHasStudents(
                testSchool.Id,
                testStudents.Select(p => p.Id).ToList(),
                DbIdentifier);

            List<SchoolStudentDTO> retrievedSchoolStudents =
                await SchoolsRepository.GetStudentsAsync(testSchool.Id);

            Assert.NotNull(retrievedSchoolStudents);

            PersonResources.AssertEqual(testStudents, retrievedSchoolStudents, true);
            PersonResources.AssertExist(retrievedSchoolStudents, DbIdentifier);

            foreach (SchoolStudentDTO retrievedSchoolStudent in retrievedSchoolStudents)
            {
                Assert.AreEqual(testSchool.Id, retrievedSchoolStudent.SchoolId);
                Assert.AreEqual(testSchool.Name, retrievedSchoolStudent.SchoolName);

                Assert.IsNull(retrievedSchoolStudent.InsuranceDocument);
                Assert.IsNull(retrievedSchoolStudent.LicenceDocument);

                SchoolStudent schoolStudent = SchoolStudentResources.GetSchoolStudent(
                    testSchool.Id,
                    retrievedSchoolStudent.Student.Id,
                    DbIdentifier);

                Assert.IsNull(schoolStudent.InsuranceDocument);
                Assert.IsNull(schoolStudent.LicenceDocument);
            }
        }

        [Test]
        public async Task CanGetSchoolStudentsWithoutAddresses()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            List<Person> testStudents =
                PersonResources.CreateTestPeople(10, DbIdentifier, false);

            SchoolStudentResources.EnsureSchoolHasStudents(
                testSchool.Id,
                testStudents.Select(p => p.Id).ToList(),
                DbIdentifier);

            List<SchoolStudentDTO> retrievedSchoolStudents =
                await SchoolsRepository.GetStudentsAsync(testSchool.Id);

            Assert.NotNull(retrievedSchoolStudents);

            PersonResources.AssertEqual(testStudents, retrievedSchoolStudents, false);
            PersonResources.AssertExist(retrievedSchoolStudents, DbIdentifier);

            foreach (SchoolStudentDTO retrievedSchoolStudent in retrievedSchoolStudents)
            {
                Assert.AreEqual(testSchool.Id, retrievedSchoolStudent.SchoolId);
                Assert.AreEqual(testSchool.Name, retrievedSchoolStudent.SchoolName);
            }
        }

        [Test]
        public async Task CanGetSchoolStudentsWithLicenceAndNoInsurance()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            List<Person> testStudents =
                PersonResources.CreateTestPeople(10, DbIdentifier, true);

            foreach (Person testStudent in testStudents)
            {
                SchoolStudentResources.EnsureSchoolHasStudent(
                    testSchool.Id, testStudent.Id, DbIdentifier);

                Document testLicenceDocument = DocumentResources.CreateTestDocument(DbIdentifier);

                SchoolStudentResources.EnsureStudentHasLicenceDocument(
                    testSchool.Id, testStudent.Id, testLicenceDocument.Id, DbIdentifier);
            }

            List<SchoolStudentDTO> retrievedSchoolStudents =
                await SchoolsRepository.GetStudentsAsync(testSchool.Id);

            Assert.NotNull(retrievedSchoolStudents);

            PersonResources.AssertEqual(testStudents, retrievedSchoolStudents, true);
            PersonResources.AssertExist(retrievedSchoolStudents, DbIdentifier);

            foreach (SchoolStudentDTO retrievedSchoolStudent in retrievedSchoolStudents)
            {
                Assert.AreEqual(testSchool.Id, retrievedSchoolStudent.SchoolId);
                Assert.AreEqual(testSchool.Name, retrievedSchoolStudent.SchoolName);

                Assert.IsNull(retrievedSchoolStudent.InsuranceDocument);
                Assert.NotNull(retrievedSchoolStudent.LicenceDocument);

                SchoolStudent schoolStudent = SchoolStudentResources.GetSchoolStudent(
                    testSchool.Id,
                    retrievedSchoolStudent.Student.Id,
                    DbIdentifier);

                Assert.IsNull(schoolStudent.InsuranceDocument);
                DocumentResources.AssertEqual(
                    schoolStudent.LicenceDocument,
                    retrievedSchoolStudent.LicenceDocument);
            }
        }

        [Test]
        public async Task CanGetSchoolStudentsWithInsuranceAndNoLicence()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            List<Person> testStudents =
                PersonResources.CreateTestPeople(10, DbIdentifier, true);

            foreach (Person testStudent in testStudents)
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

            List<SchoolStudentDTO> retrievedSchoolStudents =
                await SchoolsRepository.GetStudentsAsync(testSchool.Id);

            Assert.NotNull(retrievedSchoolStudents);

            PersonResources.AssertEqual(testStudents, retrievedSchoolStudents, true);
            PersonResources.AssertExist(retrievedSchoolStudents, DbIdentifier);

            foreach (SchoolStudentDTO retrievedSchoolStudent in retrievedSchoolStudents)
            {
                Assert.AreEqual(testSchool.Id, retrievedSchoolStudent.SchoolId);
                Assert.AreEqual(testSchool.Name, retrievedSchoolStudent.SchoolName);

                Assert.NotNull(retrievedSchoolStudent.InsuranceDocument);
                Assert.IsNull(retrievedSchoolStudent.LicenceDocument);

                SchoolStudent schoolStudent = SchoolStudentResources.GetSchoolStudent(
                    testSchool.Id,
                    retrievedSchoolStudent.Student.Id,
                    DbIdentifier);

                DocumentResources.AssertEqual(
                    schoolStudent.InsuranceDocument,
                    retrievedSchoolStudent.InsuranceDocument);
                Assert.IsNull(schoolStudent.LicenceDocument);
            }
        }

        [Test]
        public async Task CanGetSchoolStudentsWithInsuranceAndLicence()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            List<Person> testStudents =
                PersonResources.CreateTestPeople(10, DbIdentifier, true);

            foreach (Person testStudent in testStudents)
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

            List<SchoolStudentDTO> retrievedSchoolStudents =
                await SchoolsRepository.GetStudentsAsync(testSchool.Id);

            Assert.NotNull(retrievedSchoolStudents);

            PersonResources.AssertEqual(testStudents, retrievedSchoolStudents, true);
            PersonResources.AssertExist(retrievedSchoolStudents, DbIdentifier);

            foreach (SchoolStudentDTO retrievedSchoolStudent in retrievedSchoolStudents)
            {
                Assert.AreEqual(testSchool.Id, retrievedSchoolStudent.SchoolId);
                Assert.AreEqual(testSchool.Name, retrievedSchoolStudent.SchoolName);

                Assert.NotNull(retrievedSchoolStudent.InsuranceDocument);
                Assert.NotNull(retrievedSchoolStudent.LicenceDocument);

                SchoolStudent schoolStudent = SchoolStudentResources.GetSchoolStudent(
                    testSchool.Id,
                    retrievedSchoolStudent.Student.Id,
                    DbIdentifier);

                DocumentResources.AssertEqual(
                    schoolStudent.InsuranceDocument,
                    retrievedSchoolStudent.InsuranceDocument);
                DocumentResources.AssertEqual(
                    schoolStudent.LicenceDocument,
                    retrievedSchoolStudent.LicenceDocument);
            }
        }
    }
}
