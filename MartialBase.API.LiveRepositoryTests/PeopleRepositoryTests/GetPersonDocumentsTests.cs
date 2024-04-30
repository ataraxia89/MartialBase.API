// <copyright file="GetPersonDocumentsTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.PeopleRepositoryTests
{
    /// <summary>
    /// Unit tests with a live database for the
    /// <see cref="PeopleRepository.GetPersonDocumentsAsync">GetPersonDocuments</see> method in the
    /// <see cref="PeopleRepository"/>.
    /// </summary>
    public class GetPersonDocumentsTests : BaseTestClass
    {
        /// <summary>
        /// Verifies that the <see cref="PeopleRepository.GetPersonDocumentsAsync">GetPersonDocuments</see> method
        /// successfully retrieves a <see cref="DocumentDTO"/> based on a provided ID.
        /// </summary>
        [Test]
        public async Task CanGetPersonDocuments()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            List<Document> testDocuments = DocumentResources.CreateTestDocuments(10, DbIdentifier);

            PersonDocumentResources.EnsurePersonHasDocuments(
                testPerson.Id,
                testDocuments.Select(d => d.Id).ToList(),
                DbIdentifier);

            // Act
            List<DocumentDTO> retrievedDocuments =
                await PeopleRepository.GetPersonDocumentsAsync(
                    testPerson.Id, true);

            // Assert
            DocumentResources.AssertEqual(testDocuments, retrievedDocuments);
        }

        /// <summary>
        /// Verifies that the <see cref="PeopleRepository.GetPersonDocumentsAsync">GetPersonDocuments</see> method
        /// only returns active <see cref="DocumentDTO"/>s when specified.
        /// </summary>
        [Test]
        public async Task GetPersonDocumentsOnlyReturnsActiveDocumentsWhenSpecified()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            List<Document> activeDocuments = DocumentResources.CreateTestDocuments(10, DbIdentifier);
            List<Document> inactiveDocuments = DocumentResources.CreateTestDocuments(10, DbIdentifier);

            var activeDocumentIds = activeDocuments.Select(d => d.Id).ToList();
            var inactiveDocumentIds = inactiveDocuments.Select(d => d.Id).ToList();

            PersonDocumentResources.EnsurePersonHasDocuments(
                testPerson.Id, activeDocumentIds, DbIdentifier);
            PersonDocumentResources.EnsurePersonHasDocuments(
                testPerson.Id, inactiveDocumentIds, DbIdentifier);

            PersonDocumentResources.EnsurePersonDocumentsActiveStatus(
                testPerson.Id, activeDocumentIds, true, DbIdentifier);
            PersonDocumentResources.EnsurePersonDocumentsActiveStatus(
                testPerson.Id, inactiveDocumentIds, false, DbIdentifier);

            // Act
            List<DocumentDTO> retrievedDocuments =
                await PeopleRepository.GetPersonDocumentsAsync(
                    testPerson.Id, false);

            // Assert
            DocumentResources.AssertEqual(activeDocuments, retrievedDocuments);
        }

        /// <summary>
        /// Verifies that the <see cref="PeopleRepository.GetPersonDocumentsAsync">GetPersonDocuments</see> method
        /// throws an <see cref="InvalidOperationException"/> when provided with an invalid <see cref="Person"/>
        /// ID.
        /// </summary>
        [Test]
        public async Task GetPersonDocumentsForNonExistentPersonThrowsInvalidOperationException() =>

            // Act & Assert
            Assert.That(
                async () => await PeopleRepository.GetPersonDocumentsAsync(
                    Guid.NewGuid(), true),
                Throws.Exception.TypeOf<InvalidOperationException>());

        /// <summary>
        /// Verifies that the <see cref="PeopleRepository.GetPersonDocumentsAsync">GetPersonDocuments</see> method
        /// returns an empty <see cref="List{T}"/> of <see cref="DocumentDTO"/> objects when the
        /// <see cref="Person"/> has no <see cref="Document">Documents</see> assigned.
        /// </summary>
        [Test]
        public async Task GetPersonDocumentsReturnsEmptyListWhenNoDocumentsAreFound()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            // Act
            List<DocumentDTO> retrievedDocuments =
                await PeopleRepository.GetPersonDocumentsAsync(
                    testPerson.Id, true);

            // Assert
            Assert.NotNull(retrievedDocuments);
            Assert.IsEmpty(retrievedDocuments);
        }
    }
}
