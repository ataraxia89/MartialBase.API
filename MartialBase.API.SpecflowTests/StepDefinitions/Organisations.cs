// <copyright file="Organisations.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.SpecflowTests
// Copyright © Martialtech®. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;

namespace MartialBase.API.SpecflowTests.StepDefinitions
{
    [Binding]
    public sealed class Organisations : BaseStepDefinitions
    {
        public Organisations(FeatureContext featureContext, ScenarioContext scenarioContext)
            : base(featureContext, scenarioContext)
        {
        }

        [Given(@"the current user does not belong to any organisation")]
        public async Task GivenTheCurrentUserDoesNotBelongToAnyOrganisation()
        {
            List<OrganisationDTO> organisations = await OrganisationsController.GetOrganisationsAsync(HttpClient, null);

            Assert.Empty(organisations);
        }

        [When(@"the current user creates the organisation ""([^""]*)""")]
        public async Task WhenTheCurrentUserCreatesTheOrganisation(string organisationName)
        {
            CreateOrganisationDTO createOrganisation =
                DataGenerator.Organisations.GenerateCreateOrganisationDTOObject(true);

            createOrganisation.Name = organisationName;
            createOrganisation.Initials = new Regex("[^A-Z]").Replace(organisationName, string.Empty);

            HttpResponseModel response =
                await OrganisationsController.CreateOrganisationResponseAsync(HttpClient, createOrganisation);

            Assert.True(response.IsSuccess, UserMessage(response));

            ScenarioContext[$"CreateOrganisationDTO-{organisationName}"] = createOrganisation;
        }

        [When(@"the current user requests a list of all organisations")]
        public async Task WhenTheCurrentUserRequestsAListOfAllOrganisations()
        {
            ScenarioContext[$"CurrentUserOrganisations-{CurrentUser.Id}"] =
                await OrganisationsController.GetOrganisationsAsync(HttpClient, null);
        }

        [Then(@"the response contains the created organisation ""([^""]*)""")]
        public void ThenTheResponseContainsTheOrganisation(string organisationName)
        {
            OrganisationDTO? returnedOrganisation =
                ((List<OrganisationDTO>)ScenarioContext[$"CurrentUserOrganisations-{CurrentUser.Id}"]).FirstOrDefault(
                    o => o.Name == organisationName);

            Assert.NotNull(returnedOrganisation);

            OrganisationResources.AssertEqual(
                (CreateOrganisationDTO)ScenarioContext[$"CreateOrganisationDTO-{organisationName}"],
                returnedOrganisation);

            ScenarioContext[$"CreatedOrganisation-{organisationName}"] = returnedOrganisation;
        }

        [When(@"the current user creates a new person record for ""([^""]*)"" under organisation ""([^""]*)""")]
        public async Task WhenTheCurrentUserCreatesANewPersonRecordForUnderOrganisation(string personFullName, string organisationName)
        {
            string organisationId = ((OrganisationDTO)ScenarioContext[$"CreatedOrganisation-{organisationName}"]).Id;

            CreatePersonDTO createPerson = DataGenerator.People.GenerateCreatePersonDTO(realisticData: true);

            DataGenerator.People.SetPersonName(createPerson, personFullName);

            HttpResponseModel response = await PeopleController.CreatePersonResponseAsync(
                HttpClient,
                createPerson,
                organisationId,
                null);

            Assert.True(response.IsSuccess, UserMessage(response));

            ScenarioContext[$"CreatePersonDTO-{personFullName}"] = createPerson;
        }

        [When(@"the current user requests a list of organisation people for ""([^""]*)""")]
        public async Task WhenTheCurrentUserRequestsAListOfOrganisationPeopleFor(string organisationName)
        {
            string organisationId = ((OrganisationDTO)ScenarioContext[$"CreatedOrganisation-{organisationName}"]).Id;

            ScenarioContext["RequestedOrganisationPeople"] =
                await OrganisationsController.GetOrganisationPeopleAsync(HttpClient, organisationId);
        }

        [Then(@"the created person ""([^""]*)"" is in the list of requested organisation people")]
        public void ThenTheResponseContainsCreatedPersonInTheListOfOrganisationPeople(string personFullName)
        {
            OrganisationPersonDTO? returnedOrganisationPerson =
                ((List<OrganisationPersonDTO>)ScenarioContext["RequestedOrganisationPeople"]).FirstOrDefault(
                    op => op.Person.FullName.Replace(op.Person.Title, string.Empty).Trim() == personFullName);

            Assert.NotNull(returnedOrganisationPerson);

            PersonResources.AssertEqualRestrictedDTO(
                (CreatePersonDTO)ScenarioContext[$"CreatePersonDTO-{personFullName}"],
                returnedOrganisationPerson.Person,
                false);
        }
    }
}
