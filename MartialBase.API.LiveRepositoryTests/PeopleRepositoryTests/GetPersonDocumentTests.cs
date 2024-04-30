// <copyright file="GetPersonDocumentTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

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
    /// <see cref="PeopleRepository.GetPersonDocumentAsync">GetPersonDocument</see> method in the
    /// <see cref="PeopleRepository"/>.
    /// </summary>
    public class GetPersonDocumentTests : BaseTestClass
    {
        /// <summary>
        /// Verifies that the <see cref="PeopleRepository.GetPersonDocumentAsync">GetPersonDocument</see> method
        /// successfully retrieves a <see cref="DocumentDTO"/> based on a provided ID.
        /// </summary>
        [Test]
        public async Task CanGetPersonDocument()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            Document testDocument = DocumentResources.CreateTestDocument(DbIdentifier);

            PersonDocumentResources.EnsurePersonHasDocument(
                testPerson.Id,
                testDocument.Id,
                DbIdentifier);

            // Act
            DocumentDTO retrievedDocument =
                await PeopleRepository.GetPersonDocumentAsync(
                    testPerson.Id,
                    testDocument.Id);

            // Assert
            DocumentResources.AssertEqual(testDocument, retrievedDocument);
        }

        /// <summary>
        /// Verifies that the <see cref="PeopleRepository.GetPersonDocumentAsync">GetPersonDocument</see> method
        /// throws an <see cref="InvalidOperationException"/> when provided with an invalid <see cref="Person"/>
        /// ID.
        /// </summary>
        [Test]
        public async Task GetPersonDocumentForNonExistentPersonThrowsInvalidOperationException()
        {
            // Arrange
            Document testDocument = DocumentResources.CreateTestDocument(DbIdentifier);

            // Act & Assert
            Assert.That(
                async () => await PeopleRepository.GetPersonDocumentAsync(
                    Guid.NewGuid(),
                    testDocument.Id),
                Throws.Exception.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verifies that the <see cref="PeopleRepository.GetPersonDocumentAsync">GetPersonDocument</see> method
        /// throws an <see cref="InvalidOperationException"/> when provided with an invalid <see cref="Document"/>
        /// ID.
        /// </summary>
        [Test]
        public async Task GetPersonDocumentForNonExistentDocumentThrowsInvalidOperationException()
        {
            // Arrange
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            // Act & Assert
            Assert.That(
                async () => await PeopleRepository.GetPersonDocumentAsync(
                    testPerson.Id,
                    Guid.NewGuid()),
                Throws.Exception.TypeOf<InvalidOperationException>());
        }
    }
}
