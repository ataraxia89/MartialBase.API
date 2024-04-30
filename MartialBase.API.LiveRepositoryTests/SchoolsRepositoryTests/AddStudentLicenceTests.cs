// <copyright file="AddStudentLicenceTests.cs" company="Martialtech®">
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
    public class AddStudentLicenceTests : BaseTestClass
    {
        [Test]
        public async Task CanAddStudentLicenceAndArchiveExisting()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);
            Document oldLicenceDocument = DocumentResources.CreateTestDocument(DbIdentifier);

            DocumentType testDocumentType = DocumentTypeResources.CreateTestDocumentType(DbIdentifier);
            CreateDocumentInternalDTO newLicenceDocument =
                DataGenerator.Documents.GenerateCreateDocumentInternalDTO(
                    testDocumentType.Id, oldLicenceDocument.DocumentDate, oldLicenceDocument.ExpiryDate);

            DocumentResources.AssertNotEqual(oldLicenceDocument, newLicenceDocument);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testStudent.Id, DbIdentifier);
            SchoolStudentResources.EnsureStudentHasLicenceDocument(
                testSchool.Id,
                testStudent.Id,
                oldLicenceDocument.Id,
                DbIdentifier);

            DocumentDTO createdLicenceDTO = await SchoolsRepository.AddStudentLicenceAsync(
                testSchool.Id,
                testStudent.Id,
                newLicenceDocument,
                archiveExisting: true);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            DocumentResources.AssertEqual(newLicenceDocument, createdLicenceDTO);
            DocumentResources.AssertExists(createdLicenceDTO, DbIdentifier);

            (bool personDocumentExists, bool? personDocumentIsActive) = DocumentResources.CheckPersonDocumentExists(
                testStudent.Id,
                oldLicenceDocument.Id,
                DbIdentifier);
            Assert.IsTrue(personDocumentExists);
            Assert.IsFalse(personDocumentIsActive);
            Assert.IsTrue(DocumentResources.CheckExists(oldLicenceDocument.Id, DbIdentifier));
        }

        [Test]
        public async Task CanAddStudentLicenceAndRemoveExisting()
        {
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);
            Person testStudent = PersonResources.CreateTestPerson(DbIdentifier, false);
            Document oldLicenceDocument = DocumentResources.CreateTestDocument(DbIdentifier);

            DocumentType testDocumentType = DocumentTypeResources.CreateTestDocumentType(DbIdentifier);
            CreateDocumentInternalDTO newLicenceDocument =
                DataGenerator.Documents.GenerateCreateDocumentInternalDTO(
                    testDocumentType.Id,
                    oldLicenceDocument.DocumentDate,
                    oldLicenceDocument.ExpiryDate);

            DocumentResources.AssertNotEqual(oldLicenceDocument, newLicenceDocument);

            SchoolStudentResources.EnsureSchoolHasStudent(
                testSchool.Id, testStudent.Id, DbIdentifier);
            SchoolStudentResources.EnsureStudentHasLicenceDocument(
                testSchool.Id,
                testStudent.Id,
                oldLicenceDocument.Id,
                DbIdentifier);

            DocumentDTO createdLicenceDTO = await SchoolsRepository.AddStudentLicenceAsync(
                testSchool.Id,
                testStudent.Id,
                newLicenceDocument,
                archiveExisting: false);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            DocumentResources.AssertEqual(newLicenceDocument, createdLicenceDTO);
            DocumentResources.AssertExists(createdLicenceDTO, DbIdentifier);

            (bool personDocumentExists, bool? personDocumentIsActive) = DocumentResources.CheckPersonDocumentExists(
                testStudent.Id,
                oldLicenceDocument.Id,
                DbIdentifier);
            Assert.IsFalse(personDocumentExists);
            Assert.IsNull(personDocumentIsActive);
            Assert.IsFalse(DocumentResources.CheckExists(oldLicenceDocument.Id, DbIdentifier));
        }
    }
}
