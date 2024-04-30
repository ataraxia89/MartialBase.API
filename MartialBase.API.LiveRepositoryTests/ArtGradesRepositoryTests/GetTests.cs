// <copyright file="GetTests.cs" company="Martialtech®">
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
    public class GetTests : BaseTestClass
    {
        [Test]
        public async Task CanGetArtGrade()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            // Act
            ArtGradeDTO retrievedArtGrade = await ArtGradesRepository.GetAsync(testArtGrade.Id);

            // Assert
            ArtGradeResources.AssertEqual(testArtGrade, retrievedArtGrade);
        }
    }
}
