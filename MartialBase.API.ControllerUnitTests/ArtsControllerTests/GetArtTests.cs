// <copyright file="GetArtTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Controllers;
using MartialBase.API.ControllerUnitTests.TestControllerInstances;
using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Exceptions;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Arts;
using MartialBase.API.TestTools.TestResources;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.ControllerUnitTests.ArtsControllerTests
{
    public class GetArtTests
    {
        private IArtsRepository _artsRepository;
        private IMartialBaseUserHelper _martialBaseUserHelper;
        private IAzureUserHelper _azureUserHelper;
        private ArtsControllerInstance _artsControllerInstance;
        private MartialBaseUser _testUser;

        private ArtsController ArtsController => _artsControllerInstance.Instance;

        [SetUp]
        public void Setup()
        {
            _artsRepository = Substitute.For<IArtsRepository>();
            _martialBaseUserHelper = Substitute.For<IMartialBaseUserHelper>();
            _azureUserHelper = Substitute.For<IAzureUserHelper>();

            _testUser =
                DataGenerator.MartialBaseUsers.GenerateMartialBaseUserObject();

            _artsControllerInstance =
                new ArtsControllerInstance(
                    _artsRepository,
                    _martialBaseUserHelper,
                    _azureUserHelper,
                    "Live",
                    _testUser);

            _artsControllerInstance.SetTestUserRole(UserRoles.User);
        }

        [Test]
        public async Task CanGetArt()
        {
            // Arrange
            ArtDTO testArt = DataGenerator.Arts.GenerateArtDTO();

            _artsRepository
                .ExistsAsync(new Guid(testArt.Id))
                .Returns(true);

            _artsRepository
                .GetAsync(new Guid(testArt.Id))
                .Returns(testArt);

            // Act
            IActionResult result = await ArtsController.GetArtAsync(testArt.Id);
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<ArtDTO>());

            var retrievedArt = (ArtDTO)objectResult.Value;

            ArtResources.AssertEqual(testArt, retrievedArt);
        }

        [Test]
        public async Task GetNonExistentArtReturnsNotFoundResult()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;
            var invalidArtId = Guid.NewGuid();

            _artsRepository
                .ExistsAsync(invalidArtId)
                .Returns(false);

            // Act
            try
            {
                await ArtsController.GetArtAsync(invalidArtId.ToString());
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Art ID '{invalidArtId}' not found.", caughtException.Message);
        }
    }
}
