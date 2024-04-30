// <copyright file="DocumentTypeResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.DocumentTypes;
using MartialBase.API.Models.DTOs.DocumentTypes;
using MartialBase.API.Tools;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class DocumentTypeResources
    {
        internal static bool CheckExists(Guid documentTypeId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.DocumentTypes.Any(dt => dt.Id == documentTypeId);
            }
        }

        internal static void AssertExists(DocumentTypeDTO documentType, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                DocumentType checkDocumentType = dbContext.DocumentTypes
                    .Include(dt => dt.Organisation)
                    .FirstOrDefault(dt => dt.Id == new Guid(documentType.Id));
                AssertEqual(documentType, checkDocumentType);
            }
        }

        internal static void EnsureOrganisationHasDocumentTypes(Organisation organisation, List<DocumentType> documentTypes, string dbIdentifier, bool updateInMemoryObjects = true)
        {
            foreach (DocumentType documentType in documentTypes)
            {
                EnsureOrganisationHasDocumentType(organisation, documentType, dbIdentifier, updateInMemoryObjects);
            }
        }

        internal static void EnsureOrganisationHasDocumentType(Organisation organisation, DocumentType documentType, string dbIdentifier, bool updateInMemoryObject = true)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                DocumentType dbDocumentType = dbContext.DocumentTypes
                    .First(dt => dt.Id == documentType.Id);

                if (dbDocumentType.OrganisationId != organisation.Id)
                {
                    dbDocumentType.OrganisationId = organisation.Id;

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.DocumentTypes.Any(dt =>
                    dt.Id == documentType.Id &&
                    dt.OrganisationId == organisation.Id));
            }

            if (updateInMemoryObject)
            {
                documentType.OrganisationId = organisation.Id;
                documentType.Organisation = organisation;
            }
        }

        internal static DocumentType CreateTestDocumentType(string dbIdentifier, bool realisticData = false) => CreateTestDocumentTypes(1, dbIdentifier, realisticData).First();

        internal static List<DocumentType> CreateTestDocumentTypes(int numberToCreate, string dbIdentifier, bool realisticData = false)
        {
            if (numberToCreate <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberToCreate),
                    numberToCreate,
                    $"Cannot create {numberToCreate} test document types.");
            }

            var createdDocumentTypes = new List<DocumentType>();

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                for (int i = 0; i < numberToCreate; i++)
                {
                    DocumentType documentType = DataGenerator.DocumentTypes.GenerateDocumentTypeObject(realisticData);

                    Assert.False(dbContext.DocumentTypes.Any(dt => dt.Id == documentType.Id));

                    dbContext.DocumentTypes.Add(documentType);
                    createdDocumentTypes.Add(documentType);
                }

                Assert.True(dbContext.SaveChanges() > 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                foreach (DocumentType createdDocumentType in createdDocumentTypes)
                {
                    DocumentType checkDocumentType = dbContext.DocumentTypes
                            .Include(dt => dt.Organisation)
                            .ThenInclude(o => o.Address)
                            .Include(dt => dt.Organisation.Parent)
                            .ThenInclude(op => op.Address)
                            .FirstOrDefault(dt => dt.Id == createdDocumentType.Id);
                    AssertEqual(createdDocumentType, checkDocumentType);
                }
            }

            return createdDocumentTypes;
        }

        internal static DocumentType GetDocumentType(Guid documentTypeId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.DocumentTypes
                    .Include(dt => dt.Organisation)
                    .ThenInclude(o => o.Address)
                    .Include(dt => dt.Organisation.Parent)
                    .ThenInclude(o => o.Address)
                    .FirstOrDefault(dt => dt.Id == documentTypeId);
            }
        }

        internal static void AssertEqual(DocumentType expected, DocumentType actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.OrganisationId, actual.OrganisationId);
            OrganisationResources.AssertEqual(expected.Organisation, actual.Organisation);
            Assert.Equal(expected.ReferenceNo, actual.ReferenceNo);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.DefaultExpiryDays, actual.DefaultExpiryDays);
            Assert.Equal(expected.URL, actual.URL);
        }

        internal static void AssertEqual(DocumentTypeDTO expected, DocumentType actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(new Guid(expected.Id), actual.Id);
            Assert.Equal(new Guid(expected.OrganisationId), actual.OrganisationId);
            Assert.Equal(expected.Organisation, actual.Organisation.Initials);
            Assert.Equal(expected.ReferenceNo, actual.ReferenceNo);
            Assert.Equal(expected.DocumentExpiryDays, actual.DefaultExpiryDays);
            Assert.Equal(expected.URL, actual.URL);
        }

        internal static void AssertEqual(DocumentType expected, DocumentTypeDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id.ToString(), actual.Id);
            Assert.Equal(expected.OrganisationId.ToString(), actual.OrganisationId);
            Assert.Equal(expected.Organisation.Initials, actual.Organisation);
            Assert.Equal(expected.ReferenceNo, actual.ReferenceNo);
            Assert.Equal(expected.DefaultExpiryDays, actual.DocumentExpiryDays);
            Assert.Equal(expected.URL, actual.URL);
        }

        internal static void AssertEqual(DocumentTypeDTO expected, DocumentTypeDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.OrganisationId, actual.OrganisationId);
            Assert.Equal(expected.Organisation, actual.Organisation);
            Assert.Equal(expected.ReferenceNo, actual.ReferenceNo);
            Assert.Equal(expected.DocumentExpiryDays, actual.DocumentExpiryDays);
            Assert.Equal(expected.URL, actual.URL);
        }

        internal static void AssertEqual(CreateDocumentTypeInternalDTO expected, DocumentTypeDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.OrganisationId.ToString(), actual.OrganisationId);
            Assert.Equal(expected.ReferenceNo, actual.ReferenceNo);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.DefaultExpiryDays, actual.DocumentExpiryDays);
            Assert.Equal(expected.URL, actual.URL);
        }

        internal static void AssertEqual(UpdateDocumentTypeDTO expected, DocumentTypeDTO actual)
        {
            Assert.Equal(expected.ReferenceNo, actual.ReferenceNo);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.DefaultExpiryDays, actual.DocumentExpiryDays);
            Assert.Equal(expected.URL, actual.URL);
        }

        internal static void AssertEqual(List<DocumentType> expected, List<DocumentTypeDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (DocumentType expectedDocumentType in expected)
            {
                DocumentTypeDTO actualDocumentType = actual.FirstOrDefault(dt =>
                    dt.Id == expectedDocumentType.Id.ToString());

                AssertEqual(expectedDocumentType, actualDocumentType);
            }
        }

        internal static void AssertEqual(List<DocumentTypeDTO> expected, List<DocumentTypeDTO> actual)
        {
            foreach (DocumentTypeDTO expectedDocumentType in expected)
            {
                DocumentTypeDTO actualDocumentType = actual.FirstOrDefault(dt =>
                    dt.Id == expectedDocumentType.Id);

                AssertEqual(expectedDocumentType, actualDocumentType);
            }
        }

        internal static void AssertNotEqual(DocumentType expected, DocumentType actual)
        {
            Assert.NotNull(actual);
            Assert.NotEqual(expected.Id, actual.Id);
            Assert.NotEqual(expected.OrganisationId, actual.OrganisationId);
            OrganisationResources.AssertNotEqual(expected.Organisation, actual.Organisation);
            Assert.NotEqual(expected.ReferenceNo, actual.ReferenceNo);
            Assert.NotEqual(expected.Description, actual.Description);
            Assert.NotEqual(expected.DefaultExpiryDays, actual.DefaultExpiryDays);
            Assert.NotEqual(expected.URL, actual.URL);
        }

        internal static void AssertNotEqual(DocumentType expected, UpdateDocumentTypeDTO actual)
        {
            Assert.NotEqual(expected.ReferenceNo, actual.ReferenceNo);
            Assert.NotEqual(expected.Description, actual.Name);
            Assert.NotEqual(expected.DefaultExpiryDays, actual.DefaultExpiryDays);
            Assert.NotEqual(expected.URL, actual.URL);
        }
    }
}
