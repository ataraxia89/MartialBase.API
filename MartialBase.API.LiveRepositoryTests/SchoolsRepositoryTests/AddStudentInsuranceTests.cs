// <copyright file="AddStudentInsuranceTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Documents;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.SchoolsRepositoryTests
{
    public class AddStudentInsuranceTests : BaseTestClass
    {
        [Test]
        public async Task CanAddStudentInsuranceAndArchiveExisting()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);
            Document oldInsuranceDocument = DocumentResources.CreateTestDocument(DbIdentifier);

            DocumentType testDocumentType = DocumentTypeResources.CreateTestDocumentType(DbIdentifier);
            CreateDocumentInternalDTO newInsuranceDocument = DataGenerator.Documents.GenerateCreateDocumentInternalDTO(
                testDocumentType.Id,
                oldInsuranceDocument.DocumentDate,
                oldInsuranceDocument.ExpiryDate);

            DocumentResources.AssertNotEqual(oldInsuranceDocument, newInsuranceDocument);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testStudent.Id, DbIdentifier);
            SchoolStudentResources.EnsureStudentHasInsuranceDocument(
                testSchool.Id,
                testStudent.Id,
                oldInsuranceDocument.Id,
                DbIdentifier);

            DocumentDTO createdInsuranceDTO = await SchoolsRepository.AddStudentInsuranceAsync(
                testSchool.Id,
                testStudent.Id,
                newInsuranceDocument,
                archiveExisting: true);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            DocumentResources.AssertEqual(newInsuranceDocument, createdInsuranceDTO);
            DocumentResources.AssertExists(createdInsuranceDTO, DbIdentifier);

            (bool personDocumentExists, bool? personDocumentIsActive) =
                DocumentResources.CheckPersonDocumentExists(testStudent.Id, oldInsuranceDocument.Id, DbIdentifier);

            Assert.IsTrue(personDocumentExists);
            Assert.IsFalse(personDocumentIsActive);
            Assert.IsTrue(DocumentResources.CheckExists(oldInsuranceDocument.Id, DbIdentifier));
        }

        [Test]
        public async Task CanAddStudentInsuranceAndRemoveExisting()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);
            Document oldInsuranceDocument = DocumentResources.CreateTestDocument(DbIdentifier);

            DocumentType testDocumentType = DocumentTypeResources.CreateTestDocumentType(DbIdentifier);
            CreateDocumentInternalDTO newInsuranceDocument =
                DataGenerator.Documents.GenerateCreateDocumentInternalDTO(
                    testDocumentType.Id,
                    oldInsuranceDocument.DocumentDate,
                    oldInsuranceDocument.ExpiryDate);

            DocumentResources.AssertNotEqual(oldInsuranceDocument, newInsuranceDocument);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testStudent.Id, DbIdentifier);
            SchoolStudentResources.EnsureStudentHasInsuranceDocument(
                testSchool.Id,
                testStudent.Id,
                oldInsuranceDocument.Id,
                DbIdentifier);

            DocumentDTO createdInsuranceDTO = await SchoolsRepository.AddStudentInsuranceAsync(
                testSchool.Id,
                testStudent.Id,
                newInsuranceDocument,
                archiveExisting: false);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            DocumentResources.AssertEqual(newInsuranceDocument, createdInsuranceDTO);
            DocumentResources.AssertExists(createdInsuranceDTO, DbIdentifier);

            (bool personDocumentExists, bool? personDocumentIsActive) = DocumentResources.CheckPersonDocumentExists(
                testStudent.Id,
                oldInsuranceDocument.Id,
                DbIdentifier);
            Assert.IsFalse(personDocumentExists);
            Assert.IsNull(personDocumentIsActive);
            Assert.IsFalse(DocumentResources.CheckExists(oldInsuranceDocument.Id, DbIdentifier));
        }
    }
}
