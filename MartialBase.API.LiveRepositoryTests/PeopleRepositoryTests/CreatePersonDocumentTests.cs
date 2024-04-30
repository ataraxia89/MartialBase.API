// <copyright file="CreatePersonDocumentTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Documents;
using MartialBase.API.Data.Repositories;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.PeopleRepositoryTests
{
    /// <summary>
    /// Unit tests with a live database for the
    /// <see cref="PeopleRepository.CreatePersonDocumentAsync">CreatePersonDocument</see> method in the
    /// <see cref="PeopleRepository"/>.
    /// </summary>
    public class CreatePersonDocumentTests : BaseTestClass
    {
        /// <summary>
        /// Verifies that the <see cref="PeopleRepository.CreatePersonDocumentAsync">CreatePersonDocument</see> method
        /// successfully creates a <see cref="Document"/> and assigns it to a <see cref="Person"/> based on a
        /// provided <see cref="Person"/> ID and <see cref="CreateDocumentInternalDTO"/> object.
        /// </summary>
        [Test]
        public async Task CanCreatePersonDocument()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            DocumentType testDocumentType = DocumentTypeResources.CreateTestDocumentType(DbIdentifier);

            CreateDocumentInternalDTO testCreateDocumentDTO =
                DataGenerator.Documents.GenerateCreateDocumentInternalDTO(testDocumentType.Id);

            // Act
            DocumentDTO createdDocument =
                await PeopleRepository.CreatePersonDocumentAsync(testPerson.Id, testCreateDocumentDTO);

            // Assert
            DocumentResources.AssertEqual(testCreateDocumentDTO, createdDocument);

            Assert.True(await PeopleRepository.SaveChangesAsync());

            PersonDocumentResources.AssertExists(testPerson.Id, new Guid(createdDocument.Id), DbIdentifier);
        }

        /// <summary>
        /// Verifies that the <see cref="PeopleRepository.CreatePersonDocumentAsync">CreatePersonDocument</see> method
        /// throws an <see cref="InvalidOperationException"/> when provided with an invalid <see cref="Person"/>
        /// ID.
        /// </summary>
        [Test]
        public async Task CreatePersonDocumentForNonExistentPersonThrowsInvalidOperationException()
        {
            // Arrange
            DocumentType testDocumentType = DocumentTypeResources.CreateTestDocumentType(DbIdentifier);

            CreateDocumentInternalDTO testCreateDocumentDTO =
                DataGenerator.Documents.GenerateCreateDocumentInternalDTO(testDocumentType.Id);

            // Act & Assert
            Assert.That(
                async () => await PeopleRepository.CreatePersonDocumentAsync(
                    Guid.NewGuid(),
                    testCreateDocumentDTO),
                Throws.Exception.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verifies that the <see cref="PeopleRepository.CreatePersonDocumentAsync">CreatePersonDocument</see> method
        /// throws an <see cref="InvalidOperationException"/> when provided with a
        /// <see cref="CreateDocumentInternalDTO"/> containing an invalid <see cref="DocumentType"/> ID.
        /// </summary>
        [Test]
        public async Task CreatePersonDocumentWithNonExistentDocumentTypeThrowsInvalidOperationException()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            CreateDocumentInternalDTO testCreateDocumentDTO =
                DataGenerator.Documents.GenerateCreateDocumentInternalDTO(Guid.NewGuid());

            // Act & Assert
            Assert.That(
                async () => await PeopleRepository.CreatePersonDocumentAsync(
                    testPerson.Id,
                    testCreateDocumentDTO),
                Throws.Exception.TypeOf<InvalidOperationException>());
        }
    }
}
