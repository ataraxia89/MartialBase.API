// <copyright file="DocumentResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Documents;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.Tools;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class DocumentResources
    {
        internal static bool CheckExists(Guid documentId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.Documents.Any(d => d.Id == documentId);
            }
        }

        internal static void AssertExists(DocumentDTO document, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Document checkDocument = dbContext.Documents
                    .Include(d => d.DocumentType)
                    .ThenInclude(dt => dt.Organisation)
                    .ThenInclude(o => o.Address)
                    .FirstOrDefault(d => d.Id == new Guid(document.Id));
                AssertEqual(checkDocument, document);
            }
        }

        /// <summary>
        /// Checks if all <see cref="Document">Documents</see> in a provided <see cref="List{T}"/> exist on the
        /// database and deletes them if found.
        /// </summary>
        /// <param name="documents">The <see cref="List{T}"/> of <see cref="Document">Documents</see> to be checked.</param>
        /// <param name="dbIdentifier">The string identifier of the <see cref="MartialBaseDbContext"/> to be used.</param>
        internal static void EnsureDoesNotExist(List<Document> documents, string dbIdentifier)
        {
            foreach (Document document in documents)
            {
                EnsureDoesNotExist(document, dbIdentifier);
            }
        }

        /// <summary>
        /// Checks if a provided <see cref="Document"/> exists on the database and deletes it if found.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to be checked.</param>
        /// <param name="dbIdentifier">The string identifier of the <see cref="MartialBaseDbContext"/> to be used.</param>
        internal static void EnsureDoesNotExist(Document document, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Document dbDocument = dbContext.Documents.FirstOrDefault(d => d.Id == document.Id);

                if (dbDocument != null)
                {
                    var personDocuments =
                        dbContext.PersonDocuments
                            .Where(pd => pd.DocumentId == dbDocument.Id)
                            .ToList();

                    foreach (PersonDocument personDocument in personDocuments)
                    {
                        dbContext.PersonDocuments.Remove(personDocument);
                    }

                    dbContext.Documents.Remove(dbDocument);

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.Null(dbContext.Documents.FirstOrDefault(d => d.Id == document.Id));
            }
        }

        /// <summary>
        /// Creates a specified number of <see cref="Document">Documents</see> on the database.
        /// </summary>
        /// <param name="numberToCreate">The number of <see cref="Document">Documents</see> to be created.</param>
        /// <returns>A <see cref="List{T}"/> of newly-created <see cref="Document">Documents</see>.</returns>
        /// <param name="dbIdentifier">The string identifier of the <see cref="MartialBaseDbContext"/> to be used.</param>
        /// <param name="realisticData">Whether to use realistic data in the test object.</param>
        internal static List<Document> CreateTestDocuments(int numberToCreate, string dbIdentifier, bool realisticData = false)
        {
            if (numberToCreate <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberToCreate),
                    numberToCreate,
                    $"Cannot create {numberToCreate} test documents.");
            }

            var createdDocuments = new List<Document>();

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                for (int i = 0; i < numberToCreate; i++)
                {
                    Document document = DataGenerator.Documents.GenerateDocumentObject(realisticData);

                    Assert.False(dbContext.Documents.Any(d => d.Id == document.Id));

                    dbContext.Documents.Add(document);
                    createdDocuments.Add(document);
                }

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                foreach (Document createdDocument in createdDocuments)
                {
                    Document checkDocument = dbContext.Documents
                        .Include(d => d.DocumentType)
                        .ThenInclude(dt => dt.Organisation)
                        .ThenInclude(o => o.Address)
                        .FirstOrDefault(d => d.Id == createdDocument.Id);
                    AssertEqual(createdDocument, checkDocument);
                }
            }

            return createdDocuments;
        }

        internal static Document CreateTestDocument(
            string dbIdentifier,
            DateTime? avoidFiledDate = null,
            DateTime? avoidDocumentDate = null,
            DateTime? avoidExpiryDate = null,
            bool realisticData = false)
        {
            Document createdDocument = DataGenerator.Documents.GenerateDocumentObject(
                realisticData,
                avoidFiledDate,
                avoidDocumentDate,
                avoidExpiryDate);

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.False(dbContext.Documents.Any(d => d.Id == createdDocument.Id));

                dbContext.Documents.Add(createdDocument);

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Document checkDocument = dbContext.Documents
                    .Include(d => d.DocumentType)
                    .ThenInclude(dt => dt.Organisation)
                    .ThenInclude(o => o.Address)
                    .FirstOrDefault(d => d.Id == createdDocument.Id);
                AssertEqual(createdDocument, checkDocument);
            }

            return createdDocument;
        }

        internal static (bool Exists, bool? IsActive) CheckPersonDocumentExists(Guid personId, Guid documentId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                PersonDocument personDocument = dbContext.PersonDocuments.FirstOrDefault(pd =>
                    pd.PersonId == personId &&
                    pd.DocumentId == documentId);

                return (personDocument != null, personDocument?.IsActive);
            }
        }

        internal static void AssertEqual(Document expected, Document actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.DocumentTypeId, actual.DocumentTypeId);
            DocumentTypeResources.AssertEqual(expected.DocumentType, actual.DocumentType);
            Assert.Equal(expected.FiledDate, actual.FiledDate);
            Assert.Equal(expected.DocumentDate, actual.DocumentDate);
            Assert.Equal(expected.DocumentRef, actual.DocumentRef);
            Assert.Equal(expected.DocumentURL, actual.DocumentURL);
            Assert.Equal(expected.ExpiryDate, actual.ExpiryDate);
        }

        internal static void AssertEqual(Document expected, DocumentDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id.ToString(), actual.Id);
            Assert.Equal(expected.DocumentTypeId.ToString(), actual.TypeId);
            Assert.Equal(expected.DocumentType.Description, actual.Type);
            Assert.Contains(
                actual.Issuer,
                new[] { expected.DocumentType.Organisation.Initials, expected.DocumentType.Organisation.Name });
            Assert.Equal(expected.DocumentDate, actual.Date);
            Assert.Equal(expected.DocumentRef, actual.Reference);
            Assert.Equal(expected.DocumentURL, actual.URL);
            Assert.Equal(expected.ExpiryDate, actual.ExpiryDate);
        }

        /// <summary>
        /// Verifies that two <see cref="DocumentDTO">DocumentDTOs</see> are equal, including all properties.
        /// </summary>
        /// <param name="expected">The expected <see cref="DocumentDTO"/> object.</param>
        /// <param name="actual">The actual <see cref="DocumentDTO"/> object.</param>
        internal static void AssertEqual(DocumentDTO expected, DocumentDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.TypeId, actual.TypeId);
            Assert.Equal(expected.Type, actual.Type);
            Assert.Equal(expected.Issuer, actual.Issuer);
            Assert.Equal(expected.Date, actual.Date);
            Assert.Equal(expected.Reference, actual.Reference);
            Assert.Equal(expected.URL, actual.URL);
            Assert.Equal(expected.ExpiryDate, actual.ExpiryDate);
        }

        /// <summary>
        /// Verifies that a list of <see cref="Document">Documents</see> matches a list of
        /// <see cref="DocumentDTO">DocumentDTOs</see>, including all properties.
        /// </summary>
        /// <param name="expected">The expected <see cref="List{T}"/> of <see cref="Document">Documents</see>.</param>
        /// <param name="actual">The actual <see cref="List{T}"/> of <see cref="DocumentDTO">DocumentDTOs</see>.</param>
        internal static void AssertEqual(List<Document> expected, List<DocumentDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (Document expectedDocument in expected)
            {
                DocumentDTO actualDocument = actual
                    .FirstOrDefault(d => d.Id == expectedDocument.Id.ToString());

                AssertEqual(expectedDocument, actualDocument);
            }
        }

        /// <summary>
        /// Verifies that a list of <see cref="DocumentDTO">DocumentDTOs</see> matches another list of
        /// <see cref="DocumentDTO">DocumentDTOs</see>, including all properties.
        /// </summary>
        /// <param name="expected">The expected <see cref="List{T}"/> of <see cref="DocumentDTO">DocumentDTOs</see>.</param>
        /// <param name="actual">The actual <see cref="List{T}"/> of <see cref="DocumentDTO">DocumentDTOs</see>.</param>
        internal static void AssertEqual(List<DocumentDTO> expected, List<DocumentDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (DocumentDTO expectedDocument in expected)
            {
                DocumentDTO actualDocument = actual
                    .FirstOrDefault(d => d.Id == expectedDocument.Id);

                AssertEqual(expectedDocument, actualDocument);
            }
        }

        /// <summary>
        /// Verifies that the properties of a provided <see cref="DocumentDTO"/> match those of a provided
        /// <see cref="CreateDocumentDTO"/>.
        /// </summary>
        /// <param name="expected">The <see cref="CreateDocumentDTO"/> containing the expected values.</param>
        /// <param name="actual">The <see cref="DocumentDTO"/> containing the actual values.</param>
        internal static void AssertEqual(CreateDocumentDTO expected, DocumentDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.DocumentTypeId, actual.TypeId);
            Assert.Equal(expected.DocumentDate, actual.Date);
            Assert.Equal(expected.Reference, actual.Reference);
            Assert.Equal(expected.URL, actual.URL);
            Assert.Equal(expected.ExpiryDate, actual.ExpiryDate);
        }

        internal static void AssertEqual(CreateDocumentInternalDTO expected, DocumentDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.DocumentTypeId.ToString(), actual.TypeId);
            Assert.Equal(expected.DocumentDate, actual.Date);
            Assert.Equal(expected.Reference, actual.Reference);
            Assert.Equal(expected.URL, actual.URL);
            Assert.Equal(expected.ExpiryDate, actual.ExpiryDate);
        }

        internal static void AssertNotEqual(Document expected, Document actual)
        {
            Assert.NotNull(actual);
            Assert.NotEqual(expected.Id, actual.Id);
            Assert.NotEqual(expected.DocumentTypeId, actual.DocumentTypeId);
            DocumentTypeResources.AssertNotEqual(expected.DocumentType, actual.DocumentType);
            Assert.NotEqual(expected.FiledDate, actual.FiledDate);
            Assert.NotEqual(expected.DocumentDate, actual.DocumentDate);
            Assert.NotEqual(expected.DocumentRef, actual.DocumentRef);
            Assert.NotEqual(expected.DocumentURL, actual.DocumentURL);
            Assert.NotEqual(expected.ExpiryDate, actual.ExpiryDate);
        }

        internal static void AssertNotEqual(Document expected, CreateDocumentInternalDTO actual)
        {
            Assert.NotNull(actual);
            Assert.NotEqual(expected.DocumentType.Id, actual.DocumentTypeId);
            Assert.NotEqual(expected.DocumentDate, actual.DocumentDate);
            Assert.NotEqual(expected.DocumentRef, actual.Reference);
            Assert.NotEqual(expected.DocumentURL, actual.URL);
            Assert.NotEqual(expected.ExpiryDate, actual.ExpiryDate);
        }
    }
}
