// <copyright file="PersonDocumentResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data;
using MartialBase.API.Data.Models.EntityFramework;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    /// <summary>
    /// A test helper class used to create and manage <see cref="PersonDocument"/> objects on the database.
    /// </summary>
    internal static class PersonDocumentResources
    {
        /// <summary>
        /// Checks if a specified <see cref="Person"/> has multiple specified <see cref="Document">Documents</see>
        /// assigned to them on the database and adds the relevant <see cref="PersonDocument"/> objects if not.
        /// </summary>
        /// <param name="personId">The ID of the <see cref="Person"/> to whom the specified <see cref="Document">Documents</see> should be assigned.</param>
        /// <param name="documentIds">The <see cref="Document">Documents</see> which need to be assigned to the specified <see cref="Person"/>.</param>
        /// <param name="dbIdentifier">The string identifier of the <see cref="MartialBaseDbContext"/> to be used.</param>
        internal static void EnsurePersonHasDocuments(Guid personId, List<Guid> documentIds, string dbIdentifier)
        {
            foreach (Guid documentId in documentIds)
            {
                EnsurePersonHasDocument(personId, documentId, dbIdentifier);
            }
        }

        /// <summary>
        /// Checks if a specified <see cref="Person"/> has a specified <see cref="Document"/> assigned to them on
        /// the database and adds the relevant <see cref="PersonDocument"/> object if not.
        /// </summary>
        /// <param name="personId">The ID of the <see cref="Person"/> to whom the specified <see cref="Document"/> should be assigned.</param>
        /// <param name="documentId">The ID of the <see cref="Document"/> which needs to be assigned to the specified <see cref="Person"/>.</param>
        /// <param name="dbIdentifier">The string identifier of the <see cref="MartialBaseDbContext"/> to be used.</param>
        internal static void EnsurePersonHasDocument(Guid personId, Guid documentId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                PersonDocument personDocument = dbContext.PersonDocuments
                    .FirstOrDefault(op =>
                        op.PersonId == personId &&
                        op.DocumentId == documentId);

                if (personDocument == null)
                {
                    dbContext.PersonDocuments.Add(new PersonDocument
                    {
                        Id = Guid.NewGuid(),
                        PersonId = personId,
                        DocumentId = documentId,
                        IsActive = true
                    });

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.PersonDocuments.Any(pd =>
                    pd.PersonId == personId &&
                    pd.DocumentId == documentId));
            }
        }

        /// <summary>
        /// Retrieve's a list of <see cref="PersonDocument">PersonDocuments</see> and ensures their active
        /// status matches that provided.
        /// </summary>
        /// <param name="personId">The ID of the <see cref="Person"/> to whom the document belongs.</param>
        /// <param name="documentIds">The IDs of the <see cref="Document">Documents</see> to be checked.</param>
        /// <param name="isActive">The value of the <see cref="Document"/>'s active status to be set.</param>
        /// <param name="dbIdentifier">The string identifier of the <see cref="MartialBaseDbContext"/> to be used.</param>
        internal static void EnsurePersonDocumentsActiveStatus(Guid personId, List<Guid> documentIds, bool isActive, string dbIdentifier)
        {
            foreach (Guid documentId in documentIds)
            {
                EnsurePersonDocumentActiveStatus(personId, documentId, isActive, dbIdentifier);
            }
        }

        /// <summary>
        /// Retrieve's a <see cref="PersonDocument"/> and ensures it's active status matches that provided.
        /// </summary>
        /// <param name="personId">The ID of the <see cref="Person"/> to whom the document belongs.</param>
        /// <param name="documentId">The ID of the <see cref="Document"/> to be checked.</param>
        /// <param name="isActive">The value of the <see cref="Document"/>'s active status to be set.</param>
        /// <param name="dbIdentifier">The string identifier of the <see cref="MartialBaseDbContext"/> to be used.</param>
        internal static void EnsurePersonDocumentActiveStatus(Guid personId, Guid documentId, bool isActive, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                PersonDocument personDocument =
                    dbContext.PersonDocuments.First(pd =>
                        pd.PersonId == personId &&
                        pd.DocumentId == documentId);

                personDocument.IsActive = isActive;

                Assert.True(dbContext.SaveChanges() >= 0);
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                PersonDocument personDocument =
                    dbContext.PersonDocuments.First(pd =>
                        pd.PersonId == personId &&
                        pd.DocumentId == documentId);

                Assert.Equal(isActive, personDocument.IsActive);
            }
        }

        /// <summary>
        /// Asserts that a <see cref="PersonDocument"/> matching the provided <paramref name="personId"/> and
        /// <paramref name="documentId"/> exists on the database.
        /// </summary>
        /// <param name="personId">The ID of the <see cref="Person"/> to be checked.</param>
        /// <param name="documentId">The ID of the <see cref="Document"/> to be checked.</param>
        /// <param name="dbIdentifier">The string identifier of the <see cref="MartialBaseDbContext"/> to be used.</param>
        internal static void AssertExists(Guid personId, Guid documentId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.PersonDocuments.Any(pd =>
                    pd.PersonId == personId &&
                    pd.DocumentId == documentId));
            }
        }
    }
}
