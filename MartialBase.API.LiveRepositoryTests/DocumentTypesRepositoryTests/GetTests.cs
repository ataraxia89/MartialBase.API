// <copyright file="GetTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.DocumentTypes;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.DocumentTypesRepositoryTests
{
    public class GetTests : BaseTestClass
    {
        [Test]
        public async Task CanGetDocumentType()
        {
            DocumentType testDocumentType = DocumentTypeResources.CreateTestDocumentType(DbIdentifier);

            DocumentTypeDTO retrievedDocumentType = await DocumentTypesRepository.GetAsync(testDocumentType.Id);

            DocumentTypeResources.AssertEqual(testDocumentType, retrievedDocumentType);
        }
    }
}
