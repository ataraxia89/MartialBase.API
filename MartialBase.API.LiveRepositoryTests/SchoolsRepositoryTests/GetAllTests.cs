// <copyright file="GetAllTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Schools;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.SchoolsRepositoryTests
{
    public class GetAllTests : BaseTestClass
    {
        [Test]
        public async Task CanGetAllSchools()
        {
            List<School> testSchools = SchoolResources.CreateTestSchools(10, DbIdentifier);

            List<SchoolDTO> retrievedSchools = await SchoolsRepository.GetAllAsync();

            SchoolResources.AssertEqual(testSchools, retrievedSchools);
        }

        [Test]
        public async Task CanGetSchoolsWithinOrganisation()
        {
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<School> testSchools = SchoolResources.CreateTestSchools(10, DbIdentifier);

            SchoolResources.EnsureSchoolsBelongToOrganisation(testSchools, testOrganisation, DbIdentifier);

            List<SchoolDTO> retrievedSchools = await SchoolsRepository.GetAllAsync(null, testOrganisation.Id);

            SchoolResources.AssertEqual(testSchools, retrievedSchools);
        }

        [Test]
        public async Task CanGetSchoolsWithinArt()
        {
            Art testArt = ArtResources.CreateTestArt(DbIdentifier);
            List<School> testSchools = SchoolResources.CreateTestSchools(10, DbIdentifier);

            SchoolResources.EnsureSchoolsBelongToArt(testSchools, testArt, DbIdentifier);

            List<SchoolDTO> retrievedSchools =
                await SchoolsRepository.GetAllAsync(testArt.Id, null);

            SchoolResources.AssertEqual(testSchools, retrievedSchools);
        }

        [Test]
        public async Task CanGetSchoolsWithinArtAndOrganisation()
        {
            Art testArt = ArtResources.CreateTestArt(DbIdentifier);
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<School> testSchools = SchoolResources.CreateTestSchools(10, DbIdentifier);

            SchoolResources.EnsureSchoolsBelongToArt(testSchools, testArt, DbIdentifier);
            SchoolResources.EnsureSchoolsBelongToOrganisation(testSchools, testOrganisation, DbIdentifier);

            Art otherArt = ArtResources.CreateTestArt(DbIdentifier);
            Organisation otherOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);
            List<School> otherSchools = SchoolResources.CreateTestSchools(10, DbIdentifier);

            SchoolResources.EnsureSchoolsBelongToArt(otherSchools, otherArt, DbIdentifier);
            SchoolResources.EnsureSchoolsBelongToOrganisation(otherSchools, otherOrganisation, DbIdentifier);

            List<SchoolDTO> retrievedSchools = await SchoolsRepository.GetAllAsync(testArt.Id, testOrganisation.Id);

            SchoolResources.AssertEqual(testSchools, retrievedSchools);
        }
    }
}
