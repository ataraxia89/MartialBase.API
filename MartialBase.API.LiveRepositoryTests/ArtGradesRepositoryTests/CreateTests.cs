// <copyright file="CreateTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.ArtGrades;
using MartialBase.API.Models.DTOs.ArtGrades;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.ArtGradesRepositoryTests
{
    public class CreateTests : BaseTestClass
    {
        [Test]
        public async Task CanCreateArtGrade()
        {
            // Arrange
            Art testArt = ArtResources.CreateTestArt(DbIdentifier);
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            CreateArtGradeInternalDTO createArtGradeDTO =
                DataGenerator.ArtGrades.GenerateCreateArtGradeInternalDTOObject(testArt.Id, testOrganisation.Id);

            // Act
            ArtGradeDTO createdArtGrade = await ArtGradesRepository.CreateAsync(createArtGradeDTO);

            Assert.IsTrue(await ArtGradesRepository.SaveChangesAsync());

            // Assert
            ArtGradeResources.AssertEqual(createArtGradeDTO, createdArtGrade);
            ArtGradeResources.AssertExists(createdArtGrade, DbIdentifier);
        }
    }
}
