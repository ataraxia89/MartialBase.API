// <copyright file="GetDocumentTypeOrganisationIdTests.cs" company="Martialtech®">
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
    public class GetDocumentTypeOrganisationIdTests : BaseTestClass
    {
        [Test]
        public async Task CanGetDocumentTypeOrganisationId()
        {
            DocumentType testDocumentType = DocumentTypeResources.CreateTestDocumentType(DbIdentifier);

            Assert.NotNull(testDocumentType.OrganisationId);

            Assert.AreEqual(
                testDocumentType.OrganisationId,
                await DocumentTypesRepository.GetDocumentTypeOrganisationIdAsync(testDocumentType.Id));
        }
    }
}
