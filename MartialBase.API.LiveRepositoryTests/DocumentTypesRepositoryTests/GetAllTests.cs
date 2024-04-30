// <copyright file="GetAllTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.DocumentTypes;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.DocumentTypesRepositoryTests
{
    public class GetAllTests : BaseTestClass
    {
        [Test]
        public async Task CanGetDocumentTypes()
        {
            List<DocumentType> testDocumentTypes =
                DocumentTypeResources.CreateTestDocumentTypes(10, DbIdentifier);

            List<DocumentTypeDTO> retrievedDocumentTypes = await DocumentTypesRepository.GetAllAsync();

            DocumentTypeResources.AssertEqual(testDocumentTypes, retrievedDocumentTypes);
        }
    }
}
