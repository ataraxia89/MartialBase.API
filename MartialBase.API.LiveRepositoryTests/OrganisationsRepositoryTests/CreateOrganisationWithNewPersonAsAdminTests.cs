// <copyright file="CreateOrganisationWithNewPersonAsAdminTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Data.Models.InternalDTOs.Organisations;
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.OrganisationsRepositoryTests
{
    public class CreateOrganisationWithNewPersonAsAdminTests : BaseTestClass
    {
        [Test]
        public async Task CanCreateOrganisationWithNewPersonAsAdmin()
        {
            CreateOrganisationInternalDTO createOrganisationInternalDTO =
                DataGenerator.Organisations.GenerateCreateOrganisationInternalDTOObject();

            CreatePersonInternalDTO createPersonInternalDTO =
                DataGenerator.People.GenerateCreatePersonInternalDTO();

            PersonOrganisationDTO createdPersonOrganisation =
                await OrganisationsRepository.CreateOrganisationWithNewPersonAsAdminAsync(
                    createOrganisationInternalDTO,
                    createPersonInternalDTO,
                    Guid.NewGuid());

            Assert.IsTrue(await OrganisationsRepository.SaveChangesAsync());

            OrganisationResources.AssertEqual(
                createOrganisationInternalDTO,
                createdPersonOrganisation.Organisation);
            OrganisationResources.AssertExists(createdPersonOrganisation.Organisation, DbIdentifier);

            PersonResources.AssertEqual(createPersonInternalDTO, createdPersonOrganisation.Person);
            PersonResources.AssertExists(createdPersonOrganisation.Person, DbIdentifier);

            Assert.IsTrue(createdPersonOrganisation.IsAdmin);
        }
    }
}
