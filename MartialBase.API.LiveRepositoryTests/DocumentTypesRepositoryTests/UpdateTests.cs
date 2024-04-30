// <copyright file="UpdateTests.cs" company="Martialtech®">
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
    public class UpdateTests : BaseTestClass
    {
        [Test]
        public async Task CanUpdateDocumentType()
        {
            DocumentType testDocumentType = DocumentTypeResources.CreateTestDocumentType(DbIdentifier);
            UpdateDocumentTypeDTO updateDocumentTypeDTO =
                DataGenerator.DocumentTypes.GenerateUpdateDocumentTypeDTOObject();

            DocumentTypeResources.AssertNotEqual(testDocumentType, updateDocumentTypeDTO);

            DocumentTypeDTO updatedDocumentType =
                await DocumentTypesRepository.UpdateAsync(testDocumentType.Id, updateDocumentTypeDTO);

            Assert.IsTrue(await DocumentTypesRepository.SaveChangesAsync());

            DocumentTypeResources.AssertEqual(updateDocumentTypeDTO, updatedDocumentType);
            DocumentTypeResources.AssertExists(updatedDocumentType, DbIdentifier);
        }
    }
}
