// <copyright file="CreateTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Schools;
using MartialBase.API.Models.DTOs.Schools;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.SchoolsRepositoryTests
{
    public class CreateTests : BaseTestClass
    {
        [Test]
        public async Task CanCreateSchool()
        {
            Art testArt = ArtResources.CreateTestArt(DbIdentifier);
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            Person testHeadInstructor = PersonResources.CreateTestPerson(DbIdentifier, false);

            CreateSchoolInternalDTO createSchoolInternalDTO = DataGenerator.Schools.GenerateCreateSchoolInternalDTO(
                testArt.Id,
                testOrganisation.Id,
                testHeadInstructor.Id);

            SchoolDTO createdSchool = await SchoolsRepository.CreateAsync(createSchoolInternalDTO);

            Assert.IsTrue(await SchoolsRepository.SaveChangesAsync());

            SchoolResources.AssertEqual(createSchoolInternalDTO, createdSchool);
            SchoolResources.AssertExists(createdSchool, DbIdentifier);
        }
    }
}
