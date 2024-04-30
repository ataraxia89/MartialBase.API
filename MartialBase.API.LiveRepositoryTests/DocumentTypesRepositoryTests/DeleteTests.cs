// <copyright file="DeleteTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.DocumentTypesRepositoryTests
{
    public class DeleteTests : BaseTestClass
    {
        [Test]
        public async Task CanDeleteDocumentType()
        {
            DocumentType testDocumentType = DocumentTypeResources.CreateTestDocumentType(DbIdentifier);

            await DocumentTypesRepository.DeleteAsync(testDocumentType.Id);

            Assert.IsTrue(await DocumentTypesRepository.SaveChangesAsync());

            Assert.IsFalse(DocumentTypeResources.CheckExists(testDocumentType.Id, DbIdentifier));
        }
    }
}
