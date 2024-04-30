// <copyright file="BasicDockerTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ContainerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.ContainerTests.TestTools;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.TestResources;

namespace MartialBase.API.ContainerTests
{
    public class BasicDockerTests : IClassFixture<ContainerTestFixture>
    {
        private readonly ContainerTestFixture _fixture;

        public BasicDockerTests(ContainerTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CanCreateAndRetrieveOrganisation()
        {
            // Prepare resources
            var createDTO = DataGenerator.Organisations.GenerateCreateOrganisationDTOObject();
            var createParentDTO = DataGenerator.Organisations.GenerateCreateOrganisationDTOObject();

            // Create the organisation
            var createdOrganisation = await OrganisationsController.CreateOrganisationAsync(
                _fixture.Client,
                createDTO);

            Assert.NotNull(createdOrganisation);
            OrganisationResources.AssertEqual(createDTO, createdOrganisation);

            // Attempt to retrieve the created organisation
            var retrievedOrganisation =
                await OrganisationsController.GetOrganisationAsync(_fixture.Client, createdOrganisation.Id);

            Assert.NotNull(retrievedOrganisation);
            OrganisationResources.AssertEqual(createDTO, retrievedOrganisation);

            // Create an organisation parent
            var createdParentOrganisation = await OrganisationsController.CreateOrganisationAsync(
                _fixture.Client,
                createParentDTO);

            Assert.NotNull(createdParentOrganisation);
            OrganisationResources.AssertEqual(createParentDTO, createdParentOrganisation);

            // Assign the child to the parent
            var addParentResponse = await OrganisationsController.ChangeOrganisationParentResponseAsync(
                _fixture.Client,
                createdOrganisation.Id,
                createdParentOrganisation.Id);

            Assert.True(addParentResponse.IsSuccess);
        }
    }
}
