// <copyright file="ExistsTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.DocumentsRepositoryTests
{
    /// <summary>
    /// Unit tests with a live database for the <see cref="DocumentsRepository.ExistsAsync">Exists</see> method in
    /// the <see cref="DocumentsRepository"/>.
    /// </summary>
    public class ExistsTests : BaseTestClass
    {
        /// <summary>
        /// Verifies that the <see cref="DocumentsRepository.ExistsAsync">Exists</see> method returns <c>true</c> for
        /// an existing document.
        /// </summary>
        [Test]
        public async Task CanCheckDocumentExists()
        {
            // Arrange
            Document testDocument = DocumentResources.CreateTestDocument(DbIdentifier);

            // Act & Assert
            Assert.IsTrue(await DocumentsRepository.ExistsAsync(testDocument.Id));
        }

        /// <summary>
        /// Verifies that the <see cref="DocumentsRepository.ExistsAsync">Exists</see> method returns <c>false</c> for
        /// a non-existent document.
        /// </summary>
        [Test]
        public async Task CanCheckDocumentDoesNotExist() =>

            // Act & Assert
            Assert.IsFalse(await DocumentsRepository.ExistsAsync(Guid.NewGuid()));
    }
}
