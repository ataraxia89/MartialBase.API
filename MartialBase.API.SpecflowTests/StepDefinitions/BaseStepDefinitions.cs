// <copyright file="BaseStepDefinitions.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.SpecflowTests
// Copyright © Martialtech®. All rights reserved.
// </copyright>

using System.Text.Json;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.Models;

using Microsoft.Extensions.Configuration;

namespace MartialBase.API.SpecflowTests.StepDefinitions
{
    public class BaseStepDefinitions
    {
        public BaseStepDefinitions(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            FeatureContext = featureContext;
            ScenarioContext = scenarioContext;
        }

        public static JsonSerializerOptions JsonSerializerOptions => new () { PropertyNameCaseInsensitive = true };

        public FeatureContext FeatureContext { get; }

        public ScenarioContext ScenarioContext { get; }

        public HttpClient HttpClient =>
            (HttpClient)ScenarioContext[GlobalSetupAndTearDown.HttpClient];

        public IConfiguration APIConfiguration =>
            (IConfiguration)FeatureContext[GlobalSetupAndTearDown.Configuration];

        public MartialBaseUser CurrentUser =>
            (MartialBaseUser)ScenarioContext[GlobalSetupAndTearDown.CurrentUser];

        public string DbIdentifier =>
            (string)FeatureContext[GlobalSetupAndTearDown.DbIdentifier];

        public static string UserMessage(HttpResponseModel response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            string responseCodeText = response.ErrorResponseCode == ErrorResponseCode.None
                ? "no error response code"
                : $"response code {response.ErrorResponseCode}";

            string responseBodyText = string.IsNullOrEmpty(response.ResponseBody)
                ? "no response body"
                : $"response body '{response.ResponseBody}'";

            return
                $"Request failed with status code {response.StatusCode}, {responseCodeText} and {responseBodyText}";
        }

        public void SetCurrentUser(MartialBaseUser user, string token)
        {
            var newClient = new HttpClient { BaseAddress = HttpClient.BaseAddress };

            newClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            FeatureContext[GlobalSetupAndTearDown.HttpClient] = newClient;
            FeatureContext[GlobalSetupAndTearDown.CurrentUser] = user;
        }
    }
}
