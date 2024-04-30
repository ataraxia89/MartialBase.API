// <copyright file="GetAllTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.ArtGrades;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.ArtGradesRepositoryTests
{
    public class GetAllTests : BaseTestClass
    {
        [Test]
        public async Task GetAllArtGradesWithoutParametersThrowsNotImplementedException()
        {
            // Arrange
            NotImplementedException expectedException = null;

            // Act
            try
            {
                await ArtGradesRepository.GetAllAsync();
            }
            catch (NotImplementedException ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.NotNull(expectedException);
        }

        [Test]
        public async Task GetAllArtGradesWithInvalidArtIdThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            // Act
            try
            {
                await ArtGradesRepository.GetAllAsync(Guid.NewGuid(), testOrganisationId);
            }
            catch (InvalidOperationException ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.NotNull(expectedException);
        }

        [Test]
        public async Task GetAllArtGradesWithInvalidOrganisationIdThrowsInvalidOperationException()
        {
            // Arrange
            InvalidOperationException expectedException = null;
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;

            // Act
            try
            {
                await ArtGradesRepository.GetAllAsync(testArtId, Guid.NewGuid());
            }
            catch (InvalidOperationException ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.NotNull(expectedException);
        }

        [Test]
        public async Task CanGetAllArtGradesForArtAndOrganisation()
        {
            // Arrange
            Guid testArtId = ArtResources.CreateTestArt(DbIdentifier).Id;
            Guid testOrganisationId = OrganisationResources.CreateTestOrganisation(DbIdentifier).Id;

            List<ArtGrade> testArtGrades =
                ArtGradeResources.CreateTestArtGrades(10, DbIdentifier, testArtId, testOrganisationId);

            // Act
            List<ArtGradeDTO> retrievedArtGrades =
                await ArtGradesRepository.GetAllAsync(testArtId, testOrganisationId);

            // Assert
            ArtGradeResources.AssertEqual(testArtGrades, retrievedArtGrades);
        }
    }
}
