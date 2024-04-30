// <copyright file="CreateTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.InternalDTOs.Organisations;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.OrganisationsRepositoryTests
{
    public class CreateTests : BaseTestClass
    {
        [Test]
        public async Task CanCreateOrganisation()
        {
            CreateOrganisationInternalDTO createOrganisationInternalDTO =
                DataGenerator.Organisations.GenerateCreateOrganisationInternalDTOObject();

            OrganisationDTO createdOrganisation = await OrganisationsRepository.CreateAsync(createOrganisationInternalDTO);

            Assert.IsTrue(await OrganisationsRepository.SaveChangesAsync());

            OrganisationResources.AssertEqual(createOrganisationInternalDTO, createdOrganisation);
            OrganisationResources.AssertExists(createdOrganisation, DbIdentifier);
        }
    }
}
