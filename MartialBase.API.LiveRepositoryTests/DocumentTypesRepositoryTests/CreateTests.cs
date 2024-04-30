// <copyright file="CreateTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.DocumentTypes;
using MartialBase.API.Models.DTOs.DocumentTypes;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.DocumentTypesRepositoryTests
{
    public class CreateTests : BaseTestClass
    {
        [Test]
        public async Task CanCreateDocumentType()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            CreateDocumentTypeInternalDTO createDocumentTypeDTO =
                DataGenerator.DocumentTypes.GenerateCreateDocumentTypeInternalDTOObject(testOrganisation.Id);

            DocumentTypeDTO createdDocumentType = await DocumentTypesRepository.CreateAsync(createDocumentTypeDTO);

            Assert.IsTrue(await DocumentTypesRepository.SaveChangesAsync());

            DocumentTypeResources.AssertEqual(createDocumentTypeDTO, createdDocumentType);
            DocumentTypeResources.AssertExists(createdDocumentType, DbIdentifier);
        }
    }
}
