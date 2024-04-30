// <copyright file="UpdateTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.ArtGrades;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.ArtGradesRepositoryTests
{
    public class UpdateTests : BaseTestClass
    {
        [Test]
        public async Task CanUpdateArtGrade()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            UpdateArtGradeDTO updateArtGradeDTO = DataGenerator.ArtGrades.GenerateUpdateArtGradeDTOObject();

            ArtGradeResources.AssertNotEqual(testArtGrade, updateArtGradeDTO);

            // Act
            ArtGradeDTO updatedArtGrade = await ArtGradesRepository.UpdateAsync(testArtGrade.Id, updateArtGradeDTO);

            Assert.IsTrue(await ArtGradesRepository.SaveChangesAsync());

            // Assert
            ArtGradeResources.AssertEqual(updateArtGradeDTO, updatedArtGrade);
            ArtGradeResources.AssertExists(updatedArtGrade, DbIdentifier);
        }
    }
}
