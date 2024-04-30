// <copyright file="CreateUserTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.LiveControllerTests.TestTools;
using MartialBase.API.Models.DTOs.MartialBaseUsers;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.ControllerMethods;
using MartialBase.API.TestTools.TestResources;
using MartialBase.API.Tools;

using Xunit;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveControllerTests.MartialBaseUserControllerTests
{
#if DEBUG
    [Collection("LiveControllerTests")]
    public class CreateUserTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _fixture;

        public CreateUserTests(TestServerFixture fixture)
        {
            _fixture = fixture;

            _fixture.SetUpUnitTest();
            _fixture.ClearAuthorizationToken();
        }

        public static IEnumerable<object[]> InvalidCreateMartialBaseUserDTOs
        {
            get
            {
                object[] personIdTooLong =
                {
                    new CreateMartialBaseUserDTO
                    {
                        PersonId = new string('*', 69)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "PersonId", new[] { "Person ID cannot be longer than 68 characters." } }
                    }
                };

                object[] azureIdTooLong =
                {
                    new CreateMartialBaseUserDTO
                    {
                        AzureId = new string('*', 69)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "AzureId", new[] { "Azure ID cannot be longer than 68 characters." } }
                    }
                };

                object[] invitationCodeTooLong =
                {
                    new CreateMartialBaseUserDTO
                    {
                        InvitationCode = new string('*', 8)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "InvitationCode", new[] { "Invitation code cannot be longer than 7 characters." } }
                    }
                };

                var invalidCreateMartialBaseUserDTOs = new List<object[]>
                {
                    personIdTooLong,
                    azureIdTooLong,
                    invitationCodeTooLong
                };

                foreach ((CreatePersonDTO? CreatePersonDTO, Dictionary<string, string[]> Errors) invalidDTO
                         in CommonMemberData.InvalidCreatePersonDTOs
                             .Select(icpd => (icpd[0], icpd[1])))
                {
                    var invalidPersonErrors = new Dictionary<string, string[]>();

                    foreach (KeyValuePair<string, string[]> error in invalidDTO.Errors)
                    {
                        invalidPersonErrors.Add($"Person.{error.Key}", error.Value);
                    }

                    object[] invalidMartialBaseUserDTO =
                    {
                        new CreateMartialBaseUserDTO { Person = invalidDTO.CreatePersonDTO }, invalidPersonErrors
                    };

                    invalidCreateMartialBaseUserDTOs.Add(invalidMartialBaseUserDTO);
                }

                return invalidCreateMartialBaseUserDTOs;
            }
        }

        private string DbIdentifier => _fixture.DbIdentifier;

        [Fact]
        public async Task CanCreateMartialBaseUser()
        {
            // Arrange
            CreateMartialBaseUserDTO createMartialBaseUserDTO =
                DataGenerator.MartialBaseUsers.GenerateCreateMartialBaseUserDTOObject();

            // Act
            MartialBaseUserDTO createdMartialBaseUser =
                await MartialBaseUserController.CreateUserAsync(_fixture.Client, createMartialBaseUserDTO);

            // Assert
            MartialBaseUserResources.AssertEqual(createMartialBaseUserDTO, createdMartialBaseUser);

            MartialBaseUser dbMartialBaseUser = MartialBaseUserResources.GetUser(
                new Guid(createdMartialBaseUser.Id),
                DbIdentifier);

            MartialBaseUserResources.AssertEqual(createMartialBaseUserDTO, dbMartialBaseUser);
        }

        [Fact]
        public async Task CreateMartialBaseUserWithInvalidPersonIdReturnsNotFound()
        {
            // Arrange
            CreateMartialBaseUserDTO createMartialBaseUserDTO =
                DataGenerator.MartialBaseUsers.GenerateCreateMartialBaseUserDTOObject();

            createMartialBaseUserDTO.PersonId = Guid.NewGuid().ToString();
            createMartialBaseUserDTO.Person = null;

            // Act
            var response = await MartialBaseUserController.GetCreateUserResponseAsync(
                _fixture.Client, createMartialBaseUserDTO);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorResponseCode.None, response.ErrorResponseCode);
            Assert.Equal($"Person ID '{createMartialBaseUserDTO.PersonId}' not found.", response.ResponseBody);
        }

        [Theory]
        [MemberData(nameof(InvalidCreateMartialBaseUserDTOs))]
        public async Task CreateMartialBaseUserWithInvalidCreateDTOReturnsInternalServerError(
            CreateMartialBaseUserDTO invalidCreateMartialBaseUserDTO,
            Dictionary<string, string[]> expectedModelStateErrors)
        {
            // Arrange
            // Act
            var response = await MartialBaseUserController.GetCreateUserResponseAsync(
                _fixture.Client, invalidCreateMartialBaseUserDTO);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Dictionary<string, string[]> modelStateErrors = ModelStateTools.ParseModelStateErrors(response.ResponseBody);

            DictionaryResources.AssertEqual(expectedModelStateErrors, modelStateErrors);
        }
    }
#else
    [Collection("LiveControllerTests")]
    public class CreateUserTests
    {
        public CreateUserTests()
        {
        }

        [Fact]
        public void CanCreateMartialBaseUser()
        {
            return;
        }
    }
#endif
}
