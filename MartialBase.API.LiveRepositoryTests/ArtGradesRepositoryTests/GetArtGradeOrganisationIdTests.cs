// <copyright file="GetArtGradeOrganisationIdTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.ArtGradesRepositoryTests
{
    public class GetArtGradeOrganisationIdTests : BaseTestClass
    {
        [Test]
        public async Task CanGetArtGradeOrganisationId()
        {
            // Arrange
            ArtGrade testArtGrade = ArtGradeResources.CreateTestArtGrade(DbIdentifier);

            Assert.NotNull(testArtGrade.OrganisationId);

            // Act
            // Assert
            Assert.AreEqual(
                testArtGrade.OrganisationId,
                await ArtGradesRepository.GetArtGradeOrganisationIdAsync(testArtGrade.Id));
        }
    }
}
