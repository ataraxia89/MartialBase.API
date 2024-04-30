// <copyright file="GetSchoolsTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.LiveControllerTests.TestTools;
using MartialBase.API.Models.DTOs.Schools;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.TestResources;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.SchoolsControllerTests
{
    [Collection("LiveControllerTests")]
    public class GetSchoolsTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSchoolsTests"/> class.
        /// </summary>
        /// <remarks>In xUnit, the constructor is called before each test in the current class.</remarks>
        /// <param name="fixture">The <see cref="TestServerFixture"/> instance.</param>
        public GetSchoolsTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
        }

        private Guid TestUserId => _fixture.TestUser.Id;

        private Guid TestUserPersonId => _fixture.TestUser.PersonId;

        private string DbIdentifier => _fixture.DbIdentifier;

        [Theory]
        [MemberData(nameof(UserRoleResources.SchoolMemberRoleNames), MemberType = typeof(UserRoleResources))]
        public async Task SchoolMemberCanRetrieveSchoolsTheyAreAMemberOf(string roleName)
        {
            // Arrange
            List<School> testMemberSchools = SchoolResources.CreateTestSchools(
                10, DbIdentifier);

            List<School> testNonMemberSchools = SchoolResources.CreateTestSchools(
                10, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsHaveStudent(
                testMemberSchools.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            SchoolStudentResources.EnsureSchoolsDoNotHaveStudent(
                testNonMemberSchools.Select(o => o.Id).ToList(), TestUserPersonId, DbIdentifier);

            MartialBaseUserRoleResources.EnsureUserHasRole(TestUserId, roleName, DbIdentifier);

            // Act
            List<SchoolDTO> retrievedSchools =
                await SchoolsController.GetSchoolsAsync(_fixture.Client, null, null);

            // Assert
            SchoolResources.AssertEqual(testMemberSchools, retrievedSchools);

            foreach (School school in testNonMemberSchools)
            {
                Assert.Null(retrievedSchools.FirstOrDefault(o =>
                    o.Id == school.Id));
            }
        }
    }
}
