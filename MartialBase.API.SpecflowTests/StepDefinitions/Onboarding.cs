// <copyright file="Onboarding.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.SpecflowTests
// Copyright © Martialtech®. All rights reserved.
// </copyright>

using System.Text.Json;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.DTOs.Schools;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.Models;
using MartialBase.API.TestTools.TestResources;
using MartialBase.API.Tools;

using Microsoft.Extensions.Configuration;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.SpecflowTests.StepDefinitions
{
    [Binding]
    public sealed class Onboarding : BaseStepDefinitions
    {
        public Onboarding(FeatureContext featureContext, ScenarioContext scenarioContext)
            : base(featureContext, scenarioContext)
        {
        }

        [Given("there is an existing school on system called \"([^\"]*)\"")]
        public void GivenThereIsAnExistingSchoolOnSystemCalled(string schoolName)
        {
            School school = DataGenerator.Schools.GenerateSchoolObject(true);

            school.Name = schoolName;

            SchoolResources.CreateSchool(school, DbIdentifier);

            ScenarioContext[$"ExistingSchool-{schoolName}"] = school;
        }

        [Given(@"there is an existing person on system called ""([^""]*)""")]
        public async Task GivenThereIsAnExistingPersonOnSystemCalled(string personName)
        {
            Person person = DataGenerator.People.GeneratePersonObject(realisticData: true);

            DataGenerator.People.SetPersonName(person, personName);

            await PersonResources.CreatePersonAsync(
                person,
                DbIdentifier);

            ScenarioContext[$"ExistingPerson-{personName}"] = person;
        }

        [When(@"existing person ""([^""]*)"" is added to existing school ""([^""]*)""")]
        public async Task WhenExistingPersonIsAddedToExistingSchool(string studentName, string schoolName)
        {
            HttpResponseModel response = await SchoolsController.AddStudentToSchoolResponseAsync(
                HttpClient,
                ((School)ScenarioContext[$"ExistingSchool-{schoolName}"]).Id.ToString(),
                ((Person)ScenarioContext[$"ExistingPerson-{studentName}"]).Id.ToString(),
                "false",
                "false");

            Assert.True(response.IsSuccess, UserMessage(response));
        }

        [When(@"a new person ""([^""]*)"" is created under existing school ""([^""]*)""")]
        public async Task WhenANewPersonIsCreatedUnderExistingSchool(string personFullName, string schoolName)
        {
            CreatePersonDTO createPersonDTO = DataGenerator.People.GenerateCreatePersonDTO(realisticData: true);

            DataGenerator.People.SetPersonName(createPersonDTO, personFullName);

            HttpResponseModel response = await PeopleController.CreatePersonResponseAsync(
                HttpClient,
                createPersonDTO,
                null,
                ((School)ScenarioContext[$"ExistingSchool-{schoolName}"]).Id.ToString());

            Assert.True(response.IsSuccess, UserMessage(response));

            CreatedPersonDTO? createdPerson = JsonSerializer.Deserialize<CreatedPersonDTO>(
                response.ResponseBody,
                JsonSerializerOptions);

            PersonResources.AssertEqual(createPersonDTO, createdPerson);

            ScenarioContext[$"CreatedPerson-{personFullName}"] = createdPerson;
        }

        [When(@"the current user's token contains the generated invitation code for ""([^""]*)"" and a new Azure ID")]
        public void WhenTheCurrentUsersTokenContainsTheGeneratedInvitationCodeFor(string personFullName)
        {
            MartialBaseUser user = MartialBaseUserResources.GetUserFromPersonId(
                ((CreatedPersonDTO)ScenarioContext[$"CreatedPerson-{personFullName}"]).Id,
                DbIdentifier);

            string authToken = AuthTokens.GenerateAuthorizationToken(
                APIConfiguration,
                Guid.NewGuid(),
                user.InvitationCode);

            SetCurrentUser(user, authToken);
        }

        [When(@"the current user requests a list of all schools")]
        public async Task WhenTheCurrentUserRequestsAListOfAllSchools()
        {
            var response = await SchoolsController.GetSchoolsResponseAsync(HttpClient, null, null);

            Assert.True(response.IsSuccess, UserMessage(response));

            ScenarioContext["RequestedSchools"] = JsonSerializer.Deserialize<List<SchoolDTO>>(
                response.ResponseBody,
                JsonSerializerOptions);
        }

        [Then(@"student ""([^""]*)"" is returned in the list of school students of ""([^""]*)""")]
        public async Task ThenStudentIsReturnedInTheListOfSchoolStudentsOf(string studentName, string schoolName)
        {
            List<SchoolStudentDTO> schoolStudents = await SchoolsController.GetSchoolStudentsAsync(
                HttpClient,
                ((School)ScenarioContext[$"ExistingSchool-{schoolName}"]).Id.ToString());

            Assert.Contains(
                ((Person)ScenarioContext[$"ExistingPerson-{studentName}"]).Id.ToString(),
                schoolStudents.Select(ss => ss.Student.Id.ToString()));
        }

        [Then(@"the created person ""([^""]*)"" has an invitation code")]
        public void ThenTheNewPersonHasAnInvitationCode(string personFullName)
        {
            Assert.NotNull(((CreatedPersonDTO)ScenarioContext[$"CreatedPerson-{personFullName}"]).InvitationCode);
        }

        [Then(@"the school ""([^""]*)"" is in the list of requested schools")]
        public void ThenTheSchoolInTheListOfRequestedSchools(string schoolName)
        {
            SchoolDTO? returnedSchool =
                ((List<SchoolDTO>)ScenarioContext["RequestedSchools"])
                .FirstOrDefault(s => s.Name == schoolName);

            Assert.NotNull(returnedSchool);

            SchoolResources.AssertEqual((School)ScenarioContext[$"ExistingSchool-{schoolName}"], returnedSchool);
        }
    }
}
