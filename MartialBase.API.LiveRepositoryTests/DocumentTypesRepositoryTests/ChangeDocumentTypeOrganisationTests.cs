// <copyright file="ChangeDocumentTypeOrganisationTests.cs" company="Martialtech®">
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
    public class ChangeDocumentTypeOrganisationTests : BaseTestClass
    {
        [Test]
        public async Task CanChangeDocumentTypeOrganisation()
        {
            DocumentType testDocumentType = DocumentTypeResources.CreateTestDocumentType(DbIdentifier);
            Organisation newOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            Assert.AreNotEqual(testDocumentType.Organisation.Id, newOrganisation.Id);

            await DocumentTypesRepository.ChangeDocumentTypeOrganisationAsync(testDocumentType.Id, newOrganisation.Id);

            Assert.IsTrue(await DocumentTypesRepository.SaveChangesAsync());

            DocumentType checkDocumentType =
                DocumentTypeResources.GetDocumentType(testDocumentType.Id, DbIdentifier);

            Assert.AreEqual(newOrganisation.Id, checkDocumentType.Organisation.Id);
        }
    }
}
